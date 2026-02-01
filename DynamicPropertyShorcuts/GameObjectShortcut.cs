using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameCreator.Runtime.Common
{
    public enum ShortcutLookupMode
    {
        Prefab,
        SceneObjectByName,
        SceneObjectByTag
    }
    
    [CreateAssetMenu(
        fileName = "GameObject Shortcut", 
        menuName = "Game Creator/Shortcut"
    )]
    public class GameObjectShortcut : ScriptableObject
    {
        [SerializeField] private string m_ShortcutName = "Player";
        [SerializeField] private ShortcutLookupMode m_LookupMode = ShortcutLookupMode.SceneObjectByTag;
        [SerializeField] private GameObject m_Prefab;
        [SerializeField] private string m_SearchValue = "Player";
        
        private GameObject m_RuntimeOverride;
        private static Dictionary<string, GameObject> s_GlobalOverrides = new Dictionary<string, GameObject>();
        
        public string ShortcutName => m_ShortcutName;
        
        public GameObject Resolve()
        {
            // Check runtime override first
            if (m_RuntimeOverride != null) return m_RuntimeOverride;
            
            // Check global overrides by name
            if (s_GlobalOverrides.TryGetValue(m_ShortcutName, out var global) && global != null)
                return global;
            
            switch (m_LookupMode)
            {
                case ShortcutLookupMode.Prefab:
                    return m_Prefab;
                    
                case ShortcutLookupMode.SceneObjectByName:
                    return GameObject.Find(m_SearchValue);
                    
                case ShortcutLookupMode.SceneObjectByTag:
                    try { return GameObject.FindWithTag(m_SearchValue); }
                    catch { return null; }
                    
                default:
                    return null;
            }
        }
        
        public void Set(GameObject value)
        {
            m_RuntimeOverride = value;
        }
        
        public static void SetGlobal(string shortcutName, GameObject value)
        {
            s_GlobalOverrides[shortcutName] = value;
        }
        
        public static void ClearGlobal(string shortcutName)
        {
            s_GlobalOverrides.Remove(shortcutName);
        }
        
        public static void ClearAllGlobals()
        {
            s_GlobalOverrides.Clear();
        }
        
        public void ClearOverride()
        {
            m_RuntimeOverride = null;
        }
    }
}
