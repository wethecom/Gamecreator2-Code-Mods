using System;
using GameCreator.Runtime.Common;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

namespace GameCreator.Runtime.VisualScripting
{
    [Version(1, 0, 1)]
    [Title("On Mouse Wheel Scrolling")]
    [Category("Input/On Mouse Wheel Scrolling")]
    [Description("Detects when mouse wheel is scrolling, has stopped, or scrolled in specific directions")]

    [Image(typeof(IconScroll), ColorTheme.Type.Yellow)]

    [Keywords("Keyboard", "Mouse", "Wheel", "Gamepad", "Controller", "Joystick")]

    [Serializable]
    public class EventOnMouseScroll : Event
    {
        // MEMBERS: -------------------------------------------------------------------------------
        float mouseWheel;
        Vector2 m_Vec;
        
        private enum MouseScrollAction 
        {
            m_ScrollingStart,
            m_ScrollingStop,
            m_ScrollUp, 
            m_ScrollDown    
        };
        
        [SerializeField] private MouseScrollAction mouseAction;

        // NON-MEMBERS: -------------------------------------------------------------------------------
        private bool stop = true;

        // RUN METHOD: ----------------------------------------------------------------------------
        protected override void OnStart(Trigger trigger)
        {
            base.OnStart(trigger);
        }

        protected override void OnDestroy(Trigger trigger)
        {
            base.OnDestroy(this.m_Trigger);
        }

        protected override void OnUpdate(Trigger trigger)
        {
            m_Vec = Mouse.current.scroll.ReadValue();
            mouseWheel = m_Vec.y;

            switch (mouseAction)
            {
                case MouseScrollAction.m_ScrollUp:
                    if (mouseWheel > 0f)
                    {
                        _ = this.m_Trigger.Execute(this.Self);
                    }
                    break;
                case MouseScrollAction.m_ScrollDown:
                    if (mouseWheel < 0f)
                    {
                        _ = this.m_Trigger.Execute(this.Self);
                    }
                    break;
                case MouseScrollAction.m_ScrollingStart:
                    if (mouseWheel != 0f)
                    {
                        _ = this.m_Trigger.Execute(this.Self);
                    }
                    break;
                case MouseScrollAction.m_ScrollingStop:
                    if (stop == true)
                        if (mouseWheel == 0f)
                        {
                            if (stop == true)
                            {
                                _ = this.m_Trigger.Execute(this.Self);
                                stop = false;
                            }
                        }

                    if (mouseWheel != 0f)
                    {
                        stop = true;
                    }
                    break;
            }
        }
    }
}