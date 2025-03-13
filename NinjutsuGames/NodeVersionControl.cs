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
public class NodeVersionControl
{
    private Dictionary<string, List<NodeState>> nodeHistory = new();
    
    public class NodeState 
    {
        public DateTime timestamp;
        public string nodeId;
        public SerializableEdge[] connections;
        public Vector2 position;
        public object nodeData;
    }
    
    public void SaveNodeState(BaseNode node)
    {
        // Save current node state
        var state = new NodeState();
        nodeHistory[node.GUID].Add(state);
    }
}
}