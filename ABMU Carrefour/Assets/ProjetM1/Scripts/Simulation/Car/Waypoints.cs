using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Waypoints : MonoBehaviour
{
    public List<GameObject> allWaypoints = new List<GameObject>(), centerWaypoints = new List<GameObject>();
    private void OnDrawGizmos()
    {
        foreach (Transform t in transform)
        {
            Gizmos.color = Color.blue;
           // Gizmos.DrawWireSphere(t.position, 1);
        }
        Gizmos.color = Color.red;
        for (int i = 0; i < transform.childCount - 1; i++)
        {
           // Gizmos.DrawLine(transform.GetChild(i).position, transform.GetChild(i + 1).position);
        }
       // Gizmos.DrawLine(transform.GetChild(transform.childCount - 1).position, allWaypoints[0].transform.position);
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
    public List<GameObject> getCenterWaypoints()
    {
        return centerWaypoints;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="currentWaypoint"></param>
    /// <returns>le prochain waypoint en gameobject, et true si il tourne à droite</returns>
    public KeyValuePair<GameObject, bool> GetNextWaypoint(GameObject currentWaypoint, GameObject previousWaypoint) {
        if (currentWaypoint == null) return new KeyValuePair<GameObject, bool>(allWaypoints[0], false);

        int waypointSize = allWaypoints.Count;
        int currentIndex = allWaypoints.IndexOf(currentWaypoint);
        WaypointInformation currentInfo = currentWaypoint.GetComponent<WaypointInformation>();
        WaypointInformation previousInfo = previousWaypoint == null ? null : previousWaypoint.GetComponent<WaypointInformation>();
        if (currentInfo != null && currentInfo.isCenter) //si il est au carrefour
        {
            if (previousInfo != null && previousInfo.isCenter)//si il était déjà au center avant
            {
                //Debug.Log(car.name + " " + previousWaypoint.name);
                return new KeyValuePair<GameObject, bool>(allWaypoints[currentIndex >= waypointSize - 1 ? 0 : currentIndex + 1], false); //on va au prochain
            }
            int r = Random.Range(0, 3);
            if(r < 1)
            {
                return new KeyValuePair<GameObject, bool>(randomWaypointExcept(centerWaypoints, currentWaypoint), false); //on va aléatoirement à un autre waypoint du centre
            }
            else
            {
                return new KeyValuePair<GameObject, bool>(allWaypoints[currentIndex >= waypointSize - 1 ? 0 : currentIndex + 1], true); //on va à droite
            }
 
        }
        return new KeyValuePair<GameObject, bool>(allWaypoints[currentIndex >= waypointSize - 1 ? 0 : currentIndex + 1], false);  //on va au prochain
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

}
