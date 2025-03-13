/* GameCreator 2 - Simple Debug Log Instruction 
 * This script creates a custom instruction to print a message in the Unity Console.
 */

using UnityEngine;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
/* GameCreator 2 - Simple Debug Log Instruction 
 * This script creates a custom instruction to print a message in the Unity Console.
 */

using UnityEngine;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Melee;
using GameCreator.Runtime.Characters;

[Version(1, 0, 0)]
[Title("Debug Log Message")]
[Category("Debug/Log Message")]

[Image(typeof(IconConsole), ColorTheme.Type.TextLight)]
[Description("Logs a message to the Unity Console")]

[Parameter("Message", "The text to print in the console")]
[Keywords("Print", "Console", "Debug", "Log")]

[Serializable]
public class InstructionDebugLog : Instruction
{
    [SerializeField] private PropertyGetString message = new PropertyGetString("Hello, GameCreator!");
    
    [SerializeField] private AnimationClip m_Animation;
    [SerializeField] private AvatarMask m_Mask;

    [SerializeField] private MeleeMotion m_Motion = MeleeMotion.None;
    [SerializeField] private Reaction m_SyncReaction;

    [SerializeField][Range(0f, 1f)] private float m_Gravity = 1f;
    [SerializeField] private float m_TransitionIn = 0.1f;
    [SerializeField] private float m_TransitionOut = 0.25f;

    [SerializeField] private RunMeleeSequence m_MeleeSequence = new RunMeleeSequence();
    protected override Task Run(Args args)
    {
        Debug.Log("");
       // m_MeleeSequence.Run("derpname",TimeMode.UpdateMode.GameTime,);
        return Task.CompletedTask;
    }
}

