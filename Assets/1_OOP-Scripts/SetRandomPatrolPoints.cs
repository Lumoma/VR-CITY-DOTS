// File: `Assets/OOP-Scripts/SetRandomPatrolPoints.cs`
using System.Collections.Generic;
//using Unity.Behavior;
using UnityEngine;

public class SetRandomPatrolPoints : MonoBehaviour
{
    [Tooltip("Breite des Spawn-Bereichs (X)")]
    public float spawnLength = 10f;
    [Tooltip("Tiefe des Spawn-Bereichs (Z)")]
    public float spawnWidth = 10f;

    [Tooltip("Minimale Anzahl Waypoints")]
    [SerializeField]private int minWaypoints = 3;
    [Tooltip("Maximale Anzahl Waypoints")]
    [SerializeField]private int maxWaypoints = 10;

    [Tooltip("Name-Präfix für generierte Waypoints")]
    [SerializeField]private string waypointNamePrefix = "Waypoint_";

    private void Start()
    {
        //Find Object with Tag "SpawnArea"
        var spawnArea = GameObject.FindGameObjectWithTag("SpawnArea");
        var area = spawnArea.GetComponent<FenceGizmoScript>();
        if (area != null)
        {
            spawnLength = area.fenceLength;
            spawnWidth = area.fenceWidth;
        }

        int count = Random.Range(minWaypoints, maxWaypoints + 1);
        var createdTransforms = new List<GameObject>(count);

        for (int i = 0; i < count; i++)
        {
            var go = new GameObject($"{waypointNamePrefix}{i}");

            // Weltkoordinate berechnen
            Vector3 worldPos;
            if (i == 0)
            {
                // Erster Waypoint exakt an der Welt-Position des Parents
                worldPos = transform.position;
            }
            else
            {
                float randomX = Random.Range(-spawnLength / 2f, spawnLength / 2f);
                float randomZ = Random.Range(-spawnWidth / 2f, spawnWidth / 2f);
                worldPos = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);
            }

            // Position in Weltkoordinaten setzen und dann Parent anhängen, dabei Weltposition beibehalten
            go.transform.position = worldPos;
            go.transform.SetParent(transform, true);
            
            createdTransforms.Add(go);
        }

        bool assigned = TryAssignToPatrolPoints(createdTransforms);
        if (!assigned)
        {
            Debug.Log($"SetRandomPatrolPoints: {createdTransforms.Count} Waypoints erzeugt, konnte jedoch nicht zugewiesen werden.");
        }
        else
        {
            Debug.Log($"SetRandomPatrolPoints: {createdTransforms.Count} Waypoints erzeugt und in PatrolPoints geladen.");
        }
    }

    private bool TryAssignToPatrolPoints(List<GameObject> createdWaypoints)
    {
        /*
        var agentComponent = GetComponentInChildren<BehaviorGraphAgent>();
        if (agentComponent == null) return false;
        agentComponent.Set
        */
        return false;
    }
}