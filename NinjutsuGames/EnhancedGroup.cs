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
public class EnhancedGroup : Group 
{
    // Hierarchical grouping
    public List<EnhancedGroup> subGroups = new();
    
    // Smart categorization
    public string category;
    public Color categoryColor;
    
    // Collapsible state
    public bool isCollapsed;
    
    // Auto-organization
    public void AutoOrganizeNodes()
    {
        // Auto-arrange nodes within group
        // Calculate optimal positions
        // Handle subgroup layouts
    }
}
}