using System;
using UnityEngine;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Characters;

namespace GameCreator.Runtime.Common
{
    [Title("Character Shortcut")]
    [Category("Shortcut/Character")]
    [Description("Sets a shortcut's runtime reference to a Character's GameObject")]
    
    [Image(typeof(IconCharacter), ColorTheme.Type.Blue)]
    
    [Serializable]
    public class SetCharacterShortcut : PropertyTypeSetGameObject
    {
        [SerializeField] private GameObjectShortcut m_Shortcut;
        
        public override void Set(GameObject value, Args args)
        {
            if (m_Shortcut == null) return;
            
            // Only set if it has a Character component
            if (value != null && value.GetComponent<Character>() != null)
            {
                m_Shortcut.Set(value);
            }
        }
        
        public override GameObject Get(Args args)
        {
            if (m_Shortcut == null) return null;
            
            GameObject target = m_Shortcut.Resolve();
            if (target == null) return null;
            
            return target.GetComponent<Character>() != null ? target : null;
        }

        public override string String => m_Shortcut != null ? m_Shortcut.ShortcutName : "(none)";
        
        public static PropertySetGameObject Create => new PropertySetGameObject(
            new SetCharacterShortcut()
        );
    }
}
