using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Waypoints : MonoBehaviour
{
    private GameObject previousWaypoint = null;
    private List<GameObject> allWaypoints = new List<GameObject>(), centerWaypoints = new List<GameObject>();
    private void OnDrawGizmos()
    {
        foreach (Transform t in transform)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(t.position, 1);
        }
        Gizmos.color = Color.red;
        for (int i = 0; i < transform.childCount - 1; i++)
        {
            Gizmos.DrawLine(transform.GetChild(i).position, transform.GetChild(i + 1).position);
        }
        Gizmos.DrawLine(transform.GetChild(transform.childCount - 1).position, allWaypoints[0].transform.position);
    }
    public void addWaypoint(GameObject ob)
    {
        allWaypoints.Add(ob);
    }

    public void addCenterWaypoint(GameObject ob)
    {
        centerWaypoints.Add(ob);
    }
    public List<GameObject> getAllWaypoints()
    {
        return allWaypoints;
    }

    public GameObject GetNextWaypoint(GameObject currentWaypoint) {
        if (currentWaypoint == null) return allWaypoints[0];

        int waypointSize = allWaypoints.Count;
        int currentIndex = allWaypoints.IndexOf(currentWaypoint);
        WaypointInformation currentInfo = currentWaypoint.GetComponent<WaypointInformation>();
        WaypointInformation previousInfo = previousWaypoint == null ? null : previousWaypoint.GetComponent<WaypointInformation>();
        if (currentInfo != null && currentInfo.isCenter) //si il est au carrefour
        {
            if (previousInfo != null && previousInfo.isCenter)//si il était déjà au center avant
            {
                previousWaypoint = currentWaypoint;
                return allWaypoints[currentIndex >= waypointSize - 1 ? 0 : currentIndex + 1]; //on va au prochain
            }
            previousWaypoint = currentWaypoint;
            int r = Random.Range(0, 3);
            if(r < 2)
            {
                return randomWaypointExcept(centerWaypoints, currentWaypoint); //on va aléatoirement à un autre waypoint du centre
            }
            else
            {
                return allWaypoints[currentIndex >= waypointSize - 1 ? 0 : currentIndex + 1]; //on va à droite
            }
 
        }
        previousWaypoint = currentWaypoint;
        return allWaypoints[currentIndex >= waypointSize - 1 ? 0 : currentIndex + 1];  //on va au prochain
    }
    public GameObject getRandomWaypoint()
    {
        return allWaypoints[Random.Range(0, allWaypoints.Count)];
    }
    private GameObject randomWaypointExcept(List<GameObject> waypoints, GameObject ob)
    {
        GameObject ret = null;
        while(ret == null || ret == ob || (ret.transform.position.x != ob.transform.position.x & ret.transform.position.z != ob.transform.position.z)) //on enleve le tournant à gauche
        {
            ret = waypoints[Random.Range(0, waypoints.Count)];
        }
        return ret;
    }
    /*
    public Transform GetNextWaypoint(Transform currentWaypoint)
    {
        int[] numbers = { 1, 3, 6 };
        int randomIndex = Random.Range(0, 3);
        //Transform wpDesti;

        if (currentWaypoint == null) return allWaypoints[0].transform;

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
            return allWaypoints[0].transform;
        }
    }*/
}
