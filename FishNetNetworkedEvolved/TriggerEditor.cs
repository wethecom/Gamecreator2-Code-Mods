using System;
using GameCreator.Editor.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using FishNet.Object;

namespace GameCreator.Editor.VisualScripting
{
    [CustomEditor(typeof(Trigger))]
    public class TriggerEditor : BaseActionsEditor
    {
        private const string ERR_NAME = "GC-Trigger-Error-Message";
        private const string ERR_COLLIDER = "{0} requires a Collider or Collider2D in order to work";
        private const string ERR_COMPONENT = "{0} requires a {1} component in order to work";
        private const string ERR_NETWORK_OBJECT = "Network sync requires a NetworkObject component to work";
        
        // MEMBERS: -------------------------------------------------------------------------------

        private VisualElement m_Head;
        private VisualElement m_NetworkSection;
        private VisualElement m_NetworkOptions;
        private VisualElement m_Body;
        
        private Trigger m_Trigger;

        private SerializedProperty m_TriggerEvent;
        private SerializedProperty m_EnableNetworking;
        private SerializedProperty m_SyncExecution;
        private SerializedProperty m_ServerAuthoritative;
        
        // INITIALIZERS: --------------------------------------------------------------------------
        
        private void OnEnable()
        {
            this.m_Trigger = this.target as Trigger;
        }

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            this.m_Head = new VisualElement();
            this.m_NetworkSection = new VisualElement();
            this.m_NetworkOptions = new VisualElement();
            this.m_Body = new VisualElement();
            
            root.Add(this.m_Head);
            root.Add(this.m_NetworkSection);
            root.Add(this.m_Body);
            
            root.style.marginTop = DEFAULT_MARGIN_TOP;

            // Network settings section
            this.CreateNetworkSection();

            // Event section
            this.m_TriggerEvent = this.serializedObject.FindProperty("m_TriggerEvent");
            PropertyField fieldTriggerEvent = new PropertyField(this.m_TriggerEvent);
            
            this.m_Body.Add(fieldTriggerEvent);

            fieldTriggerEvent.RegisterValueChangeCallback(this.RefreshHead);
            this.RefreshHead(null);

            // Instructions section
            this.CreateInstructionsGUI(this.m_Body);
            
            return root;
        }
        
