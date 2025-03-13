using System;
using UnityEngine;
using GameCreator.Runtime.Common;

namespace NinjutsuGames.StateMachine.Runtime
{
    [Serializable]
    public class DebugResetNode : BaseGameCreatorNode
    {
        // Debug properties
        [SerializeField] private bool debugMode = true;
        [SerializeField] private string debugMessage = "";
        [SerializeField] private int executionCount = 0;

        public override string name => "Debug Reset";
        public override bool needsInspector => true;
        public override bool hideControls => false;

        [Input("In")] public NodePort input;
        [Output("Out")] public NodePort output;

        protected override void Process(Args args)
        {
            if (!Application.isPlaying) return;

            Context = args.Self;

            if (!CanExecute(args.Self)) return;

            // Debug handling
            if (debugMode)
            {
                executionCount++;
                debugMessage = $"Executed {executionCount} times";
            }

            // Start execution
            OnStartRunning(args.Self);

            // Process node logic
            ProcessNode(args);

            // Run child nodes
            RunChildNodes(args);

            // Stop execution
            OnStopRunning(args.Self);

            // Reset after execution
            ResetNode();
        }

        protected virtual void ProcessNode(Args args)
        {
            // Override this in derived classes to add specific node behavior
        }

        private void ResetNode()
        {
            // Reset runtime values
            IsContextRunning.Clear();
            IsContextDisabled.Clear();

            if (debugMode)
            {
                debugMessage = "Node Reset";
            }
        }

        protected override void Enable()
        {
            base.Enable();
            executionCount = 0;
            debugMessage = "Node Enabled";
        }

        protected override void Disable()
        {
            base.Disable();
            debugMessage = "Node Disabled";
        }

        protected override void StopRunning(GameObject context)
        {
            base.StopRunning(context);
            if (debugMode)
            {
                debugMessage = "Node Stopped";
            }
        }
    }
}