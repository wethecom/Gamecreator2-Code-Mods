using System;
using UnityEngine;
using GameCreator.Runtime.Common;

namespace GameCreator.Runtime.Common.Network
{
    [Title("Shortcut")]
    [Category("Shortcut")]
    [Description("Gets a GameObject using a configured shortcut")]
    
    [Image(typeof(IconCubeSolid), ColorTheme.Type.Green)]
    
    [Serializable]
    public class GetGameObjectShortcut : PropertyTypeGetGameObject
    {
        [SerializeField] private GameObjectShortcut m_Shortcut;
        
        public override GameObject Get(Args args)
        {
            return m_Shortcut != null ? m_Shortcut.Resolve() : null;
        }

        public override string String => m_Shortcut != null ? m_Shortcut.ShortcutName : "(none)";
        
        public static PropertyGetGameObject Create => new PropertyGetGameObject(
            new GetGameObjectShortcut()
        );
    }
}
