using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Investigation;

// Utilities
public partial class SaveManager : MonoBehaviour
{
    private string PathGen(string fileName)
    {
        if(savePath == null)
        {
            savePath = Path.Combine(Application.persistentDataPath,"SaveData");
        }
        Directory.CreateDirectory(savePath);
        
        if (fileName == "progress")
        {
            if(chiefManager == null || string.IsNullOrEmpty(chiefManager.inv_Scene_ID)) {
                Debug.LogError("chiefManager should not be null");
                return "";
            }
            fileName = "progress_"+chiefManager.inv_Scene_ID;
        }

        return Path.Combine(
            savePath,
            fileName + ".json"
        );
    }

    public object NormalizeValue(object value)
    {
        if (value == null) return null;

        if (value is JValue jValue)
            return jValue.Value;

        if (value is JObject jObject)
        {
            var result = new Dictionary<string, object>();

            foreach (JProperty property in jObject.Properties())
                result[property.Name] = NormalizeValue(property.Value);

            return result;
        }

        if (value is JArray jArray)
        {
            var result = new List<object>();

            foreach (JToken item in jArray)
                result.Add(NormalizeValue(item));

            return result;
        }

        if (value is IDictionary dictionary)
        {
            var result = new Dictionary<string, object>();

            foreach (DictionaryEntry entry in dictionary)
            {
                string key = entry.Key as string;
                if (key == null)
                    throw new InvalidOperationException("Save data dictionary keys must be strings.");

                result[key] = NormalizeValue(entry.Value);
            }

            return result;
        }

        if (value is IList list)
        {
            var result = new List<object>();

            foreach (object item in list)
                result.Add(NormalizeValue(item));

            return result;
        }

        return value;
    }
}
