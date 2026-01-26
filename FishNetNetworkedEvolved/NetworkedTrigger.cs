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
        [Header("Network Settings")]
        [SerializeField] private bool _allowClientTriggers = true;
        [SerializeField] private bool _syncToLateJoiners = true;
        
        // Reference to the network object (automatically set up)
        private NetworkObject _networkObject;
        
        // Callbacks passed during setup
        private Action<Args> _onTriggerRun;
        private Action<Args> _onTriggerStopped;
        
        // Network synchronized state
        [SyncVar(OnChange = nameof(OnTriggerStateChanged))]
        private bool _isTriggered = false;
        
        // Buffer for late-joining clients
        [SyncVar] private string _bufferedTriggerData = string.Empty;
        
        // Track setup state
        private bool _isSetup = false;
        private bool _isProcessingNetworkedTrigger = false;
        
        #region Setup & Initialization

        /// <summary>
        /// Initialize the networked trigger with an event and callbacks
        /// </summary>
        public void Setup(Event triggerEvent, Action<Args> onTriggerRun = null, Action<Args> onTriggerStopped = null)
        {
            if (_isSetup) return;
            
            // Hide base component flags as before
            hideFlags = HideFlags.HideInInspector | HideFlags.DontSaveInBuild | 
                        HideFlags.DontSaveInEditor | HideFlags.HideInHierarchy;
            
            m_TriggerEvent = triggerEvent;
            _onTriggerRun = onTriggerRun;
            _onTriggerStopped = onTriggerStopped;
            
            // Initialize the base Trigger
            Awake();
            
            // Get or add NetworkObject component
            _networkObject = GetComponent<NetworkObject>();
            if (_networkObject == null)
            {
                _networkObject = gameObject.AddComponent<NetworkObject>();
            }
            
            // Subscribe to the trigger events
            EventBeforeExecute -= OnTriggerExecute;
            EventBeforeExecute += OnTriggerExecute;
            
            _isSetup = true;
        }

        protected override void OnEnable()
        {
            // Only call base if we're already set up
            if (_isSetup)
            {
                base.OnEnable();
            }
        }

        #endregion

        #region Trigger Execution Override

        /// <summary>
        /// Override the base trigger execution to add networking
        /// </summary>
        private void OnTriggerExecute()
        {
            if (!_isSetup || _isProcessingNetworkedTrigger) return;
            
            var args = m_Args ?? new Args(gameObject);
            
            // Check if we should process this trigger
            if (!ShouldProcessTrigger(args))
                return;
            
            // Process based on network authority
            if (IsNetworkAvailable())
            {
                ProcessNetworkedTrigger(args);
            }
            else
            {
                // No network available, just execute locally
                ExecuteLocalTrigger(args);
            }
        }

        /// <summary>
        /// Determine if this trigger should be processed
        /// </summary>
        private bool ShouldProcessTrigger(Args args)
        {
            // Always allow processing if we're not networked
            if (!IsNetworkAvailable())
                return true;
            
            // Check if client triggers are allowed
            if (!_allowClientTriggers && !_networkObject.IsServerInitialized)
            {
                Debug.LogWarning("[NetworkedTrigger] Client triggers are disabled");
                return false;
            }
            
            // Prevent already triggered states from firing again
            if (_isTriggered)
                return false;
                
            return true;
        }

        /// <summary>
        /// Process trigger with networking
        /// </summary>
        private void ProcessNetworkedTrigger(Args args)
        {
            if (_networkObject.IsServerInitialized)
            {
                // Server executes and syncs to all clients
                ExecuteTriggerAsServer(args);
            }
            else if (_networkObject.IsOwner && _allowClientTriggers)
            {
                // Client owner requests server to execute
                RequestTriggerExecutionServerRpc(SerializeTriggerData(args));
            }
            // Non-owner clients do nothing - they'll receive the sync from server
        }

        /// <summary>
        /// Execute trigger on the server and sync to clients
        /// </summary>
        private void ExecuteTriggerAsServer(Args args)
        {
            // Set the sync state first
            _isTriggered = true;
            
            // Buffer data for late joiners if enabled
            if (_syncToLateJoiners)
            {
                _bufferedTriggerData = SerializeTriggerData(args);
            }
            
            // Execute locally on server
            ExecuteLocalTrigger(args);
            
            // Broadcast to all clients
            BroadcastTriggerExecutionObserversRpc(SerializeTriggerData(args));
        }

        #endregion

        #region FishNet RPCs

        /// <summary>
        /// Client requests server to execute the trigger
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        private void RequestTriggerExecutionServerRpc(string triggerData, ServerRpcParams rpcParams = default)
        {
            if (!_allowClientTriggers) return;
            
            var args = DeserializeTriggerData(triggerData);
            ExecuteTriggerAsServer(args);
        }

        /// <summary>
        /// Server broadcasts trigger execution to all clients
        /// </summary>
        [ObserversRpc(BufferLast = true, ExcludeServer = true)]
        private void BroadcastTriggerExecutionObserversRpc(string triggerData)
        {
            var args = DeserializeTriggerData(triggerData);
            ExecuteLocalTrigger(args);
        }

        #endregion

        #region Local Execution

        /// <summary>
        /// Execute the trigger locally with network awareness
        /// </summary>
        private void ExecuteLocalTrigger(Args args)
        {
            _isProcessingNetworkedTrigger = true;
            
            try
            {
                // Execute the GameCreator trigger event
                base.Execute(args);
                
                // Execute custom callbacks
                _onTriggerRun?.Invoke(args);
                _onTriggerStopped?.Invoke(args);
            }
            finally
            {
                _isProcessingNetworkedTrigger = false;
            }
        }

        #endregion

        #region Serialization Helpers

        /// <summary>
        /// Serialize trigger data for network transmission
        /// </summary>
        private string SerializeTriggerData(Args args)
        {
            // Simple serialization - extend based on your needs
            // You might want to serialize specific args properties
            return args.Target != null ? 
                $"{args.Target.GetInstanceID()}|{Time.time}" : 
                $"0|{Time.time}";
        }

        /// <summary>
        /// Deserialize trigger data from network
        /// </summary>
        private Args DeserializeTriggerData(string data)
        {
            if (string.IsNullOrEmpty(data))
                return new Args(gameObject);
            
            var parts = data.Split('|');
            if (parts.Length > 0 && int.TryParse(parts[0], out int instanceId) && instanceId > 0)
            {
                var target = FindGameObjectByInstanceID(instanceId);
                return new Args(target ?? gameObject);
            }
            
            return new Args(gameObject);
        }

        private GameObject FindGameObjectByInstanceID(int instanceId)
        {
            // Note: This is a simple implementation. In production, you might want
            // a more efficient way to find objects by instance ID across the network
            var allObjects = FindObjectsOfType<GameObject>();
            foreach (var obj in allObjects)
            {
                if (obj.GetInstanceID() == instanceId)
                    return obj;
            }
            return null;
        }

        #endregion

        #region Network State Management

        /// <summary>
        /// Called when the SyncVar changes
        /// </summary>
        private void OnTriggerStateChanged(bool previous, bool current, bool asServer)
        {
            Debug.Log($"[NetworkedTrigger] Trigger state changed: {previous} -> {current} (asServer: {asServer})");
            
            // If a client receives a triggered state, they might need to execute
            if (!asServer && current && _isSetup && !_isProcessingNetworkedTrigger)
            {
                // Client received trigger sync from server
                // Note: The actual execution should come via RPC, not SyncVar change
            }
        }

        /// <summary>
        /// Check if network is available and spawned
        /// </summary>
        private bool IsNetworkAvailable()
        {
            return _networkObject != null && _networkObject.IsSpawned;
        }

        /// <summary>
        /// Handle late-joining clients
        /// </summary>
        public void OnStartClient()
        {
            if (!_networkObject.IsServerInitialized && 
                _syncToLateJoiners && 
                !string.IsNullOrEmpty(_bufferedTriggerData) &&
                _isSetup)
            {
                // Late-joining client: execute buffered trigger
                var args = DeserializeTriggerData(_bufferedTriggerData);
                ExecuteLocalTrigger(args);
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Reset the trigger state (Server only)
        /// </summary>
        public void ResetTrigger()
        {
            if (!IsNetworkAvailable() || !_networkObject.IsServerInitialized)
                return;
            
            _isTriggered = false;
            _bufferedTriggerData = string.Empty;
        }

        /// <summary>
        /// Manually trigger from code (Server only)
        /// </summary>
        public void TriggerManually(Args args = null)
        {
            if (!IsNetworkAvailable() || !_networkObject.IsServerInitialized)
                return;
            
            ExecuteTriggerAsServer(args ?? new Args(gameObject));
        }

        #endregion

        #region Override Base Methods

        /// <summary>
        /// Override the base Execute to add network awareness
        /// </summary>
        public override void Execute(Args args)
        {
            if (!_isSetup)
            {
                base.Execute(args);
                return;
            }
            
            // Use our networked execution path
            OnTriggerExecute();
        }

        protected override void OnDestroy()
        {
            EventBeforeExecute -= OnTriggerExecute;
            base.OnDestroy();
        }

        #endregion

        #region Editor & Validation

        private void OnValidate()
        {
            // Ensure we have a NetworkObject
            if (_networkObject == null)
                _networkObject = GetComponent<NetworkObject>();
        }

        #endregion
    }
}