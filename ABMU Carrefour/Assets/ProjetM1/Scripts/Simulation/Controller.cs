using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ABMU.Core;
using ABMU;

using UnityEngine.AI;

using UnityEditor;

public class Controller : AbstractController
{
    public GameObject agentPrefab, carPrefab;
    public int numAgents = 100;

    [Header("Agent Parameters")]
    public float distToTargetThreshold = 2f;


    private List<GameObject> roads, sidewalks;
    
    void Start(){
        roads = GetAllRoads();
        sidewalks = GetAllSidewalks();
        createAgents();
        createCars();
    }
    void Update(){
        
    }

    void createAgents(){

        for (int i = 0; i < numAgents; i++)
        {

            GameObject agent = Instantiate(agentPrefab);
            var cubeRenderer = agent.GetComponent<Renderer>();
            cubeRenderer.material.SetColor("_Color", Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f));
            NavMeshAgent nmAgent = agent.GetComponent<NavMeshAgent>();
            GameObject random = GetRandomObject(sidewalks);
            Vector3 point = GetRandomPointInObject(random, agentPrefab);
            nmAgent.Warp(point);
            agent.transform.position = nmAgent.nextPosition;
            agent.GetComponent<AgentNavigation>().Init();
        }
    }

    void createCars(){

        for (int i = 0; i < numAgents; i++)
        {

            GameObject car = Instantiate(carPrefab);
            var cubeRenderer = car.GetComponent<Renderer>();
            NavMeshAgent nmAgent = car.GetComponent<NavMeshAgent>();
            GameObject random = GetRandomObject(roads);
            Vector3 point = GetRandomPointInObject(random, carPrefab);
            nmAgent.Warp(point);
            car.transform.position = nmAgent.nextPosition;
            car.GetComponent<AgentNavigation>().Init();
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
