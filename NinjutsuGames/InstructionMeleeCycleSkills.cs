using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Runtime.Melee
{
    [Version(0, 0, 2)]
    
    [Title("Cycle Through Melee Skills")]
    [Description("Cycles through an array of Skills and uses it,same as play but uses an array")]

    [Category("Melee/Skills/Cycle Through Melee and play Skill")]
    
    [Parameter("Character", "The Character that plays the Skills")]
    [Parameter("Target", "Optional reference object set as the Target of the Skills")]
    [Parameter("Skills", "Array of Skill assets to cycle through")] 

    [Keywords("Melee", "Combat", "Cycle", "Sequence")]
    [Image(typeof(IconMeleeSkill), ColorTheme.Type.Green)]
    
    [Serializable]
    public class InstructionMeleeCycleSkills : Instruction
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField] private PropertyGetGameObject m_Character = GetGameObjectPlayer.Create();
        [SerializeField] private PropertyGetGameObject m_Target = GetGameObjectNone.Create();
        [SerializeField] private PropertyGetWeapon m_Weapon = GetWeaponMeleeInstance.Create();
        
        // Array of skills to cycle through
        [SerializeField] private PropertyGetSkill[] m_Skills = new PropertyGetSkill[0];
        
        // Current index in the skills array
        private static int currentSkillIndex = 0;

        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => $"Cycle Skills on {this.m_Character}";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override Task Run(Args args)
        {
            Character character = this.m_Character.Get<Character>(args);
            if (character == null) return DefaultResult;

            GameObject target = this.m_Target.Get(args);
            
            // Check if we have any skills
            if (m_Skills == null || m_Skills.Length == 0) return DefaultResult;

            // Get the current skill and increment counter
            Skill currentSkill = m_Skills[currentSkillIndex].Get(args);
            currentSkillIndex = (currentSkillIndex + 1) % m_Skills.Length; // Cycle to next skill

            if (currentSkill == null) return DefaultResult;

            MeleeWeapon weapon = this.m_Weapon.Get(args) as MeleeWeapon;

            character.Combat
                .RequestStance<MeleeStance>()
                .PlaySkill(weapon, currentSkill, target);
            
            return DefaultResult;
        }
    }
}