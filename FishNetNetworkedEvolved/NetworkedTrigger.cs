using System;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;
using Event = GameCreator.Runtime.VisualScripting.Event;

namespace Wethecom.Runtime
{
    [AddComponentMenu("Wethecom/Networked Trigger")]
    public class NetworkedTrigger : Trigger
    {
        // Fish-Net network component reference
        private NetworkObject m_NetworkObject;
        
        // Callbacks for trigger execution
        private Action<Args> m_OnTriggerRun;
        private Action<Args> m_OnTriggerStopped;
        
        // Network synchronized state
        [SyncVar(OnChange = nameof(OnTriggerStateChanged))]
        private bool m_IsTriggered = false;
        
        // Track if we've been set up
        private bool m_IsSetup = false;

        /// <summary>
        /// Initialize the networked trigger with an event and callbacks
        /// </summary>
        public void Setup(Event triggerEvent, Action<Args> onTriggerRun, Action<Args> onTriggerStopped)
        {
            // Hide this component from the inspector
            hideFlags = HideFlags.HideInInspector | HideFlags.DontSaveInBuild | 
                        HideFlags.DontSaveInEditor | HideFlags.HideInHierarchy;
            enabled = false;
            
            m_TriggerEvent = triggerEvent;
            m_OnTriggerRun = onTriggerRun;
            m_OnTriggerStopped = onTriggerStopped;
            
            // Get or add NetworkObject component
            m_NetworkObject = GetComponent<NetworkObject>();
            if (m_NetworkObject == null)
            {
                m_NetworkObject = gameObject.AddComponent<NetworkObject>();
            }
            
            // Call Awake to initialize the Trigger base class
            Awake();

            // Subscribe to the trigger events
            EventBeforeExecute -= OnRun;
            EventBeforeExecute += OnRun;
            
            m_IsSetup = true;
            enabled = true;
        }

        /// <summary>
        /// Called when the trigger executes
        /// </summary>
        private void OnRun()
        {
            if (!m_IsSetup) return;
            
            // Get args from base Trigger class
            var args = base.m_Args;
            
            // Determine network authority and execute accordingly
            if (m_NetworkObject != null && m_NetworkObject.IsSpawned)
            {
                if (m_NetworkObject.IsServerInitialized)
                {
                    // We're the server, execute and sync to all clients
                    ExecuteAndSync(args);
                }
                else if (m_NetworkObject.IsOwner)
                {
                    // We're the owner but not server, request execution
                    RequestExecutionServerRpc(args);
                }
                else
                {
                    // Not server or owner, just execute locally
                    ExecuteLocal(args);
                }
            }
            else
            {
                // No network object or not spawned, execute locally only
                ExecuteLocal(args);
            }
        }

        /// <summary>
        /// Server executes and syncs to all clients
        /// </summary>
        private void ExecuteAndSync(Args args)
        {
            m_IsTriggered = true;
            BroadcastExecutionObserversRpc(args);
        }

        /// <summary>
        /// Owner requests server to execute and sync
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        private void RequestExecutionServerRpc(Args args)
        {
            ExecuteAndSync(args);
        }

        /// <summary>
        /// Server broadcasts execution to all clients
        /// </summary>
        [ObserversRpc(BufferLast = true)]
        private void BroadcastExecutionObserversRpc(Args args)
        {
            ExecuteLocal(args);
        }

        /// <summary>
        /// Execute the trigger callbacks locally
        /// </summary>
        private void ExecuteLocal(Args args)
        {
            // Execute both callbacks immediately
            m_OnTriggerRun?.Invoke(args);
            m_OnTriggerStopped?.Invoke(args);
        }

        /// <summary>
        /// Called when the SyncVar changes
        /// </summary>
        private void OnTriggerStateChanged(bool prev, bool next, bool asServer)
        {
            // Handle sync state changes if needed
            if (!asServer && next && m_IsSetup)
            {
                Debug.Log("[NetworkedTrigger] Trigger state synchronized from server");
            }
        }

        /// <summary>
        /// Handle late-joining clients
        /// </summary>
        private void OnStartNetwork()
        {
            // If a client joins after trigger execution, sync them
            if (m_NetworkObject != null && !m_NetworkObject.IsServerInitialized && 
                m_IsTriggered && m_IsSetup)
            {
                ExecuteLocal(base.m_Args ?? new Args(gameObject));
            }
        }

        /// <summary>
        /// Cleanup subscriptions
        /// </summary>
        protected override void OnDestroy()
        {
            EventBeforeExecute -= OnRun;
            base.OnDestroy();
        }

        /// <summary>
        /// Reset the trigger state (Server only)
        /// </summary>
        public void ResetTrigger()
        {
            if (m_NetworkObject != null && m_NetworkObject.IsServerInitialized)
            {
                m_IsTriggered = false;
            }
        }
    }
}