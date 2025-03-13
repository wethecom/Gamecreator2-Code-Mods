using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using UnityEngine;
using GameCreator.Runtime.Characters;

namespace GameCreator.Runtime.VisualScripting
{


    [Title("Has State in Layer")]
    [Description("Returns true if the Character has a State running at the specified layer index")]

    [Category("Characters/Animation/Has State in Layer")]

    [Parameter("Layer", "The layer in which the Character may have a State running")]

    [Keywords("Characters", "Animation", "Animate", "State", "Play")]
    [Image(typeof(IconCharacterState), ColorTheme.Type.Red)]
    [Serializable]
    public class CompareState : Instruction
    {
        [SerializeField] private PropertyGetGameObject m_Character = GetGameObjectPlayer.Create();
        [SerializeField] private StateData m_State = new StateData(StateData.StateType.State);

        [SerializeField] private PropertyGetInteger m_Layer = new PropertyGetInteger(1);
          [Header("True")]
         [SerializeField] private InstructionList m_OnTrue = new InstructionList();  // Action for when condition is true
        [Header("False")]
         [SerializeField] private InstructionList m_OnFalse = new InstructionList(); // Actio
        // PROPERTIES: ----------------------------------------------------------------------------



        // RUN METHOD: ---------

        protected override async Task Run(Args args)
        {
            
            Character character = this.m_Character.Get<Character>(args);
            int layer = (int)this.m_Layer.Get(args);

            bool isAvailable = character.States.IsAvailable(layer);


            if (isAvailable)
            {
                 await m_OnTrue.Run(args);  // Run actions if true
               

            }
            else
            {
                 await m_OnFalse.Run(args); // Run actions if false
                
               

            }

           // return DefaultResult;
        }
    }
}

