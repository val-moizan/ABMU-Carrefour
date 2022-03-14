using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ABMU.Core;

using UnityEngine.AI;

public class AgentNavigation : AbstractAgent
{
    Controller nCont;
    NavMeshAgent nmAgent;
    Vector3 target;
    public GameObject targetObject;
    public bool isNearTarget = false;
    int timeSpentSitting = 0;
    int stationayrDuration = -1;
    ActionManager manager;
    int timeLooking;
    Quaternion looking;
    Camera cam;
    public override void Init(){
        base.Init();
        nCont = GameObject.FindObjectOfType<Controller>();
        nmAgent = GetComponent<NavMeshAgent>();
        cam = GameObject.Find("Camera").GetComponent<Camera>();
        this.manager = new ActionManager();
       // this.transform.position = new Vector3(7.5f, 2, 7.5f);
        SetupStationary();
    }
    

    public void SetTarget(GameObject obj){
        targetObject = obj;

        GameObject carrefour = GameObject.FindGameObjectWithTag("carrefour");
        CarrefourScript sc = carrefour.GetComponent<CarrefourScript>();

        
        nCont.clearAllWaypoints();
        // nCont.addWaypoint("s", transform.position);
        Vector3 currTarget = nCont.GetRandomPointInObject(obj, nCont.agentPrefab);
        //currTarget = new Vector3(-7.5f, 2, -7.5f);

        sc.addActionsToManager(this.transform.position, currTarget, manager);

        goToNextStep();

    }
    public void goToNextStep()
    {
        timeLooking = 0;
        if(manager.getFirstAction() != null)
        {
            target = manager.getFirstAction().waypoint;

            nmAgent.SetDestination(target);
            nmAgent.isStopped = false;
            CreateStepper(CheckDistToTarget, 1, 100);
        }
        else
        {
            SetupStationary();
        }
    }
    void CheckDistToTarget(){

        float d = Vector3.Distance(this.transform.position, target);
        float distX = this.transform.position.x - target.x;
        float distZ = this.transform.position.z - target.z;
        float distH = Mathf.Sqrt(distX * distX + distZ * distZ);
        float distV = this.transform.position.y - target.y;
        Bounds rb = nCont.agentPrefab.transform.Find("Body").GetComponent<Collider>().bounds;
        
        float agentHeight = rb.size.y;
        if (distH <= nCont.distToTargetThreshold)
        { //reached target
            isNearTarget = true;
  
            nmAgent.isStopped = true;

            GameObject passage = manager.getFirstAction().pass;
            if (passage != null) //Il y a un passage piéton à traverser
            {

                faceTarget(manager.getFirstAction().otherSide); //regarder de l'autre coté du passage
                if (isLightGreen(passage)) //passage pieton vert
                {
                    DestroyStepper("CheckDistToTarget");
                    manager.removeFirstAction();
                    goToNextStep();
                }
                else
                {
                    //TODO: check if cars
                    if (this.nmAgent.velocity.x == 0 && this.nmAgent.velocity.z == 0) {
                        looking = this.transform.rotation;
                        CreateStepper(waitingGreen, 1, 100);
                        DestroyStepper("CheckDistToTarget");
                    }
                   // CreateStepper(waitForGreen, 1, 100);
                }

            }
            else
            {
                DestroyStepper("CheckDistToTarget");
                manager.removeFirstAction();
                goToNextStep();
            }
           

        }else{
            isNearTarget = false;
        }
 
    }

    void waitForGreen()
    {
        PassScript script = manager.getFirstAction().pass.GetComponent<PassScript>();
        if (script.isOpen())
        {
            DestroyStepper("waitForGreen");
            goToNextStep();
        }
    }


    void SetupStationary(){
        stationayrDuration = Random.Range(50,50);
        timeSpentSitting = 0;
        CreateStepper(Stay);
    }

    void Stay(){
        timeSpentSitting ++;
        if(timeSpentSitting > stationayrDuration){
            SetNewTarget();
            DestroyStepper("Stay");
        }
    }

    void SetNewTarget(){
        SetTarget(nCont.GetRandomObject(nCont.GetAllSidewalks()));
    }

    void waitingGreen()
    {
        if (isLightGreen(manager.getFirstAction().pass)) //le feu est passé au vert
        {
            DestroyStepper("waitingGreen");
            manager.removeFirstAction();
            goToNextStep();
            return;
        }
        float pourcentage = 1;
        float value = Random.Range(0, 100);
        
        if(value < pourcentage || timeLooking > 0)
        {
            timeLooking++;
            if (timeLooking < 100)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, this.looking * Quaternion.Euler(0, -90, 0), 0.1f);
            }
            else
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, this.looking * Quaternion.Euler(0, 90, 0), 0.1f);
            }
        }


       
     
    }

    private bool isLightGreen(GameObject passage)
    {
        PassScript script = passage.GetComponent<PassScript>();
        faceTarget(manager.getFirstAction().otherSide); //regarder de l'autre coté du passage
        return script.isOpen();
    }
    private void faceTarget(Vector3 destination)
    {
        Vector3 lookPos = destination - transform.position;
        lookPos.y = 0;
        Quaternion rotation = Quaternion.LookRotation(lookPos);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 0.1f);
    }
}
