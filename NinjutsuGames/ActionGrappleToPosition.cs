using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Inventory;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;


namespace GameCreator.Runtime.Movement
{
    [Title("Grapple to Position")]
    [Description("Makes a character grapple to a target position using a Grappling Hook asset")]
    [Image(typeof(IconAimTarget), ColorTheme.Type.Green)]

    [Category("Movement/Grappling Hook/Grapple to Position")]
    
    [Parameter("Character", "The character that performs the grapple")]
    [Parameter("Target Position", "The position where the character will grapple to")]
    [Parameter("Grappling Hook", "The Grappling Hook asset to use")]
    [Parameter("Wait to Complete", "Whether to wait until the grapple completes before continuing")]

    [Keywords("Swing", "Hook", "Rope", "Zip", "Line", "Spider", "Web", "Batman")]
    
    [Serializable]
    public class ActionGrappleToPosition : Instruction
    {
        [SerializeField] private PropertyGetGameObject m_Character = GetGameObjectPlayer.Create();
        [SerializeField] private PropertyGetPosition m_TargetPosition = GetPositionTarget.Create();
        [SerializeField] private GrapplingHook grapplingHook = new();
        [SerializeField] private bool m_WaitToComplete = true;

        private bool m_IsComplete = false;
        private ICancellable m_Cancel;
        protected override Task Run(Args args)
        {
            this.m_IsComplete = false;
            this.m_Cancel = new CancelGrapple();

            Character character = this.m_Character.Get(args)?.Get<Character>();
            if (character == null)
            {
                this.m_IsComplete = true;
                return DefaultResult;
            }

            Vector3 targetPosition = this.m_TargetPosition.Get(args);
          //  GrapplingHook grapplingHook = this.m_GrapplingHook.Get(args) as GrapplingHook;

            if (grapplingHook != null)
            {
                grapplingHook.ExecuteGrapple(character, targetPosition, this.m_Cancel, args);

                if (!this.m_WaitToComplete)
                {
                    this.m_IsComplete = true;
                    return DefaultResult;
                }
                else
                {
                    character.StartCoroutine(this.WaitForCompletion());
                }
            }
            else
            {
                this.m_IsComplete = true;
                return DefaultResult;
            }
            return DefaultResult;
        }
       

        private System.Collections.IEnumerator WaitForCompletion()
        {
            while (!this.m_Cancel.IsCancelled)
            {
                yield return null;
            }
            
            this.m_IsComplete = true;
        }
/*
        protected override bool OnUpdate(Args args)
        {
            return this.m_IsComplete;

        }

        protected override void OnStop(Args args)
        {
            if (this.m_Cancel != null)
            {
                this.m_Cancel.IsCancelled = true;
            }
        }
*/
        private class CancelGrapple : ICancellable
        {
            public bool IsCancelled { get; set; }
        }
    }
}