        private void CreateNetworkSection()
        {
            this.m_NetworkSection.Clear();
            this.m_NetworkOptions.Clear();
            
            // Network header
            Label networkHeader = new Label("Network Settings");
            networkHeader.style.unityFontStyleAndWeight = FontStyle.Bold;
            networkHeader.style.marginTop = 10;
            networkHeader.style.marginBottom = 5;
            this.m_NetworkSection.Add(networkHeader);
            
            // Enable Networking toggle (always visible)
            this.m_EnableNetworking = this.serializedObject.FindProperty("m_EnableNetworking");
            PropertyField enableField = new PropertyField(this.m_EnableNetworking, "Enable Networking");
            enableField.tooltip = "Enable network synchronization for this trigger";
            this.m_NetworkSection.Add(enableField);
            
            // Container for network options (shown/hidden based on toggle)
            this.m_NetworkSection.Add(this.m_NetworkOptions);
            
            // Register callback to show/hide network options
            enableField.RegisterValueChangeCallback(evt =>
            {
                this.RefreshNetworkOptions();
            });
            
            // Initial refresh
            this.RefreshNetworkOptions();
            
            // Separator
            VisualElement separator = new VisualElement();
            separator.style.height = 1;
            separator.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f);
            separator.style.marginTop = 10;
            separator.style.marginBottom = 10;
            this.m_NetworkSection.Add(separator);
        }
        
        private void RefreshNetworkOptions()
        {
            this.m_NetworkOptions.Clear();
            
            if (this.m_EnableNetworking == null) return;
            
            // Only show options if networking is enabled
            if (!this.m_EnableNetworking.boolValue)
            {
                // Show a hint when disabled
                HelpBox disabledHint = new HelpBox(
                    "Enable networking to synchronize trigger execution across clients.", 
                    HelpBoxMessageType.None
                );
                disabledHint.style.marginTop = 5;
                disabledHint.style.marginBottom = 5;
                this.m_NetworkOptions.Add(disabledHint);
                return;
            }
            
            // Network is enabled - show options
            this.m_NetworkOptions.style.marginLeft = 15;
            this.m_NetworkOptions.style.marginTop = 5;
            
            // Network properties
            this.m_SyncExecution = this.serializedObject.FindProperty("m_SyncExecution");
            this.m_ServerAuthoritative = this.serializedObject.FindProperty("m_ServerAuthoritative");
            
            PropertyField syncField = new PropertyField(this.m_SyncExecution, "Sync Execution");
            syncField.tooltip = "When enabled, trigger execution is synchronized across all connected clients";
            this.m_NetworkOptions.Add(syncField);
            
            PropertyField authField = new PropertyField(this.m_ServerAuthoritative, "Server Authoritative");
            authField.tooltip = "When enabled, only the server/host can initiate trigger execution";
            this.m_NetworkOptions.Add(authField);
            
            // Check for NetworkObject and show appropriate message/button
            this.CheckNetworkObject();
        }
        
        private void CheckNetworkObject()
        {
            if (this.m_Trigger == null) return;
            
            NetworkObject networkObject = this.m_Trigger.GetComponent<NetworkObject>();
            
            if (networkObject == null)
            {
                // Warning - NetworkObject missing
                HelpBox warningBox = new HelpBox(ERR_NETWORK_OBJECT, HelpBoxMessageType.Warning);
                warningBox.style.marginTop = 10;
                warningBox.style.marginBottom = 5;
                this.m_NetworkOptions.Add(warningBox);
                
                // Button to add NetworkObject
                Button addButton = new Button(() =>
                {
                    Undo.AddComponent<NetworkObject>(this.m_Trigger.gameObject);
                    // Refresh the network options to update the UI
                    this.RefreshNetworkOptions();
                });
                addButton.text = "Add NetworkObject Component";
                addButton.style.marginBottom = 5;
                this.m_NetworkOptions.Add(addButton);
            }
            else
            {
                // Success - NetworkObject found
                HelpBox infoBox = new HelpBox(
                    "NetworkObject found. Network sync is ready.", 
                    HelpBoxMessageType.Info
                );
                infoBox.style.marginTop = 10;
                infoBox.style.marginBottom = 5;
                this.m_NetworkOptions.Add(infoBox);
            }
        }

        private void RefreshHead(SerializedPropertyChangeEvent changeEvent)
        {
            this.m_Head.Clear();
            
            if (this.m_TriggerEvent == null) return;
                
            object value = this.m_TriggerEvent.GetValue<GameCreator.Runtime.VisualScripting.Event>();
            
            if (value is GameCreator.Runtime.VisualScripting.Event eventValue)
            {
                if (eventValue.RequiresCollider && !this.HasCollider())
                {
                    string message = string.Format(
                        ERR_COLLIDER, 
                        TypeUtils.GetTitleFromType(eventValue.GetType())
                    );
                    
                    this.m_Head.Add(new ErrorMessage(message) { name = ERR_NAME });
                }

                Type component = eventValue.RequiresComponent;
                if (component != null && !this.m_Trigger.GetComponent(component))
                {
                    string message = string.Format(
                        ERR_COMPONENT,
                        TypeUtils.GetTitleFromType(eventValue.GetType()),
                        TypeUtils.GetTitleFromType(component)
                    );
                    
                    this.m_Head.Add(new ErrorMessage(message) { name = ERR_NAME });
                }
            }
        }

        private bool HasCollider()
        {
            if (this.m_Trigger.GetComponent<Collider>()) return true;
            if (this.m_Trigger.GetComponent<Collider2D>()) return true;
            
            return false;
        }
        
        // CREATION MENU: -------------------------------------------------------------------------
        
        [MenuItem("GameObject/Game Creator/Visual Scripting/Trigger", false, 0)]
        public static void CreateElement(MenuCommand menuCommand)
        {
            GameObject instance = new GameObject("Trigger");
            instance.AddComponent<Trigger>();
            // NOTE: NetworkObject is NOT added automatically
            // User must enable networking and click the button to add it
            
            GameObjectUtility.SetParentAndAlign(instance, menuCommand?.context as GameObject);

            Undo.RegisterCreatedObjectUndo(instance, $"Create {instance.name}");
            Selection.activeObject = instance;
        }
    }
}
