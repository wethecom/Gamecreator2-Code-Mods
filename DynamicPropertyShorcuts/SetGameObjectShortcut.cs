using System;
using UnityEngine;
using GameCreator.Runtime.Common;

namespace GameCreator.Runtime.Common
{
    [Title("Shortcut")]
    [Category("Shortcut")]
    [Description("Sets a shortcut's runtime reference to a GameObject")]
    
    [Image(typeof(IconCubeSolid), ColorTheme.Type.Blue)]
    
    [Serializable]
    public class SetGameObjectShortcut : PropertyTypeSetGameObject
    {
        [SerializeField] private GameObjectShortcut m_Shortcut;
        
        public override void Set(GameObject value, Args args)
        {
            if (m_Shortcut != null)
            {
                m_Shortcut.Set(value);
            }
        }
        
        public override GameObject Get(Args args)
        {
            return m_Shortcut != null ? m_Shortcut.Resolve() : null;
        }

        public override string String => m_Shortcut != null ? m_Shortcut.ShortcutName : "(none)";
        
        public static PropertySetGameObject Create => new PropertySetGameObject(
            new SetGameObjectShortcut()
        );
    }
}
