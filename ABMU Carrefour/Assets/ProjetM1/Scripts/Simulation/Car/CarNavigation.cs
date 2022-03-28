using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ABMU.Core;
using UnityEngine.AI;
public class CarNavigation : AbstractAgent {
    Controller nCont;
    NavMeshAgent nmAgent;
    Vector3 target;
    //public GameObject targetObject;
    GameObject cube, cube2;
    Waypoints waypoints;
    public GameObject currentWaypoint, nextWaypoint;
    public Material transparent;
    private bool isStoped = false;
    public override void Init() {
        base.Init();
        cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Renderer cubeRenderer = cube.GetComponent<Renderer>();
        cubeRenderer.material = transparent;
        cube.transform.localScale = new Vector3(3f, 3, 3f);

        cube2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Renderer cubeRenderer2 = cube2.GetComponent<Renderer>();
        cubeRenderer2.material = transparent;

        waypoints = GameObject.FindGameObjectWithTag("CarWaypoints").GetComponent<Waypoints>();
        nmAgent = GetComponent<NavMeshAgent>();

        currentWaypoint = waypoints.getRandomWaypoint();

      /*  cube2.transform.position = currentWaypoint.transform.position;
        if(detect(cube2, false, true)) //detect voiture au waypoint
        {
            Debug.Log("keke");
            currentWaypoint = null;
            foreach (GameObject ob in waypoints.getAllWaypoints())
            {
                cube2.transform.position = currentWaypoint.transform.position;
                if (!detect(cube2, false, true))
                {
                    currentWaypoint = ob;
                }
            }
        }
        if(currentWaypoint == null) //aucun waypoint de dispo
        {
            Destroy(this);
            return;
        }*/

        transform.position = currentWaypoint.transform.position; // positionne la voiture sur le 1er waypoint
        Quaternion angleToNextWaypoint = Quaternion.LookRotation((waypoints.GetNextWaypoint(currentWaypoint).transform.position - this.transform.position).normalized);
        transform.rotation = angleToNextWaypoint;
        nCont = GameObject.FindObjectOfType<Controller>();

        CreateStepper(checkInFront, 1, 100);
        SetupStationary();
    }
    public void SetTarget(GameObject obj) {
        target = obj.transform.position;
        nmAgent.SetDestination(target);

        nmAgent.isStopped = false;

        CreateStepper(CheckDistToTarget, 1, 100);
    }
    void checkInFront()
    {

        cube.transform.position = AgentNavigation.addRotationToVec(this.transform.position, this.transform.rotation.eulerAngles.y, 3f, false);
        cube.transform.rotation = this.transform.rotation;
        Collider[] hitColliders = Physics.OverlapBox(cube.transform.position, cube.transform.localScale / 2, Quaternion.identity, 1);
        foreach(Collider col in hitColliders) //détecte les objets devant
        {
            if(col.transform != this.transform) 
            {
                if(col.transform.GetComponent<CarNavigation>() != null || col.transform.GetComponent<AgentNavigation>() != null) //voiture ou piéton présent
                {
                    isStoped = true;
                    nmAgent.isStopped = true;
                    return;
                }

            }
        }
        if(isStoped && target != null) //il est stopé, mais a plus rien devant lui
        {
            nmAgent.isStopped = false;
        }
    }
    void CheckDistToTarget()
    {
        float distX = this.transform.position.x - target.x;
        float distZ = this.transform.position.z - target.z;
        float distH = Mathf.Sqrt(distX * distX + distZ * distZ);
        float distV = this.transform.position.y - target.y;

        if (distH <= nCont.distToTargetThreshold)
        {
            nmAgent.isStopped = true;
            DestroyStepper("CheckDistToTarget");
            SetupStationary();
        }
    }
    void SetupStationary()
    {
        nextWaypoint = waypoints.GetNextWaypoint(currentWaypoint);
        CreateStepper(Stay);
    }
    void Stay() {
        WaypointInformation info = currentWaypoint.GetComponent<WaypointInformation>();
        if(info != null)
        {
            if(info.feu != null) 
            {
                if (!info.feu.isLightGreen()) //le feu est rouge
                {
                    return;// on attend
                }
            }

            if (info.isCenter)
            {
                WaypointInformation nextInfo = nextWaypoint.GetComponent<WaypointInformation>();
                if (nextInfo != null)
                {
                    Vector3 norma = AgentNavigation.addRotationToVec(Vector3.zero, transform.rotation.eulerAngles.y, 1, false);
                    float distance = 20;
                    if(Mathf.Abs(norma.z) > 0.8) //axis z
                    {
                        if(nextWaypoint.transform.position.x != currentWaypoint.transform.position.x) //va a gauche ou fait demi tour
                        {
                            cube2.transform.position = new Vector3(nextWaypoint.transform.position.x, currentWaypoint.transform.position.y, nextWaypoint.transform.position.z);
                            cube2.transform.position = AgentNavigation.addRotationToVec(cube2.transform.position, transform.rotation.eulerAngles.y, distance / 4, false);
                            cube2.transform.localScale = new Vector3(3, 3, distance);
                            Collider[] hitColliders = Physics.OverlapBox(cube2.transform.position, cube2.transform.localScale / 2, Quaternion.identity, 1);
                            foreach (Collider col in hitColliders) //détecte les objets venant d'en face
                            {
                                if (col.transform != this.transform && col.transform.GetComponent<CarNavigation>() != null) //voiture présente
                                {
                                    return;// on attend
                                }
                            }
                        }
                    }
                    else
                    {
                        if (nextWaypoint.transform.position.z != currentWaypoint.transform.position.z) //va a gauche ou fait demi tour
                        {
                            cube2.transform.position = new Vector3(nextWaypoint.transform.position.x, currentWaypoint.transform.position.y, nextWaypoint.transform.position.z);
                            cube2.transform.position = AgentNavigation.addRotationToVec(cube2.transform.position, transform.rotation.eulerAngles.y, distance / 4, false);
                            cube2.transform.localScale = new Vector3(distance, 3, 3);
                            Collider[] hitColliders = Physics.OverlapBox(cube2.transform.position, cube2.transform.localScale / 2, Quaternion.identity, 1);
                            foreach (Collider col in hitColliders) //détecte les objets venant d'en face
                            {
                                if (col.transform != this.transform && col.transform.GetComponent<CarNavigation>() != null) //voiture présente
                                {
                                    return;// on attend
                                }
                            }
                        }
                    }
                 
                }
            }
        }


        SetNewTarget();
        DestroyStepper("Stay");
    }

    private bool detect(GameObject currCube, bool agents, bool cars)
    {
        Collider[] hitColliders = Physics.OverlapBox(currCube.transform.position, currCube.transform.localScale / 2, Quaternion.identity, 1);
        foreach (Collider col in hitColliders) //détecte les objets dans le cube
        {
            if (cars && col.transform.GetComponent<CarNavigation>() != null) //voiture détéctée
            {
                return true;
            }
            else if (agents && col.transform.GetComponent<AgentNavigation>() != null) //agent détéctée
            {
                return true;
            }
        }
        return false;
    }
    void SetNewTarget() {
        currentWaypoint = nextWaypoint;
        SetTarget(currentWaypoint);
    }
}
