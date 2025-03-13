using System.Collections.Generic;
using UnityEngine;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using GameCreator.Runtime.Characters;
using System;

namespace GameCreator.Runtime.Perception
{
    [Version(1, 0, 3)]
    [Title("Flashlight Controller")]
    [Description("Controls a flashlight based on smoothed ambient luminance readings with hysteresis to prevent flickering.")]

    [Category("Perception/See/Flashlight Controller")]
    [Serializable]
    public class FlashlightController : GameCreator.Runtime.VisualScripting.Event
    {
        // Configuration Parameters
        [SerializeField]
        private PropertyGetGameObject m_Target = GetGameObjectPlayer.Create();

        [SerializeField]
        private float m_ThresholdValue = 0.5f;

        [SerializeField]
        private float m_Hysteresis = 0.05f; // Buffer to prevent rapid toggling

        [SerializeField]
        private Light controlledLight; // Reference to the Light component

        [SerializeField]
        private int smoothingFrames = 10; // Number of frames to average for smoothing

        // Internal State
        private Queue<float> luminanceValues = new Queue<float>(); // Store recent luminance values
        private float smoothedLuminance = 0f;

        protected override void OnStart(Trigger trigger)
        {
            base.OnStart(trigger);
            UpdateLightState(trigger);
        }

        protected override void OnUpdate(Trigger trigger)
        {
            base.OnUpdate(trigger);
            UpdateLightState(trigger);
        }

        private void UpdateLightState(Trigger trigger)
        {
            GameObject target = this.m_Target.Get(trigger);
            if (target == null || controlledLight == null) return;

            float currentLuminance = LuminanceManager.Instance.LuminanceAt(target.transform);
            UpdateLuminanceHistory(currentLuminance);

            // Hysteresis logic
            if (controlledLight.enabled && smoothedLuminance > m_ThresholdValue + m_Hysteresis)
            {
                controlledLight.enabled = false;
            }
            else if (!controlledLight.enabled && smoothedLuminance < m_ThresholdValue - m_Hysteresis)
            {
                controlledLight.enabled = true;
            }
        }

        private void UpdateLuminanceHistory(float newLuminance)
        {
            luminanceValues.Enqueue(newLuminance);
            if (luminanceValues.Count > smoothingFrames)
            {
                luminanceValues.Dequeue();
            }

            smoothedLuminance = 0f;
            foreach (var value in luminanceValues)
            {
                smoothedLuminance += value;
            }
            smoothedLuminance /= luminanceValues.Count;
        }
    }
}
