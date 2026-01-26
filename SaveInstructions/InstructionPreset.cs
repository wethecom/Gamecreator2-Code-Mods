using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameCreator.Editor.VisualScripting
{
    /// <summary>
    /// ScriptableObject asset that stores a collection of serialized Instructions.
    /// Users can save presets to the project and load them back later.
    /// </summary>
    [CreateAssetMenu(
        fileName = "New Instruction Preset",
        menuName = "Game Creator/Visual Scripting/Instruction Preset",
        order = 100
    )]
    public class InstructionPreset : ScriptableObject
    {
        [Serializable]
        public class SerializedInstruction
        {
            [SerializeField] private string m_TypeFullName;
            [SerializeField] private string m_JsonData;

            public string TypeFullName => this.m_TypeFullName;
            public string JsonData => this.m_JsonData;

            public SerializedInstruction(string typeFullName, string jsonData)
            {
                this.m_TypeFullName = typeFullName;
                this.m_JsonData = jsonData;
            }
        }

        [SerializeField] private string m_PresetName = "Unnamed Preset";
        [SerializeField] private string m_Description = "";
        [SerializeField] private List<SerializedInstruction> m_Instructions = new List<SerializedInstruction>();

        public string PresetName => this.m_PresetName;
        public string Description => this.m_Description;
        public IReadOnlyList<SerializedInstruction> Instructions => this.m_Instructions;

        public void SetPresetName(string presetName)
        {
            this.m_PresetName = presetName;
        }

        public void SetDescription(string description)
        {
            this.m_Description = description;
        }

        public void ClearInstructions()
        {
            this.m_Instructions.Clear();
        }

        public void AddInstruction(string typeFullName, string jsonData)
        {
            this.m_Instructions.Add(new SerializedInstruction(typeFullName, jsonData));
        }
    }
}
