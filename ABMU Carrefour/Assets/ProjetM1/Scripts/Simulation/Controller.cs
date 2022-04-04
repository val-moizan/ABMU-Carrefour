using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ABMU.Core;
using ABMU;

using UnityEngine.AI;

using UnityEditor;
using System.Linq;

public class Controller : AbstractController
{
    public GameObject agentPrefab, carPrefab;
    public int numAgents = 10, numCars = 2;

    [Header("Agent Parameters")]
    public float distToTargetThreshold = 2f;
    [Range(1, 100)]
    public float thugProbability = 10;
    [Range(1, 100)]
    public float viewDistance = 20;

    private List<GameObject> roads, sidewalks;

    public bool pov;
    void Start(){
        roads = GetAllRoads();
        sidewalks = GetAllSidewalks();
        createAgents();
        createCars();

    }
    void createAgents(){
        for (int i = 0; i < numAgents; i++) {
            GameObject agent = Instantiate(agentPrefab);
            agent.name = "Agent " + i;
            if (pov)
            {
                addCamToAgent(agent);
            }
            NavMeshAgent nmAgent = agent.GetComponent<NavMeshAgent>();
            GameObject random = GetRandomObject(sidewalks);
            Vector3 point = GetCenterOfObject(random);
            point.y -= 1;
            nmAgent.Warp(point);
            agent.transform.position = nmAgent.nextPosition;
            agent.GetComponent<AgentNavigation>().Init();
        }
    }
    void addCamToAgent(GameObject agent)
    {
        List<GameObject> childrenList = new List<GameObject>();
        Transform[] children = agent.GetComponentsInChildren<Transform>(true);
        for (int z = 0; z < children.Length; z++)
        {
            Transform child = children[z];
            if (child != transform)
            {
                childrenList.Add(child.gameObject);
            }
        }
        for (int z = 0; z < childrenList.Count; z++)
        {
            if (childrenList[z].name.Equals("Camera"))
            {
                Camera cam = childrenList[z].AddComponent<Camera>();
            }
        }
    }
    void createCars(){
       
        List<Waypoints> allList = new List<Waypoints>(FindObjectsOfType<Waypoints>()); //liste de toutes les liste de waypoint
        List<GameObject> array = new List<GameObject>();
        foreach (Waypoints waypoints in allList)
        {
            List<GameObject> list = new List<GameObject>(waypoints.getAllWaypoints());
            list = list.Where(x => !waypoints.getCenterWaypoints().Contains(x)).ToList(); //on retire les waypoint centraux et ceux pour sortir
            array.AddRange(list);
        }
        

        for (int i = 0; i < numCars; i++) {

            GameObject car = Instantiate(carPrefab);
            car.name = "Car " + i;
            CarNavigation nav = car.GetComponent<CarNavigation>();

            int lenght = array.Count;
            int index = Random.Range(0, lenght);
            nav.Init();
            nav.setStartWaypoint(array[index]);
            array.RemoveAt(index);
            if (lenght <= 1)
            {
                break;
            }
        }
    }

    public GameObject GetRandomObject(List<GameObject> list){

        GameObject ob = list[Random.Range(0,list.Count)];
        return ob;
    }
    public Vector3 GetRandomPointInObject(GameObject obj, GameObject agent){
        Bounds rb = obj.GetComponent<Collider>().bounds;
        Vector3 cr = Utilities.RandomPointInBounds(rb);
        cr.y = rb.center.y;
        cr.y -= rb.extents.y;
        cr.y += agent.GetComponent<NavMeshAgent>().baseOffset;
        return cr;
    }

    public Vector3 GetCenterOfObject(GameObject obj){
        Bounds rb = obj.GetComponent<Collider>().bounds;
        Vector3 cr = rb.center;
        cr.y -= rb.extents.y;
        cr.y += agentPrefab.transform.localScale.y/2f;
        return cr;
    }

    public List<GameObject> GetAllRoads(){
        return new List<GameObject>(GameObject.FindGameObjectsWithTag("drivable"));
    }
    public List<GameObject> GetAllSidewalks(){
        return new List<GameObject>(GameObject.FindGameObjectsWithTag("walkable"));
    }
}
