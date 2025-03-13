using UnityEngine;
using UnityEditor.Animations;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Variables;
using GameCreator.Runtime.VisualScripting;
using System;
using System.Threading.Tasks;

namespace GameCreator.Runtime.VisualScripting
{
    [Title("Change Animator Layer")]
    [Description("Changes the weight of an Animator Layer")]
    [Image(typeof(IconAnimator), ColorTheme.Type.Blue)]
    [Category("Animations/Change Animator Layer")]
    [Parameter("Layer Index", "The Animator's Layer index that's being modified")]
    [Parameter("Weight", "The target Animator layer weight")]
    [Parameter("Duration", "How long it takes to perform the transition")]
    [Parameter("Easing", "The change rate of the parameter over time")]
    [Parameter("Wait to Complete", "Whether to wait until the transition is finished")]
    [Parameter("Animation Name", "The name of the animation clip to add")]
    [Parameter("Avatar Mask Name", "The name of the avatar mask to add")]
    [Keywords("Weight")]
    [Serializable]
    public class InstructionAnimatorLayer : Instruction
    {
        [SerializeField] private PropertyGetGameObject gameObjectA = new PropertyGetGameObject();
        [SerializeField] private int m_LayerIndex = 1;
        [SerializeField] private ChangeDecimal m_Weight = new ChangeDecimal(1f);
        [SerializeField] private Transition m_Transition = new Transition();
        [SerializeField] private string m_AnimationName = "MachinimaAnimation";
        [SerializeField] private string m_AvatarMaskName = "MachinimaMask";
        [SerializeField] private AvatarMask avatarMask;
        [SerializeField] private AnimationClip animationClip;
        public override string Title => $"Change Layer Weight {this.m_LayerIndex}";

        protected override async Task Run(Args args)
        {
            GameObject gameObject = this.gameObjectA.Get(args);
            if (gameObject == null) return;

            Animator animator = gameObject.Get<Animator>();
            if (animator == null) return;

            AnimatorController animatorController = animator.runtimeAnimatorController as AnimatorController;
            if (animatorController == null) return;

            animationClip = Resources.Load<AnimationClip>(this.m_AnimationName);
            if (animationClip == null) return;

           avatarMask = Resources.Load<AvatarMask>(this.m_AvatarMaskName);
            if (avatarMask == null) return;

            // Add the animation to the Animator controller
            AnimatorControllerLayer layer = animatorController.layers[0];
            AnimatorStateMachine stateMachine = layer.stateMachine;
            AnimatorState state = stateMachine.AddState("MachinimaState");
            state.motion = animationClip;

            // Add a new layer to the Animator controller for the avatar mask
            AnimatorControllerLayer maskLayer = new AnimatorControllerLayer();
            maskLayer.avatarMask = avatarMask;
            animatorController.AddLayer(maskLayer);

            if (this.m_LayerIndex >= animator.layerCount) return;

            float valueSource = animator.GetLayerWeight(this.m_LayerIndex);
            float valueTarget = (float)this.m_Weight.Get(valueSource, args);

            ITweenInput tween = new TweenInput<float>(
                valueSource,
                valueTarget,
                this.m_Transition.Duration,
                (a, b, t) => animator.SetLayerWeight(this.m_LayerIndex, Mathf.Lerp(a, b, t)),
                Tween.GetHash(typeof(Animator), "weight"),
                this.m_Transition.EasingType,
                this.m_Transition.Time
            );

            Tween.To(gameObject, tween);
            if (this.m_Transition.WaitToComplete) await this.Until(() => tween.IsFinished);
        }
    }
}


