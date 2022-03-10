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
    public override void Init(){
        base.Init();
        nCont = GameObject.FindObjectOfType<Controller>();
        nmAgent = GetComponent<NavMeshAgent>();
        this.manager = new ActionManager();
        SetupStationary();
    }
    

    public void SetTarget(GameObject obj){
        targetObject = obj;

        GameObject carrefour = GameObject.FindGameObjectWithTag("carrefour");
        CarrefourScript sc = carrefour.GetComponent<CarrefourScript>();

        
        nCont.clearAllWaypoints();
       // nCont.addWaypoint("s", transform.position);
        sc.addActionsToManager(this.transform.position, nCont.GetRandomPointInObject(obj, nCont.agentPrefab), manager);

        goToNextStep();

    }
    public void goToNextStep()
    {
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
            
            DestroyStepper("CheckDistToTarget");

            
            if(manager.getFirstAction().pass != null) //Il y a un passage piéton à traverser
            {
                //TODO: check if cars
                PassScript script = manager.getFirstAction().pass.GetComponent<PassScript>();
                if (script.isOpen())
                {
                    manager.removeFirstAction();
                    goToNextStep();
                }
                else
                {
                    CreateStepper(waitForGreen, 1, 100);
                }

            }
            else
            {
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
}
