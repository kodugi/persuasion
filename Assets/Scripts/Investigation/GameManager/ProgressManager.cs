using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Unity.Cinemachine;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

namespace Investigation
{
    public partial class GameManager : Utility
    {
        public bool IsProgressLoaded = false;
        private object NormalizeValue(object value)
        {
            return saveManager.NormalizeValue(value);
        }

        private Dictionary<string, object> progress =
            new Dictionary<string, object>();

        public void AddProgress(string key, object value, bool overwrite = true)
        {
            value = NormalizeValue(value);

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
                        "[ProgressManager] Cannot add a non-list value " +
                        "to the existing key: " + key
                    );
                    return;
                }
            }

            SaveProgress();
        }
        public void SaveProgress()
        {
            saveManager.SaveData("progress", data:progress);
        }
        private void ProgressStart()
        {
            if (
                saveManager.TryLoadData("progress",out Dictionary<string, object> loadedProgress) 
                && saveManager.TryLoadData("general",out object _)
                )
            {
                progress = (Dictionary<string, object>)NormalizeValue(loadedProgress ?? new Dictionary<string, object>());
            }
            IsProgressLoaded = true;
        }

        public bool TryGetProgress(string key, out object result)
        {   
            bool _return = progress.TryGetValue(key, out result);
            return _return;
        }

        private string positionsKey = "positions";
        public void SaveCharacterPosition(string characterID, Vector3 position)
        {

            Dictionary<string, object> positions;

            object existing = null;
            TryGetProgress(positionsKey, out existing);

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

            AddProgress(positionsKey, positions);
        }
        public bool TryLoadCharacterPosition(string characterID, out Vector3 position)
        {
            position = Vector3.zero;

            object data;
            TryGetProgress(positionsKey, out data);

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
        public Dictionary<string, Vector3> LoadAllCharacterPositions()
        {
            Dictionary<string, Vector3> result =
                new Dictionary<string, Vector3>();

            object data;
            TryGetProgress(positionsKey, out data);
            
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
                        $"[ProgressManager] Failed to load position of " +
                        $"{characterID}: {exception.Message}"
                    );
                }
            }

            return result;
        }
    }
}