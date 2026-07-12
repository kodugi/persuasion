using System.IO;
using UnityEngine;
using SFB;

namespace FileUtils
{
    public static class JsonFileUtils
    {
        public static string OpenSingleJsonFile()
        {
            ExtensionFilter[] extensions = new[]
            {
                new ExtensionFilter("JSON", "json")
            };

            string[] paths = StandaloneFileBrowser.OpenFilePanel("파일 선택", "", extensions, false);

            if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
            {
                return File.ReadAllText(paths[0]);
            }
            
            Debug.LogError("selected file is not valid");
            return "";
        }

        public static void SaveJsonFile(string content)
        {
            string path = StandaloneFileBrowser.SaveFilePanel("파일 저장", "", "새 퍼즐", "json");
            if (!string.IsNullOrEmpty(path))
            {
                File.WriteAllText(path, content);
            }
        }
    }
}