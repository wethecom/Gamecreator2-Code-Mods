using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using NinjutsuGames.StateMachine.Runtime;
using NinjutsuGames.StateMachine.Runtime.Common;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

namespace NinjutsuGames.StateMachine.Runtime 
{
public class NodeValidator 
{
    public bool ValidateNodeConnections(BaseNode node)
    {
        // Check circular dependencies
        // Validate port compatibility
        // Check execution order
        return true;
    }
    
    public bool ValidateNodePosition(BaseNode node)
    {
        // Check overlapping
        // Validate boundaries
        return true;
    }
}
}