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
public class NodeSearchSystem
{
    public List<BaseNode> SearchNodes(string searchTerm)
    {
        // Implement fuzzy search
        return new List<BaseNode>();
    }
    
    public void NavigateToConnectedNode(BaseNode currentNode, bool forward)
    {
        // Quick navigation between nodes
    }
}
}