using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Waypoint : MonoBehaviour
{
    public Waypoint prevWaypoint;
    public Waypoint nextWaypoint;

    [Range(0f, 5f)]
    public float width = 1f;

    [Range(0f, 2f)]
    [SerializeField] private float waypointSize = 1f;
    private void OnDrawGizmos()
    {
        foreach (Transform t in transform)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(t.position, waypointSize);
        }
        Gizmos.color = Color.red;
        for (int i = 0; i < transform.childCount - 1; i++)
        {
            Gizmos.DrawLine(transform.GetChild(i).position, transform.GetChild(i + 1).position);
        }
        Gizmos.DrawLine(transform.GetChild(transform.childCount - 1).position, transform.GetChild(0).position);
    }

    public Transform GetNextWaypoint(Transform currentWaypoint)
    {
        int[] numbers = { 1, 3, 6 };
        int randomIndex = Random.Range(0, 3);
        Transform wpDesti;

        if (currentWaypoint == null)
        {
            wpDesti = transform.GetChild(0);
            return wpDesti;
        }

        if (currentWaypoint.GetSiblingIndex() < transform.childCount - 1)
        {
            if (currentWaypoint.GetSiblingIndex() + numbers[randomIndex] < transform.childCount && (currentWaypoint.GetSiblingIndex() == 1 || currentWaypoint.GetSiblingIndex() == 4 || currentWaypoint.GetSiblingIndex() == 7 || currentWaypoint.GetSiblingIndex() == 10))
            {
                wpDesti = transform.GetChild(currentWaypoint.GetSiblingIndex() + numbers[randomIndex]);
            }
            else wpDesti = transform.GetChild(currentWaypoint.GetSiblingIndex() + 1);
        }
        else wpDesti = transform.GetChild(0);

        if (wpDesti.GetSiblingIndex() > transform.childCount)
            return wpDesti.GetChild(currentWaypoint.GetSiblingIndex() - transform.childCount);

        return wpDesti;
    }

    /*
    public Transform GetNextWaypoint(Transform currentWaypoint)
    {
        int[] numbers = { 1, 3, 6 };
        int randomIndex = Random.Range(0, 3);
        Transform wpDesti;

        if (currentWaypoint == null) return transform.GetChild(0);

        if (currentWaypoint.GetSiblingIndex() < transform.childCount - 1)
        {
            if (currentWaypoint.GetSiblingIndex() == 1 || currentWaypoint.GetSiblingIndex() == 4 || currentWaypoint.GetSiblingIndex() == 7 || currentWaypoint.GetSiblingIndex() == 10)
            {
                return transform.GetChild(currentWaypoint.GetSiblingIndex() + numbers[randomIndex] );
            }
            else return transform.GetChild(currentWaypoint.GetSiblingIndex() + 1);
        }
        else return transform.GetChild(0);
    }*/
}