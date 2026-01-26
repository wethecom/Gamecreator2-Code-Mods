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
        private const string ERR_NETWORK_OBJECT = "Trigger requires a NetworkObject component for network synchronization to work";
        
        // MEMBERS: -------------------------------------------------------------------------------

        private VisualElement m_Head;
        private VisualElement m_NetworkSection;
        private VisualElement m_Body;
        
        private Trigger m_Trigger;

        private SerializedProperty m_TriggerEvent;
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
            // Network header
            Label networkHeader = new Label("Network Settings");
            networkHeader.style.unityFontStyleAndWeight = FontStyle.Bold;
            networkHeader.style.marginTop = 5;
            networkHeader.style.marginBottom = 5;
            this.m_NetworkSection.Add(networkHeader);
            
            // Network properties
            this.m_SyncExecution = this.serializedObject.FindProperty("m_SyncExecution");
            this.m_ServerAuthoritative = this.serializedObject.FindProperty("m_ServerAuthoritative");
            
            PropertyField syncField = new PropertyField(this.m_SyncExecution);
            syncField.tooltip = "When enabled, trigger execution is synchronized across all connected clients";
            this.m_NetworkSection.Add(syncField);
            
            PropertyField authField = new PropertyField(this.m_ServerAuthoritative);
            authField.tooltip = "When enabled, only the server/host can initiate trigger execution";
            this.m_NetworkSection.Add(authField);
            
            // Check for NetworkObject
            this.CheckNetworkObject();
            
            // Separator
            VisualElement separator = new VisualElement();
            separator.style.height = 1;
            separator.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f);
            separator.style.marginTop = 10;
            separator.style.marginBottom = 10;
            this.m_NetworkSection.Add(separator);
        }
        
        private void CheckNetworkObject()
        {
            if (this.m_Trigger == null) return;
            
            NetworkObject networkObject = this.m_Trigger.GetComponent<NetworkObject>();
            if (networkObject == null)
            {
                HelpBox warningBox = new HelpBox(ERR_NETWORK_OBJECT, HelpBoxMessageType.Warning);
                warningBox.style.marginTop = 5;
                warningBox.style.marginBottom = 5;
                this.m_NetworkSection.Add(warningBox);
                
                Button addButton = new Button(() =>
                {
                    Undo.AddComponent<NetworkObject>(this.m_Trigger.gameObject);
                    this.m_NetworkSection.Clear();
                    this.CreateNetworkSection();
                });
                addButton.text = "Add NetworkObject Component";
                addButton.style.marginBottom = 5;
                this.m_NetworkSection.Add(addButton);
            }
            else
            {
                HelpBox infoBox = new HelpBox("NetworkObject found. Network sync is ready.", HelpBoxMessageType.Info);
                infoBox.style.marginTop = 5;
                infoBox.style.marginBottom = 5;
                this.m_NetworkSection.Add(infoBox);
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
            instance.AddComponent<NetworkObject>();
            
            GameObjectUtility.SetParentAndAlign(instance, menuCommand?.context as GameObject);

            Undo.RegisterCreatedObjectUndo(instance, $"Create {instance.name}");
            Selection.activeObject = instance;
        }
    }
}
