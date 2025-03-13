using System;
using System.Collections.Generic;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Variables;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;
using Event = GameCreator.Runtime.VisualScripting.Event;

[Version(1, 0, 1)]
[Title("On Input Key Combo State")]
[Category("Input/On Input Combo")]
[Description("Triggered when input and keycombos changes and stores the new values in named variables")]
//updated becuase of name collison
[Image(typeof(IconButton), ColorTheme.Type.TextLight)]

[Keywords("String", "Change", "Variable", "Update", "Input")]

[Serializable]
public class EventOnInputCombo : Event
{
    [SerializeField] public PropertySetString CurrentComboStore;
    [SerializeField] public PropertySetString KeyStateStore;
    [SerializeField] private string StateDebug;

    [SerializeField] private string ComboDebug;

    private HashSet<KeyCode> currentlyPressedKeys = new HashSet<KeyCode>();
    private string lastStringValue1 = string.Empty;
    private string lastStringValue2 = string.Empty;
    bool updated = false;

    protected override void OnUpdate(Trigger trigger)
    {
        base.OnUpdate(trigger);

        // Check each key in the KeyCode enumeration
        foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(key))
            {
                currentlyPressedKeys.Add(key);
                LogKeyState(key, "Pressed", trigger);
                LogCurrentCombination(trigger);
            }
            else if (Input.GetKeyUp(key))
            {
                currentlyPressedKeys.Remove(key);
                LogKeyState(key, "Released", trigger);
            }
        }

        // Detect mouse scroll wheel movement
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            string scrollDirection = scroll > 0 ? "Scroll Up" : "Scroll Down";
            LogKeyState(KeyCode.None, scrollDirection, trigger); // Use KeyCode.None as placeholder
            LogCurrentCombination(trigger);
        }

        if (updated)
        {
            _ = this.m_Trigger.Execute(this.Self);
            updated = false;
        }
    }

    void LogKeyState(KeyCode key, string state, Trigger trigger)
    {
        StateDebug = "Key " + key + " " + state;
        KeyStateStore.Set("Key " + key + " " + state, trigger);
        updated = true;
        Debug.Log("Key " + key + " " + state);
    }

    void LogCurrentCombination(Trigger trigger)
    {
        if (currentlyPressedKeys.Count > 0 || StateDebug.Contains("Scroll"))
        {
            ComboDebug = "Current combination: " + string.Join(" + ", currentlyPressedKeys) + (StateDebug.Contains("Scroll") ? " + " + StateDebug : "");

            CurrentComboStore.Set(ComboDebug, trigger);
            updated = true;
            Debug.Log(ComboDebug);
        }
    }
}
