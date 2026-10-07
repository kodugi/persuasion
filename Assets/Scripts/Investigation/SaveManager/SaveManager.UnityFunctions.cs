using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Investigation;

public partial class SaveManager : MonoBehaviour
{
    // Unity Functions
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        IsQuittingAndResetting = false;

        chiefManager = FindFirstObjectByType<ChiefManager>();
        gameManager = FindFirstObjectByType<Investigation.GameManager>();

        if (_TryLoadData("general",out Dictionary<string, object> loadedData))
        {
            generalSave = (Dictionary<string, object>)NormalizeValue(loadedData ?? new Dictionary<string, object>());
            InitializedBasedOnSave();
        }
        else
        {
            generalSave = new Dictionary<string, object>();
            Debug.Log(
                "[SaveManager] No general save file found; Starting a new game. "
            );
            InitializeNewGame();
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
            IsQuittingAndResetting = true;

            Debug.Log(
                "[SaveManager] Deleting save files on quit."
            );

            _ResetAllSaveData();
            return;
        }
        else{
            /*
            Debug.Log(
                "[SaveManager] Saving progress on quit."
            );

            SaveProgress();*/
        }
    }
        private void InitializeNewGame()
        {
            Debug.Log("[SaveManager] InitializeEverything()");

            generalSave["noteLock"] = false;

            SaveData("general", data:generalSave);
            SaveData("notes", data:new Dictionary<string, object>());
            SaveData("inventory", data:new List<string>());

            InitializedBasedOnSave();
        }
    
        private void InitializedBasedOnSave()
        {
            // pass
        }
}
