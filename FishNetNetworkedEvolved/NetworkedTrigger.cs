using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;
using Event = GameCreator.Runtime.VisualScripting.Event;

namespace Wethecom.Runtime
{
    /// <summary>
    /// TriggerRunner with Fish-Net networking
    /// </summary>
    [AddComponentMenu("")]
    public class NetworkedTrigger : Trigger
    {
        private NetworkTriggerManager _networkManager;
        private Action<Args> _onTriggerRun;
        private Action<Args> _onTriggerStopped;

        public void Setup(Event triggerEvent, Action<Args> onTriggerRun, Action<Args> onTriggerStopped)
        {
            hideFlags = HideFlags.HideInInspector | HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor | HideFlags.HideInHierarchy;
            enabled = false;
            
            m_TriggerEvent = triggerEvent;
            _onTriggerRun = onTriggerRun;
            _onTriggerStopped = onTriggerStopped;
            
            // Setup network manager
            _networkManager = GetComponent<NetworkTriggerManager>();
            if (_networkManager == null)
            {
                _networkManager = gameObject.AddComponent<NetworkTriggerManager>();
            }
            
            Awake();

            EventBeforeExecute -= OnRun;
            EventBeforeExecute += OnRun;
            
            _networkManager.OnNetworkTrigger -= OnNetworkTrigger;
            _networkManager.OnNetworkTrigger += OnNetworkTrigger;
            
            enabled = true;
        }

        private void OnRun()
        {
            var args = base.m_Args;
            
            // If networked and spawned, sync it
            if (_networkManager != null && _networkManager.IsSpawned)
            {
                if (_networkManager.IsServerInitialized)
                {
                    // Server: execute and broadcast
                    _onTriggerRun(args);
                    _onTriggerStopped(args);
                    _networkManager.BroadcastTriggerObserversRpc(args.Target != null ? args.Target.GetInstanceID() : 0);
                }
                else
                {
                    // Client: request from server
                    _networkManager.RequestTriggerServerRpc(args.Target != null ? args.Target.GetInstanceID() : 0);
                }
            }
            else
            {
                // Not networked
                _onTriggerRun(args);
                _onTriggerStopped(args);
            }
        }

        private void OnNetworkTrigger(Args args)
        {
            _onTriggerRun(args);
            _onTriggerStopped(args);
        }

        protected override void OnDestroy()
        {
            EventBeforeExecute -= OnRun;
            
            if (_networkManager != null)
            {
                _networkManager.OnNetworkTrigger -= OnNetworkTrigger;
            }
            
            base.OnDestroy();
        }
    }
}