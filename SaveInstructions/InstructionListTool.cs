using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;
using GameCreator.Editor.Common;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Editor.VisualScripting
{
    public class InstructionListTool : TPolymorphicListTool
    {
        private const string NAME_BUTTON_ADD = "GC-Instruction-List-Foot-Add";
        private const string CLASS_INSTRUCTION_RUNNING = "gc-list-item-head-running";
        
        // Default folder for presets (relative to Assets)
        private const string DEFAULT_PRESET_FOLDER = "Assets/GameCreator/InstructionPresets";

        private static readonly IIcon ICON_PASTE = new IconPaste(ColorTheme.Type.TextNormal);
        private static readonly IIcon ICON_PLAY = new IconPlay(ColorTheme.Type.TextNormal);

        // MEMBERS: -------------------------------------------------------------------------------

        [NonSerialized] protected Button m_ButtonAdd;
        [NonSerialized] protected Button m_ButtonPaste;
        [NonSerialized] protected Button m_ButtonPlay;
        [NonSerialized] protected Button m_ButtonCopySelected;
        [NonSerialized] protected Button m_ButtonPasteSelected;
        [NonSerialized] protected Button m_ButtonDeleteSelected;
        [NonSerialized] protected Button m_ButtonSavePreset;
        [NonSerialized] protected Button m_ButtonLoadPreset;

        [NonSerialized] private readonly BaseActions m_BaseActions;
        [NonSerialized] private IVisualElementScheduledItem m_UpdateScheduler;
        [NonSerialized] private readonly HashSet<int> m_SelectedIndices = new HashSet<int>();
        
        // PROPERTIES: ----------------------------------------------------------------------------

        protected override string ElementNameHead => "GC-Instruction-List-Head";
        protected override string ElementNameBody => "GC-Instruction-List-Body";
        protected override string ElementNameFoot => "GC-Instruction-List-Foot";

        protected override List<string> CustomStyleSheetPaths => new List<string>
        {
            EditorPaths.VISUAL_SCRIPTING + "Instructions/StyleSheets/Instructions-List"
        };

        public override bool AllowReordering => true;
        public override bool AllowDuplicating => true;
        public override bool AllowDeleting  => true;
        public override bool AllowContextMenu => true;
        public override bool AllowCopyPaste => true;
        public override bool AllowInsertion => true;
        public override bool AllowBreakpoint => true;
        public override bool AllowDisable => true;
        public override bool AllowDocumentation => true;

        // CONSTRUCTOR: ---------------------------------------------------------------------------

        public InstructionListTool(SerializedProperty property)
            : base(property, InstructionListDrawer.NAME_INSTRUCTIONS)
        {
            this.m_BaseActions = property.serializedObject.targetObject as BaseActions;
            
            this.RegisterCallback<AttachToPanelEvent>(this.OnAttachPanel);
            this.RegisterCallback<DetachFromPanelEvent>(this.OnDetachPanel);
        }
        
        // SCHEDULER METHODS: ---------------------------------------------------------------------

        private void OnAttachPanel(AttachToPanelEvent attachEvent)
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (this.m_BaseActions == null) return;
            
            if (this.m_UpdateScheduler != null) return;
            this.m_UpdateScheduler = this.schedule.Execute(this.OnUpdate).Every(0);
        }

        private void OnDetachPanel(DetachFromPanelEvent detachEvent)
        {
            this.m_UpdateScheduler?.Pause();
        }
        
        private void OnUpdate()
        {
            if (this.m_Property.propertyPath != BaseActionsEditor.NAME_INSTRUCTIONS) return;
            
            foreach (VisualElement child in this.m_Body.Children())
            {
                child.RemoveFromClassList(CLASS_INSTRUCTION_RUNNING);
            }
            
            if (this.m_BaseActions == null) return;
            int index = this.m_BaseActions.IsRunning ? this.m_BaseActions.RunningIndex : -1;
            
            if (this.m_Body.childCount <= index || index < 0) return;
            this.m_Body[index].AddToClassList(CLASS_INSTRUCTION_RUNNING);
        }
        
        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override VisualElement MakeItemTool(int index)
        {
            return new InstructionItemTool(this, index);
        }

        protected override void SetupHead()
        { }

        protected override void SetupFoot()
        {
            base.SetupFoot();

            this.m_ButtonAdd = new TypeSelectorElementInstruction(this.PropertyList, this)
            {
                name = NAME_BUTTON_ADD
            };

            this.m_ButtonPaste = new Button(() =>
            {
                if (!CopyPasteUtils.CanSoftPaste(typeof(Instruction))) return;

                int pasteIndex = this.PropertyList.arraySize;
                this.InsertItem(pasteIndex, CopyPasteUtils.SourceObjectCopy);
            })
            {
                name = "GC-Instruction-List-Foot-Button"
            };

            this.m_ButtonPaste.Add(new Image
            {
                image = ICON_PASTE.Texture
            });

            this.m_ButtonPlay = new Button(this.RunInstructions)
            {
                name = "GC-Instruction-List-Foot-Button"
            };

            this.m_ButtonPlay.Add(new Image
            {
                image = ICON_PLAY.Texture
            });

            this.m_ButtonCopySelected = new Button(this.CopySelected)
            {
                name = "GC-Instruction-List-Foot-Button",
                text = "📋Copy Sel"
            };
            this.m_ButtonCopySelected.style.minWidth = 60f;

            this.m_ButtonPasteSelected = new Button(this.PasteSelected)
            {
                name = "GC-Instruction-List-Foot-Button",
                text = "📄Paste Sel"
            };
            this.m_ButtonPasteSelected.style.minWidth = 60f;

            this.m_ButtonDeleteSelected = new Button(this.DeleteSelected)
            {
                name = "GC-Instruction-List-Foot-Button",
                text = "🗑️ Delete",
                style = { minWidth = 60f }
            };

            // Save/Load Preset Buttons
            this.m_ButtonSavePreset = new Button(this.SavePreset)
            {
                name = "GC-Instruction-List-Foot-Button",
                text = "💾 Save",
                tooltip = "Save selected instructions to a preset asset"
            };
            this.m_ButtonSavePreset.style.minWidth = 55f;

            this.m_ButtonLoadPreset = new Button(this.LoadPreset)
            {
                name = "GC-Instruction-List-Foot-Button",
                text = "📂 Load",
                tooltip = "Load instructions from a preset asset"
            };
            this.m_ButtonLoadPreset.style.minWidth = 55f;

            // Add all buttons to the footer in the desired order
            this.m_Foot.Add(this.m_ButtonAdd);
            this.m_Foot.Add(this.m_ButtonCopySelected);
            this.m_Foot.Add(this.m_ButtonPasteSelected);
            this.m_Foot.Add(this.m_ButtonDeleteSelected);
            this.m_Foot.Add(this.m_ButtonSavePreset);
            this.m_Foot.Add(this.m_ButtonLoadPreset);
            this.m_Foot.Add(this.m_ButtonPaste);
            this.m_Foot.Add(this.m_ButtonPlay);

            this.m_ButtonPlay.SetEnabled(EditorApplication.isPlayingOrWillChangePlaymode);
            this.m_ButtonPlay.style.display = this.SerializedObject?.targetObject as BaseActions != null
                ? DisplayStyle.Flex
                : DisplayStyle.None;

            this.UpdateFooterState();
        }

        // PRESET SAVE/LOAD METHODS: --------------------------------------------------------------

        private void SavePreset()
        {
            this.NormalizeSelection();
            
            // Determine what to save: selected items or all items
            List<int> indicesToSave;
            if (this.m_SelectedIndices.Count > 0)
            {
                indicesToSave = this.m_SelectedIndices.OrderBy(i => i).ToList();
            }
            else
            {
                // If nothing selected, save all instructions
                indicesToSave = Enumerable.Range(0, this.PropertyList.arraySize).ToList();
            }

            if (indicesToSave.Count == 0)
            {
                EditorUtility.DisplayDialog("Save Preset", "No instructions to save.", "OK");
                return;
            }

            // Ensure default folder exists
            EnsurePresetFolderExists();

            // Show save file dialog
            string path = EditorUtility.SaveFilePanelInProject(
                "Save Instruction Preset",
                "NewInstructionPreset",
                "asset",
                "Choose a location to save the instruction preset",
                DEFAULT_PRESET_FOLDER
            );

            if (string.IsNullOrEmpty(path)) return;

            // Create or overwrite the preset asset
            InstructionPreset preset = AssetDatabase.LoadAssetAtPath<InstructionPreset>(path);
            bool isNewAsset = preset == null;
            
            if (isNewAsset)
            {
                preset = ScriptableObject.CreateInstance<InstructionPreset>();
            }

            preset.ClearInstructions();
            preset.SetPresetName(System.IO.Path.GetFileNameWithoutExtension(path));

            // Serialize each instruction
            foreach (int index in indicesToSave)
            {
                SerializedProperty element = this.PropertyList.GetArrayElementAtIndex(index);
                Instruction instruction = element.GetValue<Instruction>();
                
                if (instruction != null)
                {
                    string typeFullName = instruction.GetType().AssemblyQualifiedName;
                    string jsonData = EditorJsonUtility.ToJson(instruction);
                    preset.AddInstruction(typeFullName, jsonData);
                }
            }

            if (isNewAsset)
            {
                AssetDatabase.CreateAsset(preset, path);
            }
            else
            {
                EditorUtility.SetDirty(preset);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Highlight the asset in the project window
            EditorGUIUtility.PingObject(preset);

            Debug.Log($"[InstructionPreset] Saved {indicesToSave.Count} instruction(s) to: {path}");
        }

        private void LoadPreset()
        {
            // Show open file dialog
            string path = EditorUtility.OpenFilePanel(
                "Load Instruction Preset",
                GetPresetFolderPath(),
                "asset"
            );

            if (string.IsNullOrEmpty(path)) return;

            // Convert absolute path to relative project path
            if (path.StartsWith(Application.dataPath))
            {
                path = "Assets" + path.Substring(Application.dataPath.Length);
            }

            InstructionPreset preset = AssetDatabase.LoadAssetAtPath<InstructionPreset>(path);
            
            if (preset == null)
            {
                EditorUtility.DisplayDialog("Load Preset", 
                    "Could not load the selected file. Make sure it's an InstructionPreset asset.", "OK");
                return;
            }

            if (preset.Instructions.Count == 0)
            {
                EditorUtility.DisplayDialog("Load Preset", "The preset contains no instructions.", "OK");
                return;
            }

            // Ask user how to load
            int choice = EditorUtility.DisplayDialogComplex(
                "Load Instruction Preset",
                $"Loading '{preset.PresetName}' with {preset.Instructions.Count} instruction(s).\n\nHow would you like to add them?",
                "Append",      // 0
                "Cancel",      // 1
                "Replace All"  // 2
            );

            if (choice == 1) return; // Cancel

            this.SerializedObject.Update();

            // If replacing, clear existing instructions
            if (choice == 2)
            {
                while (this.PropertyList.arraySize > 0)
                {
                    this.DeleteItem(0);
                }
            }

            // Load instructions from preset
            int insertIndex = this.PropertyList.arraySize;
            int loadedCount = 0;

            foreach (var serializedInstruction in preset.Instructions)
            {
                try
                {
                    Type type = Type.GetType(serializedInstruction.TypeFullName);
                    if (type == null)
                    {
                        Debug.LogWarning($"[InstructionPreset] Could not find type: {serializedInstruction.TypeFullName}");
                        continue;
                    }

                    Instruction instance = (Instruction)Activator.CreateInstance(type);
                    EditorJsonUtility.FromJsonOverwrite(serializedInstruction.JsonData, instance);
                    
                    this.InsertItem(insertIndex, instance);
                    insertIndex++;
                    loadedCount++;
                }
                catch (Exception e)
                {
                    Debug.LogError($"[InstructionPreset] Failed to load instruction: {e.Message}");
                }
            }

            this.ClearSelection();
            this.UpdateFooterState();

            Debug.Log($"[InstructionPreset] Loaded {loadedCount} instruction(s) from: {path}");
        }

        private static void EnsurePresetFolderExists()
        {
            if (!AssetDatabase.IsValidFolder(DEFAULT_PRESET_FOLDER))
            {
                string[] folders = DEFAULT_PRESET_FOLDER.Split('/');
                string currentPath = folders[0]; // "Assets"

                for (int i = 1; i < folders.Length; i++)
                {
                    string newPath = currentPath + "/" + folders[i];
                    if (!AssetDatabase.IsValidFolder(newPath))
                    {
                        AssetDatabase.CreateFolder(currentPath, folders[i]);
                    }
                    currentPath = newPath;
                }
            }
        }

        private static string GetPresetFolderPath()
        {
            if (AssetDatabase.IsValidFolder(DEFAULT_PRESET_FOLDER))
            {
                return Application.dataPath.Replace("Assets", "") + DEFAULT_PRESET_FOLDER;
            }
            return Application.dataPath;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        public void DeleteSelected()
        {
            if (this.m_SelectedIndices.Count == 0) return;

            // Delete in reverse order to maintain indices
            var ordered = this.m_SelectedIndices.OrderByDescending(i => i).ToList();

            foreach (int index in ordered)
            {
                this.DeleteItem(index);
            }

            this.ClearSelection();
            this.UpdateFooterState();
        }

        private void RunInstructions()
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (this.m_BaseActions == null) return;
            
            this.m_BaseActions.Invoke(this.m_BaseActions.gameObject);
        }
        
        private void CopySelected()
        {
            this.NormalizeSelection();
            if (this.m_SelectedIndices.Count == 0) return;
            
            List<InstructionSnapshot> snapshots = new List<InstructionSnapshot>();
            List<int> ordered = this.m_SelectedIndices.OrderBy(i => i).ToList();
            
            foreach (int index in ordered)
            {
                SerializedProperty element = this.PropertyList.GetArrayElementAtIndex(index);
                InstructionSnapshot shot = InstructionSnapshot.FromProperty(element);
                if (shot != null) snapshots.Add(shot);
            }
            
            if (snapshots.Count == 0) return;
            InstructionMultiClipboard.Set(snapshots);
            this.UpdateFooterState();
        }
        
        private void PasteSelected()
        {
            List<InstructionSnapshot> snapshots = InstructionMultiClipboard.HasContent
                ? InstructionMultiClipboard.Get()
                : this.BuildSnapshotsFromSelection();
            
            if (snapshots.Count == 0) return;
            
            this.SerializedObject.Update();
            int insertIndex = this.PropertyList.arraySize;
            
            foreach (InstructionSnapshot snapshot in snapshots)
            {
                this.InsertItem(insertIndex, snapshot.CreateInstance());
                insertIndex += 1;
            }
            
            this.ClearSelection();
            this.UpdateFooterState();
        }
        
        internal void SetSelected(int index, bool isSelected)
        {
            this.NormalizeSelection();
            if (isSelected) this.m_SelectedIndices.Add(index);
            else this.m_SelectedIndices.Remove(index);
            this.UpdateFooterState();
        }
        
        internal bool IsSelected(int index)
        {
            this.NormalizeSelection();
            return this.m_SelectedIndices.Contains(index);
        }
        
        private void ClearSelection()
        {
            this.m_SelectedIndices.Clear();
        }
        
        private void NormalizeSelection()
        {
            int size = this.PropertyList?.arraySize ?? 0;
            this.m_SelectedIndices.RemoveWhere(i => i < 0 || i >= size);
        }

        private void UpdateFooterState()
        {
            this.NormalizeSelection();
            int selectedCount = this.m_SelectedIndices.Count;

            if (this.m_ButtonCopySelected != null)
            {
                this.m_ButtonCopySelected.SetEnabled(selectedCount > 0);
            }

            if (this.m_ButtonPasteSelected != null)
            {
                bool canPaste = InstructionMultiClipboard.HasContent || selectedCount > 0;
                this.m_ButtonPasteSelected.SetEnabled(canPaste);
            }

            if (this.m_ButtonDeleteSelected != null)
            {
                this.m_ButtonDeleteSelected.SetEnabled(selectedCount > 0);
            }

            // Save is enabled when there are items (selected or all)
            if (this.m_ButtonSavePreset != null)
            {
                int totalItems = this.PropertyList?.arraySize ?? 0;
                this.m_ButtonSavePreset.SetEnabled(totalItems > 0);
                this.m_ButtonSavePreset.tooltip = selectedCount > 0
                    ? $"Save {selectedCount} selected instruction(s) to preset"
                    : $"Save all {totalItems} instruction(s) to preset";
            }

            // Load is always enabled
            if (this.m_ButtonLoadPreset != null)
            {
                this.m_ButtonLoadPreset.SetEnabled(true);
            }
        }

        private List<InstructionSnapshot> BuildSnapshotsFromSelection()
        {
            List<InstructionSnapshot> snapshots = new List<InstructionSnapshot>();
            this.NormalizeSelection();
            
            List<int> ordered = this.m_SelectedIndices.OrderBy(i => i).ToList();
            foreach (int index in ordered)
            {
                SerializedProperty element = this.PropertyList.GetArrayElementAtIndex(index);
                InstructionSnapshot shot = InstructionSnapshot.FromProperty(element);
                if (shot != null) snapshots.Add(shot);
            }
            
            return snapshots;
        }
        
        private class InstructionSnapshot
        {
            private readonly Type m_Type;
            private readonly string m_Json;

            private InstructionSnapshot(Type type, string json)
            {
                this.m_Type = type;
                this.m_Json = json;
            }

            public Instruction CreateInstance()
            {
                Instruction instance = (Instruction)Activator.CreateInstance(this.m_Type);
                EditorJsonUtility.FromJsonOverwrite(this.m_Json, instance);
                return instance;
            }

            public static InstructionSnapshot FromProperty(SerializedProperty property)
            {
                Instruction instruction = property.GetValue<Instruction>();
                if (instruction == null) return null;

                return new InstructionSnapshot(instruction.GetType(), EditorJsonUtility.ToJson(instruction));
            }
        }
        
        private static class InstructionMultiClipboard
        {
            private static List<InstructionSnapshot> Snapshots = new List<InstructionSnapshot>();

            public static bool HasContent => Snapshots.Count > 0;

            public static void Set(List<InstructionSnapshot> snapshots)
            {
                Snapshots = snapshots ?? new List<InstructionSnapshot>();
            }

            public static List<InstructionSnapshot> Get()
            {
                return new List<InstructionSnapshot>(Snapshots);
            }

            public static void Clear()
            {
                Snapshots.Clear();
            }
        }
    }
}
