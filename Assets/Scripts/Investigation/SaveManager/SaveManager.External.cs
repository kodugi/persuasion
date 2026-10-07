using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Investigation;
using Unity.VisualScripting;

public partial class SaveManager : MonoBehaviour
{
    public static void ResetAllSaveData()
    {
        _ResetAllSaveData();
    }
    public bool TryLoadData<T>(string fileName, out T result) where T : new()
    {
        return _TryLoadData<T>(fileName, out result);
    }
    /// <summary>
    /// if source is... 
    /// <para> general: additionalPath, data - not required </para>
    /// <para> progress: additionalPath - not required, data - required </para>
    /// <para> notes: additionalPath, data - not required </para>
    /// <para> inventory: additionalPath, data - not required </para>
    /// </summary>
    /// <param name="source">general, progress, notes, inventory</param>
    /// <param name="additionalPath"></param>
    /// <param name="data"></param>
    public void SaveData(string source, string additionalPath="", object data=null)
    {
        switch (source)
        {
            case "general":
                _SaveData("general", (data==null)?generalSave:data);
                break;
            default:
                _SaveData(source, data);
                break;
        }
    }
}
