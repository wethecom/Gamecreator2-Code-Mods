using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GameCreator.Editor.Tools
{
    public class AsmdefFishNetFixer : EditorWindow
    {
        private const string FISHNET_REFERENCE = "FishNet.Runtime";
        private const string GAMECREATOR_PATH = "Assets/Plugins/GameCreator";
        
        private Vector2 m_ScrollPosition;
        private List<AsmdefInfo> m_AsmdefFiles = new List<AsmdefInfo>();
        private bool m_Scanned = false;
        
        private class AsmdefInfo
        {
            public string Path;
            public string Name;
            public bool HasFishNet;
            public bool Selected;
        }
        
        [MenuItem("Tools/Game Creator/FishNet Asmdef Fixer")]
        public static void ShowWindow()
        {
            var window = GetWindow<AsmdefFishNetFixer>("Asmdef FishNet Fixer");
            window.minSize = new Vector2(500, 400);
        }
        
        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("FishNet Assembly Reference Fixer", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "This tool scans all .asmdef files in GameCreator folders and adds FishNet.Runtime reference if missing.",
                MessageType.Info
            );
            EditorGUILayout.Space(10);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Scan Asmdef Files", GUILayout.Height(30)))
            {
                ScanAsmdefFiles();
            }
            
            EditorGUI.BeginDisabledGroup(!m_Scanned || m_AsmdefFiles.Count == 0);
            if (GUILayout.Button("Fix Selected", GUILayout.Height(30)))
            {
                FixSelectedAsmdefs();
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(10);
            
            if (m_Scanned && m_AsmdefFiles.Count > 0)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Select All Missing"))
                {
                    foreach (var info in m_AsmdefFiles)
                    {
                        if (!info.HasFishNet) info.Selected = true;
                    }
                }
                if (GUILayout.Button("Deselect All"))
                {
                    foreach (var info in m_AsmdefFiles)
                    {
                        info.Selected = false;
                    }
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space(5);
                
                // Summary
                int missing = 0;
                int hasRef = 0;
                foreach (var info in m_AsmdefFiles)
                {
                    if (info.HasFishNet) hasRef++;
                    else missing++;
                }
                
                EditorGUILayout.LabelField($"Found {m_AsmdefFiles.Count} asmdef files: {hasRef} have FishNet, {missing} missing");
                
                EditorGUILayout.Space(5);
                
                // List
                m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);
                
                foreach (var info in m_AsmdefFiles)
                {
                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                    
                    EditorGUI.BeginDisabledGroup(info.HasFishNet);
                    info.Selected = EditorGUILayout.Toggle(info.Selected, GUILayout.Width(20));
                    EditorGUI.EndDisabledGroup();
                    
                    EditorGUILayout.LabelField(info.Name, GUILayout.Width(300));
                    
                    if (info.HasFishNet)
                    {
                        GUI.color = Color.green;
                        EditorGUILayout.LabelField("✓ Has FishNet");
                        GUI.color = Color.white;
                    }
                    else
                    {
                        GUI.color = Color.yellow;
                        EditorGUILayout.LabelField("✗ Missing FishNet");
                        GUI.color = Color.white;
                    }
                    
                    EditorGUILayout.EndHorizontal();
                }
                
                EditorGUILayout.EndScrollView();
            }
            else if (m_Scanned)
            {
                EditorGUILayout.HelpBox("No asmdef files found in GameCreator folder.", MessageType.Warning);
            }
        }
        
        private void ScanAsmdefFiles()
        {
            m_AsmdefFiles.Clear();
            m_Scanned = true;
            
            if (!Directory.Exists(GAMECREATOR_PATH))
            {
                Debug.LogError($"GameCreator folder not found at: {GAMECREATOR_PATH}");
                return;
            }
            
            string[] asmdefPaths = Directory.GetFiles(GAMECREATOR_PATH, "*.asmdef", SearchOption.AllDirectories);
            
            foreach (string path in asmdefPaths)
            {
                string normalizedPath = path.Replace("\\", "/");
                string json = File.ReadAllText(normalizedPath);
                
                var info = new AsmdefInfo
                {
                    Path = normalizedPath,
                    Name = Path.GetFileNameWithoutExtension(normalizedPath),
                    HasFishNet = json.Contains(FISHNET_REFERENCE),
                    Selected = false
                };
                
                m_AsmdefFiles.Add(info);
            }
            
            Debug.Log($"Scanned {m_AsmdefFiles.Count} asmdef files");
        }
        
        private void FixSelectedAsmdefs()
        {
            int fixedCount = 0;
            
            foreach (var info in m_AsmdefFiles)
            {
                if (!info.Selected || info.HasFishNet) continue;
                
                if (AddFishNetReference(info.Path))
                {
                    info.HasFishNet = true;
                    info.Selected = false;
                    fixedCount++;
                }
            }
            
            if (fixedCount > 0)
            {
                AssetDatabase.Refresh();
                Debug.Log($"Fixed {fixedCount} asmdef files. Unity will recompile.");
            }
            else
            {
                Debug.Log("No files needed fixing.");
            }
        }
        
        private bool AddFishNetReference(string path)
        {
            try
            {
                string json = File.ReadAllText(path);
                
                // Simple JSON manipulation - find "references" array and add to it
                // This handles the common asmdef format
                
                if (json.Contains("\"references\""))
                {
                    // Has references array - add to it
                    int refIndex = json.IndexOf("\"references\"");
                    int bracketStart = json.IndexOf("[", refIndex);
                    int bracketEnd = json.IndexOf("]", bracketStart);
                    
                    string beforeBracket = json.Substring(0, bracketStart + 1);
                    string insideBracket = json.Substring(bracketStart + 1, bracketEnd - bracketStart - 1).Trim();
                    string afterBracket = json.Substring(bracketEnd);
                    
                    string newReference = $"\"GUID:{GetFishNetGuid()}\"";
                    
                    // Also try assembly name format
                    if (string.IsNullOrEmpty(GetFishNetGuid()))
                    {
                        newReference = $"\"{FISHNET_REFERENCE}\"";
                    }
                    
                    string newInside;
                    if (string.IsNullOrEmpty(insideBracket))
                    {
                        newInside = "\n        " + newReference + "\n    ";
                    }
                    else
                    {
                        newInside = insideBracket + ",\n        " + newReference;
                    }
                    
                    json = beforeBracket + newInside + afterBracket;
                }
                else
                {
                    // No references array - add one before the closing brace
                    int lastBrace = json.LastIndexOf("}");
                    string newReference = $"\"{FISHNET_REFERENCE}\"";
                    string referencesArray = $",\n    \"references\": [\n        {newReference}\n    ]\n";
                    json = json.Substring(0, lastBrace) + referencesArray + "}";
                }
                
                File.WriteAllText(path, json);
                Debug.Log($"Added FishNet reference to: {path}");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to fix {path}: {e.Message}");
                return false;
            }
        }
        
        private string GetFishNetGuid()
        {
            // Try to find FishNet.Runtime asmdef and get its GUID
            string[] guids = AssetDatabase.FindAssets("FishNet.Runtime t:asmdef");
            if (guids.Length > 0)
            {
                return guids[0];
            }
            
            // Also try searching by file name
            string[] paths = Directory.GetFiles("Assets", "FishNet.Runtime.asmdef", SearchOption.AllDirectories);
            if (paths.Length > 0)
            {
                return AssetDatabase.AssetPathToGUID(paths[0].Replace("\\", "/"));
            }
            
            return "";
        }
    }
}
