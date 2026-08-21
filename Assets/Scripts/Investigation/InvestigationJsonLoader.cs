using System.IO;
using UnityEngine;

namespace Investigation
{
    /// <summary>
    /// Loads investigation JSON from the copy embedded in StreamingAssets.
    /// The editor fallback keeps Play Mode working without duplicating source files.
    /// </summary>
    internal static class InvestigationJsonLoader
    {
        private const string StreamingRoot = "Investigation/Dialogue";

        public static string LoadMap(string mapId)
        {
            return Load(Path.Combine("Maps", RequireId(mapId, nameof(mapId)) + ".json"));
        }

        public static string LoadDialogue(string dialogueId, int state)
        {
            string relativePath = Path.Combine(
                RequireId(dialogueId, nameof(dialogueId)),
                "Dialogue" + state + ".json"
            );

            return Load(relativePath);
        }

        private static string Load(string relativePath)
        {
            string streamingPath = Path.Combine(
                Application.streamingAssetsPath,
                StreamingRoot,
                relativePath
            );

            if (File.Exists(streamingPath))
            {
                return File.ReadAllText(streamingPath);
            }

#if UNITY_EDITOR
            string editorPath = Path.Combine(
                Application.dataPath,
                "Scripts",
                "Investigation",
                "Dialogue",
                relativePath
            );

            if (File.Exists(editorPath))
            {
                return File.ReadAllText(editorPath);
            }
#endif

            throw new FileNotFoundException(
                "Investigation JSON was not included in the player build. " +
                "Rebuild the player so InvestigationJsonBuildProcessor can add it " +
                "to StreamingAssets.",
                streamingPath
            );
        }

        private static string RequireId(string id, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new System.ArgumentException(
                    "An investigation JSON ID is required before loading a scene.",
                    parameterName
                );
            }

            return id;
        }
    }
}
