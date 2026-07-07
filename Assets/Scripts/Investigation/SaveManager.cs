using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class SaveManager : MonoBehaviour
{
    public bool reseting = false;
    public Dictionary<string, object> progress = new Dictionary<string, object>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        progress = LoadData<Dictionary<string, object>>("progress");
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnApplicationQuit()
    {
        if (reseting) {
            ResetProgressData("progress");
            ResetProgressData("notes");
        }
        else SaveData("progress", progress);
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
    public T LoadData<T>(string fileName) where T : new()
    {        
        string path = PathGen(fileName);
        if (System.IO.File.Exists(path))
        {
            string json = System.IO.File.ReadAllText(path);
            return JsonConvert.DeserializeObject<T>(json);
        }
        else
        {
            Debug.LogWarning("File not found: " + path);
            return new T();
        }
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
    }

    //temp
    public void ResetProgressData(string fileName)
    {
        string path = PathGen(fileName);
        /*if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("Deleted save file: " + path);
        }*/
        SaveData(fileName, new Dictionary<string, object>());
    }
}
