using UnityEngine;
using System.IO;

public class Inv_Temp_Reseter : MonoBehaviour
{
    private string PathGen(string fileName)
    {
        string saveFilePath = Path.Combine(Application.persistentDataPath, fileName + ".json");
        return saveFilePath;
    }
    public void ResetData(string fileName)
    {
        string path = PathGen(fileName);

        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("Deleted save file: " + path);
        }
        else
        {
            Debug.Log("Save file does not exist: " + path);
        }
    }
}
