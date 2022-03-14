using ABMU.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class carWaypointNav : AbstractAgent
{
    private Waypoint waypoints;
    NavMeshAgent nmAgent;
    private Transform currentWaypoint;

    public override void Init() {
        base.Init();
        waypoints = GameObject.FindGameObjectWithTag("CarWaypoints").GetComponent<Waypoint>();
        nmAgent = GetComponent<NavMeshAgent>();
        currentWaypoint = waypoints.GetNextWaypoint(currentWaypoint);
        transform.position = currentWaypoint.position;
        nmAgent.SetDestination(waypoints.GetNextWaypoint(null).position);
    }
    void Update()
    {
        /*transform.position = Vector3.MoveTowards(transform.position, currentWaypoint.position, moveSpeed * Time.deltaTime);
        if (Vector3.Distance(transform.position, currentWaypoint.position) < distanceThreshold)
        {
            currentWaypoint = waypoints.GetNextWaypoint(currentWaypoint);
        }*/

        
    }
}