using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ABMU.Core;

using UnityEngine.AI;

public class CarNavigation : AbstractAgent
{
    Controller nCont;
    NavMeshAgent nmAgent;
    Vector3 target;
    public GameObject targetObject;
    public bool isNearTarget = false;

    int timeSpentSitting = 0;
    int stationayrDuration = -1;
    
    public override void Init(){
        base.Init();
        nCont = GameObject.FindObjectOfType<Controller>();
        nmAgent = GetComponent<NavMeshAgent>();
        Debug.Log("car");
        CreateStepper(CheckDistToTarget, 1, 100);
        SetupStationary();
    }
    

    public void SetTarget(GameObject obj){
        targetObject = obj;
        target = nCont.GetRandomPointInObject(targetObject, nCont.carPrefab);
        nmAgent.SetDestination(target);
        nmAgent.isStopped = false;
        
        CreateStepper(CheckDistToTarget, 1, 100);
    }

    void CheckDistToTarget(){
        float d = Vector3.Distance(this.transform.position, target);
        if(d < nCont.distToTargetThreshold){ //reached target
            isNearTarget = true;

            nmAgent.isStopped = true;
            
            DestroyStepper("CheckDistToTarget");
            DestroyStepper("Move");

            SetupStationary();
        }
        else{
            isNearTarget = false;
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
        SetTarget(nCont.GetRandomObject(nCont.GetAllRoads()));
    }
}
