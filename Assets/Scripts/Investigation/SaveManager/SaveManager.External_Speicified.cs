using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Investigation;

public partial class SaveManager : MonoBehaviour
{
    // Data-Specific Fuctions: General Data
    public void AddGeneralData(
        string key,
        object value,
        bool overwrite = true
    )
    {
        value = NormalizeValue(value);

        if (!generalSave.ContainsKey(key))
        {
            generalSave.Add(key, value);
        }
        else if (overwrite)
        {
            generalSave[key] = value;
        }
        else
        {
            if (
                generalSave[key] is List<object> existingList
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

        SaveData("general");
    }
    public bool TryGetGeneralData(string key, out object result)
    {
        bool _return = generalSave.TryGetValue(key, out result);
        return _return;
    }

}
