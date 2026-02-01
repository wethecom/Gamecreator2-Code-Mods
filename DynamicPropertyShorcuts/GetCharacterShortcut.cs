using System;
using UnityEngine;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Characters;

namespace GameCreator.Runtime.Common
{
    [Title("Character Shortcut")]
    [Category("Shortcut/Character")]
    [Description("Gets a Character's GameObject using a configured shortcut")]
    
    [Image(typeof(IconCharacter), ColorTheme.Type.Green)]
    
    [Serializable]
    public class GetCharacterShortcut : PropertyTypeGetGameObject
    {
        [SerializeField] private GameObjectShortcut m_Shortcut;
        
        public override GameObject Get(Args args)
        {
            if (m_Shortcut == null) return null;
            
            GameObject target = m_Shortcut.Resolve();
            if (target == null) return null;
            
            // Verify it has a Character component
            return target.GetComponent<Character>() != null ? target : null;
        }

        public override string String => m_Shortcut != null ? m_Shortcut.ShortcutName : "(none)";
        
        public static PropertyGetGameObject Create => new PropertyGetGameObject(
            new GetCharacterShortcut()
        );
    }
}
