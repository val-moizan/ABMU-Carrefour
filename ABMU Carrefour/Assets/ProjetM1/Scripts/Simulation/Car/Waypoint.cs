using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Waypoint : MonoBehaviour
{
    //public Waypoint prevWaypoint;
    //public Waypoint nextWaypoint;
    private Transform previousWaypoint = null;
    /*
    [Range(0f, 5f)]
    public float width = 1f;
    */
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

    public Transform GetNextWaypoint(Transform currentWaypoint) {
        int[] numbers = { 1, 3, 6 };
        int randomIndex = Random.Range(0, 3);

        if (previousWaypoint == null) previousWaypoint = transform.GetChild(0);
        if (currentWaypoint == null) return transform.GetChild(0);

        Debug.Log("prevWaypoint = " + previousWaypoint.GetSiblingIndex());
        //Debug.Log("currentWaypoint = " + currentWaypoint.GetSiblingIndex());

        if (currentWaypoint.GetSiblingIndex() < transform.childCount - 1){ // si le prochain waypoint n'est pas le dernier
            int randNum = numbers[randomIndex];
            // si arriver au carrefour
            if (currentWaypoint.GetSiblingIndex() == 1 || currentWaypoint.GetSiblingIndex() == 4 || currentWaypoint.GetSiblingIndex() == 7 || currentWaypoint.GetSiblingIndex() == 10) {
                // si pr�cedemment d�j� au carrefour, sort du carrefour
                if (previousWaypoint.GetSiblingIndex() == 1 || previousWaypoint.GetSiblingIndex() == 4 || previousWaypoint.GetSiblingIndex() == 7 || previousWaypoint.GetSiblingIndex() == 10)
                {
                    previousWaypoint = currentWaypoint;
                    return transform.GetChild(currentWaypoint.GetSiblingIndex() + 1);
                }
                previousWaypoint = currentWaypoint;
                //Debug.Log("Carrefour");

                if (currentWaypoint.GetSiblingIndex() + randNum - 1 > transform.childCount - 1) // si d�passe le total
                    return transform.GetChild(currentWaypoint.GetSiblingIndex() + randNum - transform.childCount);
                // au carrefour mais ne d�passe pas le total, return random waypoint du carrefour
                else return transform.GetChild(currentWaypoint.GetSiblingIndex() + randNum);
            }
            else
            {
                previousWaypoint = currentWaypoint;
                return transform.GetChild(currentWaypoint.GetSiblingIndex() + 1);
            }
        }
        else return transform.GetChild(0);
    }
    /*
    public Transform GetNextWaypoint(Transform currentWaypoint)
    {
        int[] numbers = { 1, 3, 6 };
        int randomIndex = Random.Range(0, 3);
        //Transform wpDesti;

        if (currentWaypoint == null) return transform.GetChild(0);

        if (currentWaypoint.GetSiblingIndex() < transform.childCount - 1) // si le prochain waypoint n'est pas le dernier
        {
            int randNum = numbers[randomIndex];
            // si arriver au carrefour
            if (currentWaypoint.GetSiblingIndex() == 1 || currentWaypoint.GetSiblingIndex() == 4 || currentWaypoint.GetSiblingIndex() == 7 || currentWaypoint.GetSiblingIndex() == 10)
            {
                // si d�passe le total
                if (currentWaypoint.GetSiblingIndex() + randNum - 1 > transform.childCount - 1)
                {
                    Debug.Log("d�passe");
                    Debug.Log(currentWaypoint.GetSiblingIndex() + randNum - transform.childCount);
                    return transform.GetChild(currentWaypoint.GetSiblingIndex() + randNum - transform.childCount);
                }
                // au carrefour mais ne d�passe pas le total, return random waypoint du carrefour
                else
                {
                    Debug.Log("d�passe pas");
                    Debug.Log(currentWaypoint.GetSiblingIndex() + randNum);
                    return transform.GetChild(currentWaypoint.GetSiblingIndex() + randNum);
                }
            }
            else
            {
                Debug.Log("next");
                Debug.Log(currentWaypoint.GetSiblingIndex() + 1);
                return transform.GetChild(currentWaypoint.GetSiblingIndex() + 1);
            }
        }
        else
        {
            Debug.Log("retour d�but");
            return transform.GetChild(0);
        }
    }*/
}
