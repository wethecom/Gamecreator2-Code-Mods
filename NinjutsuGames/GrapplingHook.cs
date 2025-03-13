using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Common.Audio;
using GameCreator.Runtime.Melee;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Movement
{
    [CreateAssetMenu(
        fileName = "GrapplingHook", 
        menuName = "Game Creator/Movement/Grappling Hook",
        order    = 50
    )]
    
    [Icon(EditorPaths.PACKAGES + "Movement/Editor/Gizmos/GizmoGrapplingHook.png")]
    
    [Serializable]
    public class GrapplingHook : ScriptableObject, IStageGizmos
    {
        private const int LAYER_TIME_SCALE = 999;
        
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField] private PropertyGetString m_Title = GetStringString.Create;
        [SerializeField] private PropertyGetString m_Description = GetStringTextArea.Create();

        [SerializeField] private PropertyGetSprite m_Icon = GetSpriteNone.Create;
        [SerializeField] private PropertyGetColor m_Color = GetColorColorsWhite.Create;

        [SerializeField] private AnimationClip m_LaunchAnimation;
        [SerializeField] private AnimationClip m_TravelAnimation;
        [SerializeField] private AnimationClip m_ArrivalAnimation;
        [SerializeField] private AvatarMask m_Mask;

        [SerializeField] private float m_GrappleSpeed = 10f;
        [SerializeField] private float m_MaxGrappleDistance = 50f;
        [SerializeField] private float m_TransitionIn = 0.1f;
        [SerializeField] private float m_TransitionBetween = 0.1f;
        [SerializeField] private float m_TransitionOut = 0.25f;
        
        [SerializeField] private PropertyGetGameObject m_GrappleEffectPrefab = GetGameObjectNone.Create();
        [SerializeField] private PropertyGetAudio m_LaunchSound = GetAudioNone.Create;
        [SerializeField] private PropertyGetAudio m_TravelSound = GetAudioNone.Create;
        [SerializeField] private PropertyGetAudio m_ArrivalSound = GetAudioNone.Create;

        [SerializeField] private RunInstructionsList m_OnStart = new RunInstructionsList();
        [SerializeField] private RunInstructionsList m_OnTravel = new RunInstructionsList();
        [SerializeField] private RunInstructionsList m_OnArrival = new RunInstructionsList();
        [SerializeField] private RunInstructionsList m_OnFinish = new RunInstructionsList();

        [SerializeField] private RunConditionsList m_CanGrapple = new RunConditionsList();
        
        // PROPERTIES: ----------------------------------------------------------------------------

        public AnimationClip LaunchAnimation => this.m_LaunchAnimation;
        public AnimationClip TravelAnimation => this.m_TravelAnimation;
        public AnimationClip ArrivalAnimation => this.m_ArrivalAnimation;
        public AvatarMask Mask => this.m_Mask;

        public float GrappleSpeed => this.m_GrappleSpeed;
        public float MaxGrappleDistance => this.m_MaxGrappleDistance;
        
        public float TransitionIn => this.m_TransitionIn;
        public float TransitionBetween => this.m_TransitionBetween;
        public float TransitionOut => this.m_TransitionOut;
        
        [field: SerializeField] public string EditorModelPath { get; set; }

        // GETTER METHODS: ------------------------------------------------------------------------

        public string GetName(Args args) => this.m_Title.Get(args);
        public string GetDescription(Args args) => this.m_Description.Get(args);
        
        public Sprite GetSprite(Args args) => this.m_Icon.Get(args);
        public Color GetColor(Args args) => this.m_Color.Get(args);

        public GameObject GetGrappleEffect(Args args) => this.m_GrappleEffectPrefab.Get(args);
        public AudioClip GetLaunchSound(Args args) => this.m_LaunchSound.Get(args);
        public AudioClip GetTravelSound(Args args) => this.m_TravelSound.Get(args);
        public AudioClip GetArrivalSound(Args args) => this.m_ArrivalSound.Get(args);

        // PUBLIC METHODS: ------------------------------------------------------------------------
        internal static readonly Vector2 PITCH = new Vector2(0.9f, 1.1f);
        public bool CanGrapple(Args args)
        {
            return this.m_CanGrapple.Check(args);
        }
        
        public void ExecuteGrapple(Character character, Vector3 targetPosition, ICancellable cancel, Args args)
        {
            if (character == null) return;
            
            Vector3 startPosition = character.transform.position;
            Vector3 direction = targetPosition - startPosition;
            float distance = direction.magnitude;
            
            if (distance > this.m_MaxGrappleDistance)
            {
                Debug.LogWarning($"Grapple target is too far: {distance} > {this.m_MaxGrappleDistance}");
                return;
            }
            
            if (!this.CanGrapple(args))
            {
                Debug.LogWarning("Cannot grapple due to conditions");
                return;
            }
            
            // Start the grappling sequence
            _ = this.m_OnStart.Run(args);
            
            // Play launch animation
            if (this.m_LaunchAnimation != null)
            {
                float launchDuration = this.m_LaunchAnimation.length;
                ConfigGesture launchConfig = new ConfigGesture(
                    0f, launchDuration, 1f, true,
                    this.m_TransitionIn, this.m_TransitionBetween
                );
                
                _ = character.Gestures.CrossFade(
                    this.m_LaunchAnimation, this.m_Mask, BlendMode.Blend, 
                    launchConfig, true
                );
                
                // Play launch sound
                AudioClip launchSound = this.GetLaunchSound(args);
                if (launchSound != null)
                {
                   AudioConfigSoundEffect soundConfig = AudioConfigSoundEffect.Create(
                    1f, PITCH, 0f,
                    character.Time.UpdateTime, SpatialBlending.Spatial, args.Self
                );


                    _ = AudioManager.Instance.SoundEffect.Play(launchSound, soundConfig, args);
                }
                
                // Create grapple effect
                GameObject grappleEffect = this.GetGrappleEffect(args);
                if (grappleEffect != null)
                {
                    GameObject instance = PoolManager.Instance.Pick(
                        grappleEffect,
                        character.transform.position,
                        Quaternion.LookRotation(direction),
                        5,
                        distance / this.m_GrappleSpeed + launchDuration
                    );
                    
                    // You could add a line renderer component to visualize the grapple line
                    if (instance.TryGetComponent<LineRenderer>(out LineRenderer line))
                    {
                        line.SetPosition(0, character.transform.position);
                        line.SetPosition(1, targetPosition);
                    }
                }
                
                // Wait for launch animation to complete before starting travel
                character.StartCoroutine(this.TravelAfterLaunch(
                    character, 
                    startPosition, 
                    targetPosition, 
                    launchDuration, 
                    cancel, 
                    args
                ));
            }
            else
            {
                // No launch animation, start travel immediately
                this.StartTravel(character, startPosition, targetPosition, cancel, args);
            }
        }
        
        private System.Collections.IEnumerator TravelAfterLaunch(
            Character character, 
            Vector3 startPosition, 
            Vector3 targetPosition, 
            float delay, 
            ICancellable cancel, 
            Args args)
        {
            yield return new WaitForSeconds(delay);
            
            if (cancel != null && cancel.IsCancelled) yield break;
            
            this.StartTravel(character, startPosition, targetPosition, cancel, args);
        }
        
        private void StartTravel(
            Character character, 
            Vector3 startPosition, 
            Vector3 targetPosition, 
            ICancellable cancel, 
            Args args)
        {
            _ = this.m_OnTravel.Run(args);
            
            // Calculate travel time based on distance and speed
            Vector3 direction = targetPosition - startPosition;
            float distance = direction.magnitude;
            float travelTime = distance / this.m_GrappleSpeed;
            
            // Play travel animation
            if (this.m_TravelAnimation != null)
            {
                ConfigGesture travelConfig = new ConfigGesture(
                    0f, travelTime, 1f, true,
                    this.m_TransitionBetween, this.m_TransitionBetween
                );
                
                _ = character.Gestures.CrossFade(
                    this.m_TravelAnimation, this.m_Mask, BlendMode.Blend, 
                    travelConfig, true
                );
            }
            
            // Play travel sound
            AudioClip travelSound = this.GetTravelSound(args);
            if (travelSound != null)
            {
                AudioConfigSoundEffect soundConfig = AudioConfigSoundEffect.Create(
                    1f, PITCH, 0f,
                    character.Time.UpdateTime, SpatialBlending.Spatial, args.Self
                );
               
                _ = AudioManager.Instance.SoundEffect.Play(travelSound, soundConfig, args);
            }
            
            // Start the actual movement
            character.StartCoroutine(this.MoveCharacterToTarget(
                character, 
                startPosition, 
                targetPosition, 
                travelTime, 
                cancel, 
                args
            ));
        }
        
        private System.Collections.IEnumerator MoveCharacterToTarget(
            Character character, 
            Vector3 startPosition, 
            Vector3 targetPosition, 
            float duration, 
            ICancellable cancel, 
            Args args)
        {
            // Temporarily disable character controller
            bool wasControllerEnabled = character.Player.IsControllable;
            //character.Driver.SetController(false);
            character.Player.IsControllable = false;

            // Make character look at target
            character.transform.rotation = Quaternion.LookRotation(targetPosition - startPosition);
            
            float elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                if (cancel != null && cancel.IsCancelled) break;
                
                float t = elapsedTime / duration;
                
                // Use a smooth curve for movement (ease in/out)
                float smoothT = Mathf.SmoothStep(0f, 1f, t);
                
                // Move the character
                character.transform.position = Vector3.Lerp(startPosition, targetPosition, smoothT);
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            // Ensure final position is reached
            if (!(cancel != null && cancel.IsCancelled))
            {
                character.transform.position = targetPosition;
                
                // Arrival sequence
                this.OnArrival(character, targetPosition, cancel, args);
            }
            
            // Re-enable character controller
           // character.Driver.SetController(wasControllerEnabled);
            character.Player.IsControllable = wasControllerEnabled;
        }
        
        private void OnArrival(Character character, Vector3 position, ICancellable cancel, Args args)
        {
            _ = this.m_OnArrival.Run(args);
            
            // Play arrival sound
            AudioClip arrivalSound = this.GetArrivalSound(args);
            if (arrivalSound != null)
            {
                AudioConfigSoundEffect soundConfig = AudioConfigSoundEffect.Create(
                    1f, PITCH, 0f,
                    character.Time.UpdateTime, SpatialBlending.Spatial, args.Self
                );
                
                _ = AudioManager.Instance.SoundEffect.Play(arrivalSound, soundConfig, args);
            }
            
            // Play arrival animation
            if (this.m_ArrivalAnimation != null)
            {
                float arrivalDuration = this.m_ArrivalAnimation.length;
                ConfigGesture arrivalConfig = new ConfigGesture(
                    0f, arrivalDuration, 1f, true,
                    this.m_TransitionBetween, this.m_TransitionOut
                );
                
                _ = character.Gestures.CrossFade(
                    this.m_ArrivalAnimation, this.m_Mask, BlendMode.Blend, 
                    arrivalConfig, true
                );
                
                // Wait for arrival animation to complete before finishing
                character.StartCoroutine(this.FinishAfterArrival(
                    character, 
                    arrivalDuration, 
                    cancel, 
                    args
                ));
            }
            else
            {
                // No arrival animation, finish immediately
                this.FinishGrapple(character, cancel, args);
            }
        }
        
        private System.Collections.IEnumerator FinishAfterArrival(
            Character character, 
            float delay, 
            ICancellable cancel, 
            Args args)
        {
            yield return new WaitForSeconds(delay);
            
            if (cancel != null && cancel.IsCancelled) yield break;
            
            this.FinishGrapple(character, cancel, args);
        }
        
        private void FinishGrapple(Character character, ICancellable cancel, Args args)
        {
            _ = this.m_OnFinish.Run(args);
            
            if (cancel != null)
            {
               // cancel.IsCancelled = true;
            }
        }

        // STAGE GIZMOS: --------------------------------------------------------------------------
        
        public void StageGizmos(StagingGizmos stagingGizmos)
        {
            // Draw a sphere representing max grapple distance
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(stagingGizmos.gameObject.transform.position, this.m_MaxGrappleDistance);
        }
    }
}