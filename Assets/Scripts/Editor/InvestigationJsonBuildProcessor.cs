using System.IO;
using UnityEditor.Build;
using UnityEngine;

/// <summary>
/// Includes the source investigation JSON files in every player build without
/// depending on the process working directory or copying editor scripts.
/// </summary>
public sealed class InvestigationJsonBuildProcessor : BuildPlayerProcessor
{
    private const string SourceRelativePath = "Scripts/Investigation/Dialogue";
    private const string StreamingRelativePath = "Investigation/Dialogue";

    public override int callbackOrder => 0;

    public override void PrepareForBuild(BuildPlayerContext buildPlayerContext)
    {
        string sourceRoot = Path.GetFullPath(
            Path.Combine(Application.dataPath, SourceRelativePath)
        );

        if (!Directory.Exists(sourceRoot))
        {
            throw new BuildFailedException(
                "Investigation JSON source directory was not found: " + sourceRoot
            );
        }

        string[] jsonPaths = Directory.GetFiles(
            sourceRoot,
            "*.json",
            SearchOption.AllDirectories
        );

        if (jsonPaths.Length == 0)
        {
            throw new BuildFailedException(
                "No investigation JSON files were found under: " + sourceRoot
            );
        }

        foreach (string jsonPath in jsonPaths)
        {
            string relativePath = jsonPath
                .Substring(sourceRoot.Length)
                .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .Replace(Path.DirectorySeparatorChar, '/');

            string streamingPath = StreamingRelativePath + "/" + relativePath;
            buildPlayerContext.AddAdditionalPathToStreamingAssets(
                jsonPath,
                streamingPath
            );
        }

        Debug.Log(
            $"[InvestigationJsonBuildProcessor] Added {jsonPaths.Length} JSON files " +
            "to StreamingAssets."
        );
    }
}
