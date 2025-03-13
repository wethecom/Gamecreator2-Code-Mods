using UnityEditor;
using UnityEngine;
using System.Text.RegularExpressions;
using System;
using System.IO;

public class ClipboardScriptCreator
{
    [MenuItem("Assets/Create/Script From Clipboard", false, 10)]
    public static void CreateScriptFromClipboard()
    {
        string clipboardContent = EditorGUIUtility.systemCopyBuffer;

        if (string.IsNullOrEmpty(clipboardContent))
        {
            Debug.Log("Clipboard is empty");
            return;
        }

        // Try to find the class name using regex
        string className = ExtractClassName(clipboardContent);

        if (string.IsNullOrEmpty(className))
        {
            Debug.Log("No valid C# class found in clipboard");
            return;
        }

        // Get the currently selected folder in the Project window
        string selectedPath = "Assets";
        if (Selection.activeObject != null)
        {
            selectedPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (Directory.Exists(selectedPath))
            {
                selectedPath = selectedPath.TrimEnd('/');
            }
            else
            {
                selectedPath = Path.GetDirectoryName(selectedPath);
            }
        }

        // Create the script file
        string path = $"{selectedPath}/{className}.cs";

        // Check if file already exists
        if (System.IO.File.Exists(path))
        {
            if (!EditorUtility.DisplayDialog("File Exists",
                $"The file {className}.cs already exists. Do you want to overwrite it?",
                "Yes", "No"))
            {
                return;
            }
        }

        try
        {
            System.IO.File.WriteAllText(path, clipboardContent);
            AssetDatabase.Refresh();
            Debug.Log($"Created script: {path}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error creating script: {e.Message}");
        }
    }

    private static string ExtractClassName(string content)
    {
        // This regex looks for class declarations
        // It handles public/private/internal class declarations
        // And supports classes that inherit from other classes
        var regex = new Regex(@"(?:public|private|protected|internal|\s)*\s+class\s+(\w+)");
        var match = regex.Match(content);

        if (match.Success)
        {
            return match.Groups[1].Value;
        }

        return null;
    }
}
