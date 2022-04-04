using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ABMU.Core;
using UnityEngine.AI;
using System.Linq;

public class CarNavigation : AbstractAgent {
    Controller nCont;
    NavMeshAgent nmAgent;
    Vector3 target;
    //public GameObject targetObject;
    public GameObject previousWaypoint, currentWaypoint, nextWaypoint;

    public bool turningRight = false, turningLeft = false, isStoped = false;
    private bool stopCheckingfront = false;
    private int ticks = 0;
    public override void Init() {
        base.Init();
        updateClignotant("R", false);
        updateClignotant("L", false);
        nmAgent = GetComponent<NavMeshAgent>();

      //  Quaternion angleToNextWaypoint = Quaternion.LookRotation((waypoints.GetNextWaypoint(currentWaypoint).transform.position - this.transform.position).normalized);
      //  transform.rotation = angleToNextWaypoint;
        nCont = GameObject.FindObjectOfType<Controller>();


    }

    private void LateUpdate()
    {
        string side = turningLeft ? "L" : (turningRight ? "R" : "");
        if(side.Equals("")) return;
        ticks++;
        if(ticks == 100)
        {
            updateClignotant(side, true);
        }
        else if(ticks > 200)
        {
            updateClignotant(side, false);
            ticks = 0;
        }
    }
    public void setStartWaypoint(GameObject waypoint)
    {
        
        currentWaypoint = waypoint;
        nmAgent.Warp(waypoint.transform.position);
        transform.position = waypoint.transform.position; // positionne la voiture sur le 1er waypoint
        Waypoints waypoints = waypoint.transform.parent.gameObject.GetComponent<Waypoints>();
        transform.rotation = Utils.getLookingRotation(this.transform.position, waypoints.GetNextWaypoint(currentWaypoint, null).Key.transform.position); //rotation direction le prochain waypoint
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

        Vector3 checkPos = Utils.addRotationToVec(this.transform.position, this.transform.rotation.eulerAngles.y, stopCheckingfront ? 1.5f : 2, false);
        if (Utils.detect(checkPos, new Vector3(3,3,3), this.gameObject, true, true, false))
        {
            isStoped = true;
            nmAgent.isStopped = true;
            return;
        }
        if(isStoped && target != null) //il est stopé, mais a plus rien devant lui
        {
            isStoped = false;
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
        WaypointInformation currentInfo = currentWaypoint.GetComponent<WaypointInformation>();
        Waypoints waypoints = currentWaypoint.transform.parent.gameObject.GetComponent<Waypoints>();
        if (currentInfo.isOut) //arrive en bout de carrefour
        {
            Vector3 outPos = Utils.addRotationToVec(transform.position, transform.rotation.eulerAngles.y, 5, false);
            WaypointInformation info = getNearestWaypoint(outPos);
            Waypoints newWaypoints = info.transform.parent.gameObject.GetComponent<Waypoints>();
            if (newWaypoints != waypoints) //nouveau carrefour détecté
            {
                nextWaypoint = info.gameObject;
                turningRight = false;
                CreateStepper(Stay);
                return;
            }
        }

        KeyValuePair<GameObject, bool> pair = waypoints.GetNextWaypoint(currentWaypoint, previousWaypoint);
        nextWaypoint = pair.Key;
        turningRight = pair.Value;
        CreateStepper(Stay);
    }
    void Stay() {
        WaypointInformation info = currentWaypoint.GetComponent<WaypointInformation>();

        if (info.feu != null)
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
                Vector3 norma = Utils.addRotationToVec(Vector3.zero, transform.rotation.eulerAngles.y, 1, false);
                float distance = 25;
                if (Mathf.Abs(norma.z) > 0.8) //axis z
                {
                    if (nextWaypoint.transform.position.x != currentWaypoint.transform.position.x) //va a gauche ou fait demi tour
                    {
                        Vector3 checkPos = new Vector3(nextWaypoint.transform.position.x, currentWaypoint.transform.position.y, nextWaypoint.transform.position.z);
                        checkPos = Utils.addRotationToVec(checkPos, transform.rotation.eulerAngles.y, distance / 4, false);
                        if (Utils.detectNonTurningCars(checkPos, new Vector3(3, 3, distance), this.gameObject))
                        {
                            turningLeft = true;
                            stopCheckingfront = true;
                            return;
                        }
                    }
                }
                else
                {
                    if (nextWaypoint.transform.position.z != currentWaypoint.transform.position.z) //va a gauche ou fait demi tour
                    {
                        Vector3 checkPos = new Vector3(nextWaypoint.transform.position.x, currentWaypoint.transform.position.y, nextWaypoint.transform.position.z); //position du waypoint à gauche
                        checkPos = Utils.addRotationToVec(checkPos, transform.rotation.eulerAngles.y, distance / 4, false);
                        if (Utils.detectNonTurningCars(checkPos, new Vector3(distance, 3, 3), this.gameObject))
                        {
                            turningLeft = true;
                            stopCheckingfront = true;
                            return;
                        }
                    }
                }
            }

        }


        if (turningRight)
        {

            Quaternion nextRot = Utils.getLookingRotation(currentWaypoint.transform.position, nextWaypoint.transform.position); //rotation direction prochain waypoint
            Vector3 pointNearPass = Utils.addRotationToVec(currentWaypoint.transform.position, nextRot.eulerAngles.y, 1, false); //on ajoute 1 de distance en direction du prochain waypoint à droite
            PassScript sc = getNearestPass(pointNearPass);
            if (sc.containsPedestrians() || (sc.hasWaitingPedestrians() && sc.isOpen())) //si piétons présent OU piétons attendent avec feu vert pour eux
            {
                return; //on attend
            }
            updateClignotant("R", false);

        }
        if (turningLeft)
        {
            turningLeft = false;
            updateClignotant("L", false);
        }

        StartCoroutine(checkAgain());
       
        SetNewTarget();
        DestroyStepper("Stay");
    }
    
    void SetNewTarget() {
        previousWaypoint = currentWaypoint;
        currentWaypoint = nextWaypoint;
        SetTarget(currentWaypoint);
    }
    public IEnumerator checkAgain()
    {
        yield return new WaitForSeconds(1);
        stopCheckingfront = false;
    }
        private PassScript getNearestPass(Vector3 pos)
    {
        PassScript[] list = FindObjectsOfType<PassScript>();
        List<PassScript> array = new List<PassScript>(list);
        array = array.OrderBy(o => Vector3.Distance(pos, o.gameObject.transform.position)).ToList();
        return array.Count == 0 ? null : array[0];
    }

    private void updateClignotant(string side, bool open)
    {
        MeshRenderer re = this.transform.Find("realcar").Find("Car").GetComponent<MeshRenderer>();
        foreach(Material mat in re.materials)
        {
            if (mat.name.Equals("Turnsignal" + side.ToUpper() + " (Instance)"))
            {
                if (open)
                {
                    mat.SetColor("_Color", new Color(1f, 0.4f, 0f));
                }
                else
                {
                    mat.SetColor("_Color", new Color(0.5f, 0.1f, 0f));
                }
                
            }
        }
    }
    private WaypointInformation getNearestWaypoint(Vector3 pos)
    {
        WaypointInformation[] list = FindObjectsOfType<WaypointInformation>();
        List<WaypointInformation> array = new List<WaypointInformation>(list);
        //tri des waypoint en fonction de leur distance par rapport au point
        array = array.OrderBy(o => Vector3.Distance(pos, o.gameObject.transform.position)).ToList();
        return array.Count == 0 ? null : array[0];
    }
}
