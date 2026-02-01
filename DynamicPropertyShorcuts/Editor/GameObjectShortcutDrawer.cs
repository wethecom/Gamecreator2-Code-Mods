#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace GameCreator.Runtime.Common.Network
{
    [CustomPropertyDrawer(typeof(GameObjectShortcut))]
    public class GameObjectShortcutDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            
            var shortcuts = FindAllShortcuts();
            var currentShortcut = property.objectReferenceValue as GameObjectShortcut;
            
            int currentIndex = 0;
            var options = new List<string> { "(None)" };
            
            for (int i = 0; i < shortcuts.Count; i++)
            {
                options.Add(shortcuts[i].ShortcutName);
                if (currentShortcut == shortcuts[i])
                    currentIndex = i + 1;
            }
            
            int newIndex = EditorGUI.Popup(position, label.text, currentIndex, options.ToArray());
            
            if (newIndex != currentIndex)
            {
                property.objectReferenceValue = newIndex == 0 ? null : shortcuts[newIndex - 1];
            }
            
            EditorGUI.EndProperty();
        }
        
        private List<GameObjectShortcut> FindAllShortcuts()
        {
            return AssetDatabase.FindAssets("t:GameObjectShortcut")
                .Select(guid => AssetDatabase.LoadAssetAtPath<GameObjectShortcut>(
                    AssetDatabase.GUIDToAssetPath(guid)))
                .Where(s => s != null)
                .OrderBy(s => s.ShortcutName)
                .ToList();
        }
    }
}
#endif
