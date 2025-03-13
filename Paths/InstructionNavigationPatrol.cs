using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using UnityEngine;
using GameCreator.Runtime.Variables;

namespace GameCreator.Runtime.VisualScripting
{
    [Version(1, 0, 2)]

    [Title("Patrol Markers Using Local List")]
    [Description("Instructs the Character to Patrol a list of Markers stored in LocalListVariables.")]

    [Category("Path/Patrol Markers Localist")]

    [Parameter("Stop Distance", "Distance to the destination that the Character considers it has reached the target")]
    [Parameter("Emergency Stop", "This bool can be triggered via a variable but then has to be toggled to false by the same trigger")]

    [Keywords("Walk", "Run", "Position", "Location", "Destination", "Patrol")]
    [Image(typeof(IconCharacterWalk), ColorTheme.Type.Blue)]

    [Serializable]
    public class InstructionNavigationPatrol : TInstructionCharacterNavigation
    {
        [Serializable]
        public class NavigationOptions
        {
            [SerializeField] public bool m_WaitToArrive = true;
            [SerializeField] public PropertyGetDecimal m_StopDistance = GetDecimalConstantZero.Create;
            [SerializeField] public PropertyGetBool EmergencyStop = GetBoolValue.Create(false);
            [SerializeField] public PropertyGetDecimal m_MoveTimeout = GetDecimalConstantZero.Create; // Max time to wait before timeout

            [NonSerialized] public bool m_MovementComplete;
            [SerializeField] public PropertyGetInteger m_Repeat = GetDecimalInteger.Create(1);

            public async Task<bool> Await(bool doit, Character character, Args args)
            {
                if (m_WaitToArrive)
                {
                    float startTime = global::UnityEngine.Time.time; // Ensure using Unity's Time class

                    while (!this.m_MovementComplete)
                    {
                        await Task.Yield();

                        bool m_EmergencyStop = EmergencyStop.Get(args);
                        if (m_EmergencyStop)
                        {
                            character.Motion.StopToDirection(1);
                            Debug.LogWarning("Emergency stop triggered. Stopping character.");
                            return false; // Return false to indicate failure
                        }

                        // Check if movement has timed out
                        float timeoutDuration = (float)m_MoveTimeout.Get(args);
                        if (timeoutDuration > 0 && global::UnityEngine.Time.time - startTime >= timeoutDuration)
                        {
                            character.Motion.StopToDirection(1);
                            Debug.LogWarning("Movement timed out. Stopping character.");
                            return false; // Return false to indicate failure
                        }
                    }
                }

                return this.m_MovementComplete;
            }
        }

        [SerializeField] private LocalListVariables m_Ways; // Reference to LocalListVariables
        [SerializeField] private NavigationOptions m_Options = new NavigationOptions();

        public override string Title => $"Move {this.m_Character} to Markers in LocalListVariables";

        protected override async Task Run(Args args)
        {
            Character character = this.m_Character.Get<Character>(args);
            if (character == null || m_Ways == null) return;

            int m_Repeat = (int)m_Options.m_Repeat.Get(args);

            for (int i = -1; i < m_Repeat; i++)
            {
                for (int index = 0; index < m_Ways.Count; index++)
                {
                    GameObject markerObj = m_Ways.Get(index) as GameObject;
                    if (markerObj == null) continue;

                    Marker marker = markerObj.GetComponent<Marker>();
                    if (marker == null) continue;

                    Location location = new Location(marker);
                    character.Motion.MoveToLocation(
                        location,
                        (float)this.m_Options.m_StopDistance.Get(args),
                        this.OnFinish);

                    m_Options.m_MovementComplete = false;

                    if (this.m_Options.m_WaitToArrive)
                    {
                        bool success = await this.m_Options.Await(m_Options.m_MovementComplete, character, args);

                        // If movement failed, exit and return early
                        if (!success)
                        {
                            Debug.LogWarning("Character movement failed. Exiting patrol.");
                            return; // Exit the method to indicate failure
                        }
                    }
                }
            }
        }

        private void OnFinish(Character character, bool success)
        {
            this.m_Options.m_MovementComplete = success;
        }
    }
}
