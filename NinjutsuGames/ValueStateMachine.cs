using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Variables;
using NinjutsuGames.StateMachine.Runtime.Common;
using UnityEngine;

namespace NinjutsuGames.StateMachine.Runtime
{
    [Image(typeof(IconStateMachine), ColorTheme.Type.Blue)]
    [Title("State Machine")]
    [Category("State Machine/State Machine")]
    
    [Serializable]
    public class ValueStateMachine : TValue
    {
        public static readonly IdString TYPE_ID = new("state-machine");
        
        // EXPOSED MEMBERS: ----
        [SerializeField] private StateMachineAsset m_Value;
        
        // PROPERTIES: ----
        public override IdString TypeID => TYPE_ID;
        public override Type Type => typeof(StateMachineAsset);
        
        public override bool CanSave => false;

        public override TValue Copy => new ValueStateMachine
        {
            m_Value = this.m_Value
        };
        
        // CONSTRUCTORS: ----
        public ValueStateMachine() : base()
        { }

        public ValueStateMachine(StateMachineAsset value) : this()
        {
            this.m_Value = value;
        }

        // OVERRIDE METHODS: ----
        protected override object Get()
        {
            return this.m_Value;
        }

        protected override void Set(object value)
        {
            this.m_Value = value is StateMachineAsset cast ? cast : null;
        }
        
        public override string ToString()
        {
            return this.m_Value != null ? this.m_Value.name : "(none)";
        }
        
        // REGISTRATION METHODS: ----
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RuntimeInit() => RegisterValueType(
            TYPE_ID, 
            new TypeData(typeof(ValueStateMachine), CreateValue),
            typeof(StateMachineAsset)
        );
        
        #if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        private static void EditorInit() => RegisterValueType(
            TYPE_ID, 
            new TypeData(typeof(ValueStateMachine), CreateValue),
            typeof(StateMachineAsset)
        );
        #endif

        private static ValueStateMachine CreateValue(object value)
        {
            return new ValueStateMachine(value as StateMachineAsset);
        }
    }
}