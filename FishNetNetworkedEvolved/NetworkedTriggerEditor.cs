using GameCreator.Editor.VisualScripting;
using UnityEditor;
using UnityEngine;
using Wethecom.Runtime;

namespace Wethecom.Editor
{
    [CustomEditor(typeof(NetworkedTrigger))]
    public class NetworkedTriggerEditor : TriggerEditor
    {
        public override void OnInspectorGUI()
        {
            NetworkedTrigger trigger = (NetworkedTrigger)target;
            
            EditorGUILayout.HelpBox(
                "NetworkedTrigger extends GameCreator's Trigger system with Fish-Net networking support.\n\n" +
                "Trigger execution is synchronized across all clients in the network.",
                MessageType.Info
            );
            
            EditorGUILayout.Space();
            
            // Show network state if in play mode
            if (Application.isPlaying)
            {
                var netObj = trigger.GetComponent<FishNet.Object.NetworkObject>();
                if (netObj != null)
                {
                    EditorGUILayout.LabelField("Fish-Net Network Status", EditorStyles.boldLabel);
                    
                    GUI.enabled = false;
                    EditorGUILayout.Toggle("Is Spawned", netObj.IsSpawned);
                    EditorGUILayout.Toggle("Is Server", netObj.IsServerInitialized);
                    EditorGUILayout.Toggle("Is Owner", netObj.IsOwner);
                    EditorGUILayout.Toggle("Is Client", netObj.IsClientInitialized);
                    GUI.enabled = true;
                    
                    EditorGUILayout.Space();
                }
            }
            
            // Draw the base Trigger inspector
            base.OnInspectorGUI();
        }
    }
}