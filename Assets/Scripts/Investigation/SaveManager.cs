using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Investigation;

public partial class SaveManager : MonoBehaviour
{
    [SerializeField]
    public bool resetOnQuit = true;
    [SerializeField]
    public bool saveWhilePlaying = true;

    private Inv_GameManager gameManager;

    public Dictionary<string, object> progress =
        new Dictionary<string, object>();

    public static SaveManager Instance { get; private set; }
    public bool isProgressLoaded = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void OnInvestigationSceneStart()
    {
        gameManager = GameObject.FindFirstObjectByType<Inv_GameManager>();
        //print("??");
        if (LoadData(
            "progress",
            out Dictionary<string, object> loadedProgress
        ))
        {
            progress = NormalizeProgressData(
                loadedProgress ?? new Dictionary<string, object>()
            );

            //EnsureProgressDefaults();
            InitializedBasedOnProgress();
        }
        else
        {
            Debug.Log(
                "[SaveManager] No progress file found. " +
                "Initialising default data."
            );

            InitializeEverything();
        }
        isProgressLoaded = true;
    }

    private void InitializeEverything()
    {
        Debug.Log("[SaveManager] InitializeEverything()");

        progress.Clear();
        progress["noteLock"] = true;

        SaveData("progress", progress);
        SaveData("notes", new Dictionary<string, object>());
        SaveData("inventory", new List<string>());

        InitializedBasedOnProgress();
    }
/*
    private void EnsureProgressDefaults()
    {
        bool changed = false;

        if (!progress.ContainsKey("noteLock"))
        {
            progress["noteLock"] = true;
            changed = true;
        }

        if (changed)
        {
            SaveProgress();
        }
    }
*/
    private void InitializedBasedOnProgress()
    {
        
        if (gameManager == null)
        {
            Debug.LogWarning(
                "[SaveManager] Inv_GameManager was not found."
            );

            return;
        }
        if(TryLoadProgress("noteLock", out object result))
        {
            bool result_b = (bool)result;
            gameManager.NoteLock(result_b);
        }
        
    }

    private void OnApplicationQuit()
    {
        Debug.Log(
            $"[SaveManager] OnApplicationQuit " +
            $"resetOnQuit={resetOnQuit}"
        );

        if (resetOnQuit)
        {
            // Set this before deleting anything so later save calls
            // cannot recreate the files.
            //isQuittingAndResetting = true;

            Debug.Log(
                "[SaveManager] Deleting save files on quit."
            );

            ResetAllSaveData();
            return;
        }
        else{
            Debug.Log(
                "[SaveManager] Saving progress on quit."
            );

            SaveProgress();
        }
    }

    private string PathGen(string fileName)
    {
        return Path.Combine(
            Application.persistentDataPath,
            fileName + ".json"
        );
    }

