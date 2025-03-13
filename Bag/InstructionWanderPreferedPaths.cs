using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Variables;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;
//wethecom@gmail.com
[Version(3, 0, 1)]
[Title("Wander Prefered Paths")]
[Description("Iterates through all waypoints of a path before selecting the next path with randomness. Executes actions if the end of all ways is reached. you will need to make a gameboject path with ways as children and a gameobject patrol as a parent of all of it, this will be on github under wethcom with details ")]

[Category("$wethcom/Prefered Paths Wandering")]

[Parameter("Ways", "The collector that holds the list of waypoints")]
[Parameter("Paths", "The collector that holds the list of paths")]
[Parameter("ChosenPath", "holds chosen Path")]
[Parameter("ChosenWay", "ChosenWay gameobject with a maker on it")]
[Keywords("Path", "Waypoint", "Navigation", "Sequence")]

[Serializable]
public class InstructionWanderPreferedPaths: Instruction
{
    [SerializeField] private CollectorListVariable m_Ways;
    [SerializeField] private CollectorListVariable m_Paths;

    [SerializeField] private PropertySetGameObject m_ChosenPath = new PropertySetGameObject();
    [SerializeField] private PropertySetGameObject m_ChosenWay = new PropertySetGameObject();

    [Header("Run instructions at the end of all ways")]
    [SerializeField] private InstructionList m_EndOfWays = new InstructionList();

    private GameObject currentWay;
    private GameObject currentPath;

    public override string Title => "Choose Nearest Next Path";

    protected override async Task Run(Args args)
    {
        currentWay = m_ChosenWay.Get(args) as GameObject;
        currentPath = m_ChosenPath.Get(args) as GameObject;

        // Get paths
        List<object> pathObjects = m_Paths.Get(args);
        List<GameObject> paths = pathObjects.Cast<GameObject>().ToList();

        // Initialization: If no chosen way or path, select the nearest path
        if (currentPath == null)
        {
            Debug.Log("Initializing path selection...");

            // Select nearest path
            paths = paths.OrderBy(path => Vector3.Distance(Vector3.zero, path.transform.position)).ToList(); // Use Vector3.zero as reference
            currentPath = paths.FirstOrDefault();

            if (currentPath != null)
            {
                m_ChosenPath.Set(currentPath, args);
                PopulateWays(currentPath, args); // Populate the ways variable
            }
        }

        // Get waypoints for the current path
        List<GameObject> waypoints = m_Ways.Get(args).Cast<GameObject>().ToList();
        if (currentWay == null)
        {
            Debug.Log("Selecting the first Waypoint...");
            currentWay = waypoints.FirstOrDefault();

            if (currentWay != null)
            {
                Debug.Log("Initial Waypoint: " + currentWay.name);
                m_ChosenWay.Set(currentWay, args);
                return; // Exit to avoid running the rest of the method
            }
        }

        // Traverse the waypoints of the current path
        int currentIndex = waypoints.IndexOf(currentWay);
        if (currentIndex >= 0 && currentIndex < waypoints.Count - 1)
        {
            // Move to the next waypoint in the current path
            currentWay = waypoints[currentIndex + 1];
            Debug.Log("Next Waypoint: " + currentWay.name);
            m_ChosenWay.Set(currentWay, args);
        }
        else
        {
            // All waypoints in the current path have been visited
            Debug.Log("End of current path. Selecting a new path...");

            paths = paths.Where(p => p != currentPath) // Exclude current path
                         .OrderBy(_ => UnityEngine.Random.value) // Add randomness to path selection
                         .OrderBy(path => Vector3.Distance(currentWay.transform.position, path.transform.position))
                         .ToList();

            GameObject nearestPath = paths.FirstOrDefault();
            if (nearestPath != null)
            {
                currentPath = nearestPath;
                m_ChosenPath.Set(currentPath, args);

                PopulateWays(currentPath, args); // Populate the ways variable for the new path
                waypoints = m_Ways.Get(args).Cast<GameObject>().ToList();

                currentWay = waypoints.FirstOrDefault();
                if (currentWay != null)
                {
                    Debug.Log("Switching to new path. First Waypoint: " + currentWay.name);
                    m_ChosenWay.Set(currentWay, args);
                }
            }
            else
            {
                Debug.LogWarning("No new paths found!");
                await HandleEndOfWays(args);
            }
        }
    }

    private async Task HandleEndOfWays(Args args)
    {
        Debug.Log("End of all ways reached. Executing end-of-ways actions.");

        // Run the end-of-ways instructions
        await m_EndOfWays.Run(args);
    }

    private int GetNumberFromName(string name)
    {
        string numberStr = new string(name.Where(char.IsDigit).ToArray());
        return int.TryParse(numberStr, out int number) ? number : -1;
    }

    private void PopulateWays(GameObject path, Args args)
    {
        Debug.Log("Populating ways for path: " + path.name);

        List<GameObject> ways = new List<GameObject>();

        foreach (Transform child in path.transform)
        {
            ways.Add(child.gameObject);
        }

        // Use Fill() to populate the m_Ways variable
        m_Ways.Fill(ways.ToArray(), args);
        Debug.Log("Filled Ways with " + ways.Count + " entries from path: " + path.name);
    }

}
