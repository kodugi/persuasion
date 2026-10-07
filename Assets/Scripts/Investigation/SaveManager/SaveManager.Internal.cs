using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Investigation;

public partial class SaveManager : MonoBehaviour
{
    // Accessing Raw Saved Data Files
    private bool _SaveData(string fileName, object data)
    {
        if (!saveWhilePlaying) return true;
        
        string path = PathGen(fileName);
        try
        {
            string json = JsonConvert.SerializeObject(data,Formatting.Indented);
            File.WriteAllText(path, json);
            Debug.Log($"[SaveManager] Saved {fileName}: {path}");
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError($"[SaveManager] Failed to save {fileName}: " + exception);
            return false;
        }
    }

    private bool _TryLoadData<T>(string fileName, out T result) where T : new()
    {
        string path = PathGen(fileName);
        // Debug.Log($"[SaveManager] Loading {fileName}: " + $"{path}, exists={File.Exists(path)}");
        if (!File.Exists(path))
        {
            result = new T();
            Debug.LogWarning("[SaveManager] File not found: " + path);
            return false;
        }
        try
        {
            string json = File.ReadAllText(path);
            result = JsonConvert.DeserializeObject<T>(json);
            if (result == null)
            {
                Debug.LogWarning($"[SaveManager] {fileName} contained null data.");
                result = new T();
                return false;
            }
            //print(JsonConvert.SerializeObject(result, Formatting.Indented));
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError($"[SaveManager] Failed to load {fileName}: " + exception);

            result = new T();
            return false;
        }
    }

    private static void _ResetAllSaveData()
    {
        SaveManager instance = Instance;
        if (instance == null)
        {
            Debug.LogWarning("[SaveManager] Cannot reset: instance is missing.");
            return;
        }

        var gameManager = FindFirstObjectByType<Investigation.GameManager>();
        if (gameManager != null)
            gameManager.ClearSaveData();

        instance.generalSave.Clear();

        if (string.IsNullOrEmpty(instance.savePath) ||
            !Directory.Exists(instance.savePath))
        {
            Debug.Log("[SaveManager] No save directory to clear.");
            return;
        }

        try
        {
            foreach (string path in Directory.EnumerateFiles(
                        instance.savePath, "*.json", SearchOption.TopDirectoryOnly))
            {
                _DeleteSaveFile(Path.GetFileNameWithoutExtension(path));
            }
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"[SaveManager] Failed to enumerate save files: {exception}"
            );
        }
        Debug.Log("[SaveManager] Successfully Reset");
    }

    private static void _DeleteSaveFile(string fileName)
    {
        string path = Instance.PathGen(fileName);

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

}
