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
public class NodeTemplateSystem
{
    private Dictionary<string, BaseNode> templates = new();
    
    public void SaveAsTemplate(BaseNode node, string templateName)
    {
        // Save node configuration
        templates[templateName] = node;
    }
    
    public BaseNode CreateFromTemplate(string templateName)
    {
        // Create from template
        return templates[templateName];
    }
}
}