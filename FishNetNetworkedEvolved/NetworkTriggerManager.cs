using System;
using FishNet.Object;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace Wethecom.Runtime
{
    /// <summary>
    /// Handles Fish-Net networking for trigger synchronization
    /// </summary>
    [AddComponentMenu("")]
    public class NetworkTriggerManager : NetworkBehaviour
    {
        public event Action<Args> OnNetworkTrigger;

        [ServerRpc(RequireOwnership = false)]
        public void RequestTriggerServerRpc(int targetInstanceId)
        {
            BroadcastTriggerObserversRpc(targetInstanceId);
        }

        [ObserversRpc]
        public void BroadcastTriggerObserversRpc(int targetInstanceId)
        {
            GameObject target = null;
            if (targetInstanceId != 0)
            {
                var allObjects = FindObjectsOfType<GameObject>();
                foreach (var obj in allObjects)
                {
                    if (obj.GetInstanceID() == targetInstanceId)
                    {
                        target = obj;
                        break;
                    }
                }
            }
            
            var args = new Args(target ?? gameObject);
            OnNetworkTrigger?.Invoke(args);
        }
    }
}