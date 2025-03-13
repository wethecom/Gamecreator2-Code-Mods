using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Variables;
using GameCreator.Runtime.VisualScripting;
using System.Threading.Tasks;

[Version(1, 0, 5)]
[Title("Place Nearest Marker in Variable random Waypoints pathfinding")]
[Description("Waypoint system that finds one of the top pooled nearest markers with a specific tag and places it into a named LocalNameVariables")]

[Category("Custom/Waypoint Place Nearest Random Marker in Variable")]
[Keywords("Random", "Navigation", "Waypoint", "Markers", "Pathfinding")]
[Serializable]
[Parameter(
        "waypointTag",
        "Tag to find usualy called waypoint"
    )]
[Parameter(
        "layerMask",
        "to narrow the search"
    )]
[Parameter(
        "localNameVariables",
        "Where to store the variable for later use in move to or follow"
    )]
   
[Parameter(
        "variableName",
        "The name of the local named variable to store the marker in"
    )]
[Parameter(
        "RandomPool",
        "The amount of markers nearest to you to pool for random choice so you dont go across the world"
    )]
public class InstructionPlaceNearestMarkerInVariable : Instruction
{
    [SerializeField] private string waypointTag = "WayPoint";
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private LocalNameVariables localNameVariables;
    [SerializeField] private string variableName;
    [SerializeField] private int RandomPool=6;
    public override string Title => $"Select one of the three nearest '{waypointTag}' objects within specified layers";

    protected override Task Run(Args args)
    {
        SelectNearestWaypoint(args);
        return DefaultResult;
    }

    private void SelectNearestWaypoint(Args args)
    {
        var allWaypoints = GameObject.FindGameObjectsWithTag(waypointTag)
            .Where(waypoint => ((1 << waypoint.layer) & layerMask) != 0)
            .Select(waypoint => new
            {
                waypoint,
                distance = Vector3.Distance(args.Self.transform.position, waypoint.transform.position)
            })
            .OrderBy(x => x.distance)
            .Take(RandomPool)
            .ToList();

        if (allWaypoints.Count > 0)
        {
            var selectedWaypoint = allWaypoints[UnityEngine.Random.Range(0, allWaypoints.Count)].waypoint;

            localNameVariables.Set(variableName, selectedWaypoint);
            Debug.Log($"Selected Waypoint: {selectedWaypoint.name} placed in variable '{variableName}'.");
        }
        else
        {
            Debug.Log($"No waypoints with tag '{waypointTag}' found within specified layers.");
        }
    }
}