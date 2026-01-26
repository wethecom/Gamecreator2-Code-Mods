using GameCreator.Editor.VisualScripting;
using UnityEditor;
using UnityEngine;
using Wethecom.Runtime;
using FishNet.Object;

namespace Wethecom.Editor
{
    [CustomEditor(typeof(NetworkedTrigger))]
    public class NetworkedTriggerEditor : TriggerEditor
    {
        private SerializedProperty _allowClientTriggers;
        private SerializedProperty _syncToLateJoiners;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            _allowClientTriggers = serializedObject.FindProperty("_allowClientTriggers");
            _syncToLateJoiners = serializedObject.FindProperty("_syncToLateJoiners");
        }

        public override void OnInspectorGUI()
        {
            NetworkedTrigger trigger = (NetworkedTrigger)target;
            
            EditorGUILayout.HelpBox(
                "NetworkedTrigger extends GameCreator's Trigger system with Fish-Net networking support.\n\n" +
                "• Trigger execution is synchronized across all clients\n" +
                "• Can restrict triggers to server-only\n" +
                "• Supports late-joining clients",
                MessageType.Info
            );
            
            EditorGUILayout.Space();
            
            // Network settings
            EditorGUILayout.LabelField("Network Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_allowClientTriggers);
            EditorGUILayout.PropertyField(_syncToLateJoiners);
            
            EditorGUILayout.Space();
            
            // Show network status if in play mode
            if (Application.isPlaying)
            {
                var netObj = trigger.GetComponent<NetworkObject>();
                if (netObj != null)
                {
                    EditorGUILayout.LabelField("Fish-Net Status", EditorStyles.boldLabel);
                    
                    using (new EditorGUI.DisabledScope(true))
                    {
                        EditorGUILayout.Toggle("Is Spawned", netObj.IsSpawned);
                        EditorGUILayout.Toggle("Is Server", netObj.IsServerInitialized);
                        EditorGUILayout.Toggle("Is Owner", netObj.IsOwner);
                        EditorGUILayout.Toggle("Is Client", netObj.IsClientInitialized);
                    }
                    
                    EditorGUILayout.Space();
                }
            }
            
            serializedObject.ApplyModifiedProperties();
            
            // Draw the base Trigger inspector (conditions, actions, etc.)
            EditorGUILayout.LabelField("Trigger Configuration", EditorStyles.boldLabel);
            base.OnInspectorGUI();
        }
    }
}