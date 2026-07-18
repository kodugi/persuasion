using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Investigation;
using System;

public partial class SaveManager : MonoBehaviour
{
    [SerializeField] bool reseting=true;
    private Inv_GameManager gameManager;
    public Dictionary<string, object> progress = new Dictionary<string, object>();
    public static SaveManager Instance { get; private set; }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void OnInvestigationSceneStart()
    {
        gameManager = GameObject.FindFirstObjectByType<Inv_GameManager>();
        if(LoadData<Dictionary<string, object>>("progress", out Dictionary<string, object> result))
        {
            progress = result;
            InitializedBasedOnProgress();
        }
        else
        {
            progress = result;
            InitializeEverything();
        }
    }
    private void InitializeEverything()
    {
        SaveData("notes", new Dictionary<string, object>());
        SaveData("inventory", new List<string>());
        //Progress Initialization
        progress["noteLock"] = true;
        SaveData("progress", progress);
        InitializedBasedOnProgress();
    }
    private void InitializedBasedOnProgress()
    {
        gameManager.NoteLock((bool)progress["noteLock"]);
    }
    void OnApplicationQuit()
    {
        if (reseting) {
            ResetProgressData<Dictionary<string, object>>("progress");
            ResetProgressData<Dictionary<string, object>>("notes");
            ResetProgressData<List<string>>("inventory");
        }
        else {
            SaveData("progress", progress);
        }
    }
    private string PathGen(string fileName)
    {
        string path = Path.Combine(Application.persistentDataPath, fileName + ".json");
        return path;
    }
    public void SaveData(string fileName, object data)
    {
        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        System.IO.File.WriteAllText(PathGen(fileName), json);
    }
    public bool LoadData<T>(string fileName, out T result) where T : new()
    {        
        string path = PathGen(fileName);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            result = JsonConvert.DeserializeObject<T>(json);
            return true;
        }
        Debug.LogWarning("File not found: " + path);
        result = new T();
        return false;
    }

    // overwrite=false는 object타입이 List일때만: add로 작용
    public void AddProgress(string key, object value, bool overwrite = true)
    {
        if (!progress.ContainsKey(key))
        {
            progress.Add(key, value);
        }
        else
        {
            if(overwrite)
            {
                progress[key] = value;
            }
            else
            {
                if (progress[key] is List<object> existingList && value is List<object> newList)
                {
                    existingList.AddRange(newList);
                }
                else
                {
                    Debug.LogWarning("Cannot add non-list value to existing key: " + key);
                }
            }
        }
        SaveData("progress", progress);
        AddProgressException(key, value);
    }
    public object LoadProgress(string key)
    {
        if (progress.ContainsKey(key))
        {
            return progress[key];
        }
        else
        {
            return null;
        }
    }
    void AddProgressException(string key, object value)
    {
        switch (key)
        {
            case "notePossessed":
            case "penPossessed":
                if(Convert.ToBoolean(LoadProgress("notePossessed") ?? false) && Convert.ToBoolean(LoadProgress("penPossessed") ?? false))
                {
                    AddProgress("noteLock", false);
                    gameManager.NoteLock(false);
                }
                break;
            default:
                return;
        }
    }

    //temp
    public void ResetProgressData<T>(string fileName)where T : new()
    {
        string path = PathGen(fileName);
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("Deleted save file: " + path);
        }
        //SaveData(fileName, new T());
    }
}
