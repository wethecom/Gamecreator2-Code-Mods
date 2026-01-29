using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using UnityEngine;
using UnityEngine.EventSystems;
using FishNet.Object;
using FishNet.Connection;

namespace GameCreator.Runtime.VisualScripting
{
    [HelpURL("https://docs.gamecreator.io/gamecreator/visual-scripting/triggers")]
    [AddComponentMenu("Game Creator/Visual Scripting/Trigger")]
    [DefaultExecutionOrder(ApplicationManager.EXECUTION_ORDER_DEFAULT_LATER)]
    
    [Icon(RuntimePaths.GIZMOS + "GizmoTrigger.png")]
    public class Trigger : 
        BaseActions, 
        IPointerEnterHandler, 
        IPointerExitHandler,
        ISelectHandler,
        IDeselectHandler,
        ISignalReceiver
    {
        private const int NO_TARGET = -1;
        
        // MEMBERS: -------------------------------------------------------------------------------
        
        [SerializeReference]
        protected Event m_TriggerEvent = new EventOnStart();
        
        [Header("Network Settings")]
        [SerializeField, Tooltip("Enable network synchronization for this trigger")]
        private bool m_EnableNetworking = false;
        
        [SerializeField, Tooltip("Synchronize trigger execution across all clients")]
        private bool m_SyncExecution = true;
        
        [SerializeField, Tooltip("Only server/host can initiate trigger execution")]
        private bool m_ServerAuthoritative = true;

        [NonSerialized] public Args m_Args;

        [NonSerialized] private Rigidbody m_Rigidbody3D;
        [NonSerialized] private Rigidbody2D m_Rigidbody2D;

        [NonSerialized] private Collider m_Collider3D;
        [NonSerialized] private Collider2D m_Collider2D;
        
        [NonSerialized] private IInteractive m_Interactive;
        
        [NonSerialized] private bool m_IsNetworkExecution;
        
        // Cached NetworkObject reference
        [NonSerialized] private NetworkObject m_NetworkObject;
        [NonSerialized] private bool m_NetworkObjectCached;

        // PROPERTIES: ----------------------------------------------------------------------------

        public bool IsExecuting { get; private set; }
        
        public bool EnableNetworking
        {
            get => m_EnableNetworking;
            set => m_EnableNetworking = value;
        }
        
        public bool SyncExecution
        {
            get => m_SyncExecution;
            set => m_SyncExecution = value;
        }
        
        public bool ServerAuthoritative
        {
            get => m_ServerAuthoritative;
            set => m_ServerAuthoritative = value;
        }
        
        /// <summary>
        /// Returns the cached NetworkObject component, or null if not present
        /// </summary>
        private NetworkObject CachedNetworkObject
        {
            get
            {
                if (!m_NetworkObjectCached)
                {
                    m_NetworkObject = GetComponent<NetworkObject>();
                    m_NetworkObjectCached = true;
                }
                return m_NetworkObject;
            }
        }
        
        /// <summary>
        /// Returns true if networking is enabled, NetworkObject exists, and object is spawned
        /// </summary>
        private bool IsNetworkReady => m_EnableNetworking && 
                                        CachedNetworkObject != null && 
                                        CachedNetworkObject.IsSpawned;
        
        /// <summary>
        /// Returns true if we should sync this execution across the network
        /// </summary>
        private bool ShouldSync => m_SyncExecution && IsNetworkReady && !m_IsNetworkExecution;

        // EVENTS: --------------------------------------------------------------------------------

        public event Action EventBeforeExecute;
        public event Action EventAfterExecute;
        
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public static void Reconfigure(Trigger trigger, Event triggerEvent, InstructionList instructions)
        {
            trigger.m_TriggerEvent = triggerEvent;
            trigger.m_Instructions = instructions;
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void Invoke(GameObject self = null)
        {
            Args args = new Args(self != null ? self : this.gameObject, this.gameObject);
            _ = this.Execute(args);
        }
        
        public async Task Execute(Args args)
        {
            if (this.IsExecuting) return;
            
            if (ShouldSync)
            {
                int targetId = GetNetworkObjectId(args.Target);
                
                if (m_ServerAuthoritative)
                {
                    if (CachedNetworkObject.IsServerInitialized)
                    {
                        ExecuteOnClientsRpc(targetId);
                    }
                    else
                    {
                        RequestExecuteServerRpc(targetId);
                    }
                }
                else
                {
                    if (CachedNetworkObject.IsServerInitialized)
                    {
                        ExecuteOnClientsRpc(targetId);
                    }
                    else
                    {
                        RequestExecuteServerRpc(targetId);
                    }
                }
                return;
            }
            
            await ExecuteLocalInternal(args);
        }
        
        private async Task ExecuteLocalInternal(Args args)
        {
            if (this.IsExecuting) return;
            this.IsExecuting = true;
            
            this.EventBeforeExecute?.Invoke();
            
            try
            {
                await this.ExecInstructions(args);
            }
            catch (Exception exception)
            {
                Debug.LogError(exception.ToString(), this);
            }
            
            this.IsExecuting = false;
            this.EventAfterExecute?.Invoke();
        }
        
        public async Task Execute(GameObject target)
        {
            if (this.IsExecuting) return;
            
            this.m_Args.ChangeTarget(target);
            await this.Execute(this.m_Args);
        }
        
        public async Task Execute()
        {
            if (this.IsExecuting) return;
            
            this.m_Args.ChangeTarget(null);
            await this.Execute(this.m_Args);
        }

        public void Cancel()
        {
            if (ShouldSync)
            {
                if (CachedNetworkObject.IsServerInitialized)
                {
                    CancelOnClientsRpc();
                }
                else
                {
                    RequestCancelServerRpc();
                }
                return;
            }
            
            CancelLocalInternal();
        }
        
        private void CancelLocalInternal()
        {
            this.StopExecInstructions();
        }
        
        /// <summary>
        /// Clears the cached NetworkObject reference. Call this if you add/remove
        /// NetworkObject at runtime.
        /// </summary>
        public void RefreshNetworkObjectCache()
        {
            m_NetworkObjectCached = false;
            m_NetworkObject = null;
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        // NETWORK HELPERS: -----------------------------------------------------------------------
        
        private int GetNetworkObjectId(GameObject target)
        {
            if (target == null) return NO_TARGET;
            
            NetworkObject nob = target.GetComponent<NetworkObject>();
            if (nob == null || !nob.IsSpawned) return NO_TARGET;
            
            return nob.ObjectId;
        }
        
        private GameObject ResolveNetworkObject(int objectId)
        {
            if (objectId == NO_TARGET) return null;
            if (CachedNetworkObject == null || CachedNetworkObject.NetworkManager == null) return null;
            
            var networkManager = CachedNetworkObject.NetworkManager;
            
            if (CachedNetworkObject.IsServerInitialized)
            {
                if (networkManager.ServerManager.Objects.Spawned.TryGetValue(objectId, out NetworkObject nob))
                {
                    return nob.gameObject;
                }
            }
            else if (CachedNetworkObject.IsClientInitialized)
            {
                if (networkManager.ClientManager.Objects.Spawned.TryGetValue(objectId, out NetworkObject nob))
                {
                    return nob.gameObject;
                }
            }
            
            return null;
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        // NETWORK RPCs: --------------------------------------------------------------------------
        // Note: These methods will only work if a NetworkObject component is present
        // and the object has been spawned on the network.
        
        [ServerRpc(RequireOwnership = false)]
        private void RequestExecuteServerRpc(int targetObjectId, NetworkConnection sender = null)
        {
            ExecuteOnClientsRpc(targetObjectId);
        }
        
        [ObserversRpc(BufferLast = true, RunLocally = true)]
        private void ExecuteOnClientsRpc(int targetObjectId)
        {
            m_IsNetworkExecution = true;
            
            GameObject target = ResolveNetworkObject(targetObjectId);
            this.m_Args.ChangeTarget(target);
            
            _ = ExecuteLocalInternal(this.m_Args);
            
            m_IsNetworkExecution = false;
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void RequestCancelServerRpc(NetworkConnection sender = null)
        {
            CancelOnClientsRpc();
        }
        
        [ObserversRpc(RunLocally = true)]
        private void CancelOnClientsRpc()
        {
            m_IsNetworkExecution = true;
            CancelLocalInternal();
            m_IsNetworkExecution = false;
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        // INITIALIZERS: --------------------------------------------------------------------------
        
        protected virtual void Awake()
        {
            this.m_Args = new Args(this.gameObject);
            this.m_TriggerEvent?.OnAwake(this);
        }
        
        protected virtual void Start()
        {
            this.m_TriggerEvent?.OnStart(this);
        }

        // BEHAVIOR: ------------------------------------------------------------------------------
        
        protected virtual void OnEnable()
        {
            this.m_TriggerEvent?.OnEnable(this);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            this.m_TriggerEvent?.OnDisable(this);
        }

        protected virtual void Update()
        {
            this.m_TriggerEvent?.OnUpdate(this);
        }
        
        protected virtual void FixedUpdate()
        {
            this.m_TriggerEvent?.OnFixedUpdate(this);
        }

        protected virtual void LateUpdate()
        {
            this.m_TriggerEvent?.OnLateUpdate(this);
        }
        
        protected void OnBecameVisible()
        {
            this.m_TriggerEvent?.OnBecameVisible(this);
        }
        
        protected void OnBecameInvisible()
        {
            this.m_TriggerEvent?.OnBecameInvisible(this);
        }
        
        protected void OnApplicationFocus(bool hasFocus)
        {
            this.m_TriggerEvent?.OnApplicationFocus(this, hasFocus);
        }
        
        protected void OnApplicationPause(bool pauseStatus)
        {
            this.m_TriggerEvent?.OnApplicationPause(this, pauseStatus);
        }
        
        protected void OnApplicationQuit()
        {
            this.m_TriggerEvent?.OnApplicationQuit(this);
        }

        // PHYSICS 3D: ----------------------------------------------------------------------------

        protected void OnCollisionEnter(Collision c)
        {
            this.m_TriggerEvent?.OnCollisionEnter3D(this, c);
        }

        protected void OnCollisionExit(Collision c)
        {
            this.m_TriggerEvent?.OnCollisionExit3D(this, c);
        }

        protected void OnCollisionStay(Collision c)
        {
            this.m_TriggerEvent?.OnCollisionStay3D(this, c);
        }

        protected void OnTriggerEnter(Collider c)
        {
            this.m_TriggerEvent?.OnTriggerEnter3D(this, c);
        }

        protected void OnTriggerExit(Collider c)
        {
            this.m_TriggerEvent?.OnTriggerExit3D(this, c);
        }

        protected void OnTriggerStay(Collider c)
        {
            this.m_TriggerEvent?.OnTriggerStay3D(this, c);
        }

        protected void OnJointBreak(float force)
        {
            this.m_TriggerEvent?.OnJointBreak3D(this, force);
        }

        // PHYSICS 2D: ----------------------------------------------------------------------------

        protected void OnCollisionEnter2D(Collision2D c)
        {
            this.m_TriggerEvent?.OnCollisionEnter2D(this, c);
        }

        protected void OnCollisionExit2D(Collision2D c)
        {
            this.m_TriggerEvent?.OnCollisionExit2D(this, c);
        }

        protected void OnCollisionStay2D(Collision2D c)
        {
            this.m_TriggerEvent?.OnCollisionStay2D(this, c);
        }

        protected void OnTriggerEnter2D(Collider2D c)
        {
            this.m_TriggerEvent?.OnTriggerEnter2D(this, c);
        }

        protected void OnTriggerExit2D(Collider2D c)
        {
            this.m_TriggerEvent?.OnTriggerExit2D(this, c);
        }

        protected void OnTriggerStay2D(Collider2D c)
        {
            this.m_TriggerEvent?.OnTriggerStay2D(this, c);
        }

        protected void OnJointBreak2D(Joint2D joint)
        {
            this.m_TriggerEvent?.OnJointBreak2D(this, joint);
        }

        // INPUT: ---------------------------------------------------------------------------------

        protected void OnMouseDown()
        {
            this.m_TriggerEvent?.OnMouseDown(this);
        }

        protected void OnMouseUp()
        {
            this.m_TriggerEvent?.OnMouseUp(this);
        }

        protected void OnMouseUpAsButton()
        {
            this.m_TriggerEvent?.OnMouseUpAsButton(this);
        }

        protected void OnMouseEnter()
        {
            this.m_TriggerEvent?.OnMouseEnter(this);
        }

        protected void OnMouseOver()
        {
            this.m_TriggerEvent?.OnMouseOver(this);
        }

        protected void OnMouseExit()
        {
            this.m_TriggerEvent?.OnMouseExit(this);
        }

        protected void OnMouseDrag()
        {
            this.m_TriggerEvent?.OnMouseDrag(this);
        }
        
        // UI: ------------------------------------------------------------------------------------
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            this.m_TriggerEvent?.OnPointerEnter(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            this.m_TriggerEvent?.OnPointerExit(this);
        }
        
        public void OnSelect(BaseEventData eventData)
        {
            this.m_TriggerEvent?.OnSelect(this);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            this.m_TriggerEvent?.OnDeselect(this);
        }
        
        // GIZMOS: --------------------------------------------------------------------------------

        protected virtual void OnDrawGizmos()
        {
            #if UNITY_EDITOR
            if (UnityEditor.PrefabUtility.IsPartOfPrefabAsset(this.gameObject)) return;
            #endif
            
            this.m_TriggerEvent?.OnDrawGizmos(this);
        }

        protected virtual void OnDrawGizmosSelected()
        {
            #if UNITY_EDITOR
            if (UnityEditor.PrefabUtility.IsPartOfPrefabAsset(this.gameObject)) return;
            #endif
            
            this.m_TriggerEvent?.OnDrawGizmosSelected(this);
        }
        
        // SIGNALS: -------------------------------------------------------------------------------

        void ISignalReceiver.OnReceiveSignal(SignalArgs args)
        {
            
            this.m_TriggerEvent?.OnReceiveSignal(this, args);
        }
        
        // CUSTOM CALLBACKS: ----------------------------------------------------------------------

        /// <summary>
        /// Attempts to invoke a command that can be interpreted by the Event. If the event
        /// is listening to this event, it will be executed.
        /// </summary>
        /// <param name="args">The command name and optional target to execute</param>
        public void OnReceiveCommand(CommandArgs args)
        {
            this.m_TriggerEvent?.OnReceiveCommand(this, args);
        }
        
        /// <summary>
        /// Attempts to invoke a command that can be interpreted by the Event. If the event
        /// is listening to this event, it will be executed.
        /// </summary>
        /// <param name="command">The name of the command to execute</param>
        [Obsolete("Soon to deprecate. Use OnReceiveCommand(CommandArgs) instead")]
        public void OnReceiveCommand(PropertyName command)
        {
            this.m_TriggerEvent?.OnReceiveCommand(this, new CommandArgs(command));
        }
        
        ///////////////////////////////////////////////////////////////////////////////////////////
        // PHYSICS METHODS: -----------------------------------------------------------------------

        public void RequireRigidbody()
        {
            if (this.m_Collider3D == null) this.m_Collider3D = this.GetComponent<Collider>();
            if (this.m_Collider2D == null) this.m_Collider2D = this.GetComponent<Collider2D>();
            
            if (this.m_Collider3D != null) this.RequireRigidbody3D();
            if (this.m_Collider2D != null) this.RequireRigidbody2D();
        }
        
        private void RequireRigidbody3D()
        {
            if (this.m_Rigidbody3D != null) return;
            
            this.m_Rigidbody3D = this.GetComponent<Rigidbody>();
            if (this.m_Rigidbody3D != null) return;
            
            if (this.m_Collider3D == null)
            {
                this.m_Collider3D = this.GetComponent<Collider>();
                if (this.m_Collider3D == null) return;
            }

            this.m_Rigidbody3D = this.gameObject.AddComponent<Rigidbody>();
            this.m_Rigidbody3D.isKinematic = true;
            this.m_Rigidbody3D.hideFlags = HideFlags.HideInInspector;
        }
        
        private void RequireRigidbody2D()
        {
            if (this.m_Rigidbody2D != null) return;
            
            this.m_Rigidbody2D = this.GetComponent<Rigidbody2D>();
            if (this.m_Rigidbody2D != null) return;
            
            if (this.m_Collider2D == null)
            {
                this.m_Collider2D = this.GetComponent<Collider2D>();
                if (this.m_Collider2D == null) return;
            }

            this.m_Rigidbody2D = this.gameObject.AddComponent<Rigidbody2D>();
            this.m_Rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
            this.m_Rigidbody2D.hideFlags = HideFlags.HideInInspector;
        }
        
        ///////////////////////////////////////////////////////////////////////////////////////////
        // INTERACTION: ---------------------------------------------------------------------------

        internal void RequireInteractionTracker()
        {
            InteractionTracker tracker = InteractionTracker.Require(this.gameObject);
            
            this.m_Interactive = tracker;
            
            tracker.EventInteract -= this.OnInteract;
            tracker.EventInteract += this.OnInteract;
        }

        private void OnInteract(Character character, IInteractive interactive)
        {
            this.EventAfterExecute -= this.OnStopInteraction;
            this.EventAfterExecute += this.OnStopInteraction;
            
            if (this.m_TriggerEvent?.OnInteract(this, character) ?? false) return;
            this.m_Interactive?.Stop();
        }

        private void OnStopInteraction()
        {
            this.EventAfterExecute -= this.OnStopInteraction;
            this.m_Interactive?.Stop();
        }
    }
}