    public void SaveData(string fileName, object data)
    {
        if(!saveWhilePlaying) return;
        
        string path = PathGen(fileName);

        try
        {
            string json = JsonConvert.SerializeObject(
                data,
                Formatting.Indented
            );

            File.WriteAllText(path, json);

            Debug.Log(
                $"[SaveManager] Saved {fileName}: {path}"
            );
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"[SaveManager] Failed to save {fileName}: " +
                exception
            );
        }
    }

    public bool LoadData<T>(string fileName, out T result) where T : new()
    {
        string path = PathGen(fileName);
        /*
        Debug.Log(
            $"[SaveManager] Loading {fileName}: " +
            $"{path}, exists={File.Exists(path)}"
        );*/

        if (!File.Exists(path))
        {
            result = new T();

            Debug.LogWarning(
                "[SaveManager] File not found: " + path
            );

            return false;
        }

        try
        {
            string json = File.ReadAllText(path);

            result = JsonConvert.DeserializeObject<T>(json);

            if (result == null)
            {
                Debug.LogWarning(
                    $"[SaveManager] {fileName} contained null data."
                );

                result = new T();
                return false;
            }
            //print(JsonConvert.SerializeObject(result, Formatting.Indented));
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"[SaveManager] Failed to load {fileName}: " +
                exception
            );

            result = new T();
            return false;
        }
    }

    // overwrite=false works as AddRange only when both values
    // are List<object>.
    private object NormalizeProgressValue(object value)
    {
        if (value is JToken token)
        {
            if (token is JValue jValue)
            {
                return jValue.Value;
            }

            if (token is JArray jArray)
            {
                List<object> list = new List<object>();
                foreach (JToken item in jArray)
                {
                    list.Add(NormalizeProgressValue(item));
                }
                return list;
            }

            if (token is JObject jObject)
            {
                Dictionary<string, object> dict = new Dictionary<string, object>();
                foreach (var pair in jObject)
                {
                    dict[pair.Key] = NormalizeProgressValue(pair.Value);
                }
                return dict;
            }
        }

        return value;
    }

    private Dictionary<string, object> NormalizeProgressData(
        Dictionary<string, object> data
    )
    {
        Dictionary<string, object> normalized =
            new Dictionary<string, object>();

        foreach (var pair in data)
        {
            normalized[pair.Key] = NormalizeProgressValue(pair.Value);
        }

        return normalized;
    }

    public void AddProgress(
        string key,
        object value,
        bool overwrite = true
    )
    {
        value = NormalizeProgressValue(value);
        /*
        if (isQuittingAndResetting)
        {
            Debug.LogWarning(
                $"[SaveManager] Blocked AddProgress(\"{key}\") " +
                "because save data is being reset on quit."
            );

            return;
        }*/

        if (!progress.ContainsKey(key))
        {
            progress.Add(key, value);
        }
        else if (overwrite)
        {
            progress[key] = value;
        }
        else
        {
            if (
                progress[key] is List<object> existingList
                && value is List<object> newList
            )
            {
                existingList.AddRange(newList);
            }
            else
            {
                Debug.LogWarning(
                    "[SaveManager] Cannot add a non-list value " +
                    "to the existing key: " + key
                );

                return;
            }
        }

        SaveProgress();
        AddProgressException(key);
    }
    public bool TryLoadProgress(string key, out object result)
    {
        result = LoadProgress(key);
        return result!=null;
    }
    public object LoadProgress(string key)
    {
        if (progress.TryGetValue(key, out object value))
        {
            //print(JToken.FromObject(value).ToString(Formatting.Indented));
            return NormalizeProgressValue(value);
        }
        return null;
    }

    private void AddProgressException(string key)
    {
        switch (key)
        {
            case "notePossessed":
            case "penPossessed":
                bool notePossessed = Convert.ToBoolean(
                    LoadProgress("notePossessed") ?? false
                );

                bool penPossessed = Convert.ToBoolean(
                    LoadProgress("penPossessed") ?? false
                );

                if (notePossessed && penPossessed)
                {
                    AddProgress("noteLock", false);

                    if (gameManager != null)
                    {
                        gameManager.NoteLock(false);
                    }
                }

                break;
        }
    }

    /// <summary>
    /// Deletes every save file without recreating default files.
    /// </summary>
    public void ResetAllSaveData()
    {
        progress.Clear();

        DeleteSaveFile("progress");
        DeleteSaveFile("notes");
        DeleteSaveFile("inventory");

        Debug.Log(
            "[SaveManager] Reset completed.\n" +
            $"Progress exists: {File.Exists(PathGen("progress"))}\n" +
            $"Notes exists: {File.Exists(PathGen("notes"))}\n" +
            $"Inventory exists: {File.Exists(PathGen("inventory"))}"
        );
    }

    private void DeleteSaveFile(string fileName)
    {
        string path = PathGen(fileName);

        if (!File.Exists(path))
        {
            Debug.Log(
                $"[SaveManager] File was already absent: {path}"
            );

            return;
        }

        try
        {
            File.Delete(path);

            Debug.Log(
                $"[SaveManager] Deleted {fileName}: {path}, " +
                $"still exists={File.Exists(path)}"
            );
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"[SaveManager] Failed to delete {fileName}: " +
                exception
            );
        }
    }

    public void SaveProgress()
    {
        SaveData("progress", progress);
    }

    public void SaveCharacterPosition(string mapID, string characterID, Vector3 position)
    {
        string key = mapID + "_positions";

        Dictionary<string, object> positions;

        object existing = LoadProgress(key);

        if (existing is Dictionary<string, object> existingPositions)
        {
            positions = existingPositions;
        }
        else
        {
            positions = new Dictionary<string, object>();
        }

        positions[characterID] = new Dictionary<string, object>
        {
            { "x", position.x },
            { "y", position.y },
            { "z", position.z }
        };

        AddProgress(key, positions);
    }
    public bool TryLoadCharacterPosition(string mapID, string characterID, out Vector3 position)
    {
        position = Vector3.zero;

        string key = mapID + "_positions";

        object data = LoadProgress(key);

        if (!(data is Dictionary<string, object> positions))
        {
            return false;
        }

        if (!positions.TryGetValue(characterID, out object characterData))
        {
            return false;
        }

        if (!(characterData is Dictionary<string, object> pos))
        {
            return false;
        }

        try
        {
            float x = Convert.ToSingle(pos["x"]);
            float y = Convert.ToSingle(pos["y"]);
            float z = Convert.ToSingle(pos["z"]);

            position = new Vector3(x, y, z);

            return true;
        }
        catch
        {
            return false;
        }
    }
    public Dictionary<string, Vector3> LoadAllCharacterPositions(string mapID)
    {
        print(mapID);
        Dictionary<string, Vector3> result =
            new Dictionary<string, Vector3>();

        string key = mapID + "_positions";

        object data = LoadProgress(key);
        
        if (!(data is Dictionary<string, object> positions))
        {
            return result;
        }

        foreach (var pair in positions)
        {
            string characterID = pair.Key;
            print(characterID);
            if (!(pair.Value is Dictionary<string, object> pos))
            {
                continue;
            }

            try
            {
                float x = Convert.ToSingle(pos["x"]);
                float y = Convert.ToSingle(pos["y"]);
                float z = Convert.ToSingle(pos["z"]);

                result[characterID] = new Vector3(x, y, z);
            }
            catch (Exception exception)
            {
                Debug.LogWarning(
                    $"[SaveManager] Failed to load position of " +
                    $"{characterID} in {mapID}: {exception.Message}"
                );
            }
        }

        return result;
    }
}