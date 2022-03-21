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

    public RuntimeAnimatorController walkingAnim, idleAnim;
    public Material transparent;
    int timeSpentSitting = 0;
    int stationayrDuration = -1;
    ActionManager manager;
    int timeLooking;
    Quaternion looking;
    Animator animator;
    GameObject cube;
    bool isThug;
    public override void Init(){
        base.Init();
        nCont = GameObject.FindObjectOfType<Controller>();
        nmAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        // cam = GameObject.Find("Camera").GetComponent<Camera>();
        this.manager = new ActionManager();
       // this.transform.position = new Vector3(7.5f, 2, 7.5f);

        SetupStationary();

        float rand = Random.Range(0f, 100f);
        if(rand < 10)
        {
            isThug = true;
            this.transform.Find("DummyMesh").GetComponent<Renderer>().material.SetColor("_Color", new Color(1f, 0f, 0f));
        }
        else
        {
            isThug = false;
            this.transform.Find("DummyMesh").GetComponent<Renderer>().material.SetColor("_Color", new Color(0f, 1f, 0f));
        }

    }


    public void SetTarget(GameObject obj){

        GameObject carrefour = GameObject.FindGameObjectWithTag("carrefour");
        CarrefourScript sc = carrefour.GetComponent<CarrefourScript>();


        nCont.clearAllWaypoints();
        // nCont.addWaypoint("s", transform.position);
        Vector3 currTarget = nCont.GetRandomPointInObject(obj, nCont.agentPrefab);
        currTarget.x = Mathf.Min(currTarget.x, 10);
        currTarget.z = Mathf.Min(currTarget.z, 10);

        currTarget.x = Mathf.Max(currTarget.x, -10);
        currTarget.z = Mathf.Max(currTarget.z, -10);
        //    currTarget = new Vector3(-7.5f, 2, -7.5f);

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
            animator.runtimeAnimatorController = walkingAnim;
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
        //Bounds rb = nCont.agentPrefab.transform.Find("Body").GetComponent<Collider>().bounds;
        float speed = Mathf.Sqrt(nmAgent.velocity.x * nmAgent.velocity.x + nmAgent.velocity.z * nmAgent.velocity.z);
        animator.speed = speed / nmAgent.speed;
        //float agentHeight = 2;// rb.size.y;
        if (distH <= nCont.distToTargetThreshold)
        { //reached target

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
                        if (isThug)
                        {
                            CreateStepper(checkingCars, 1, 100);
                        }

                        DestroyStepper("CheckDistToTarget");
                        timeLooking = 1;
                        animator.runtimeAnimatorController = idleAnim;
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
            animator.runtimeAnimatorController = walkingAnim;
            DestroyStepper("waitingGreen");
            manager.removeFirstAction();
            goToNextStep();
            return;
        }
    }

    void checkingCars()
    {
        if(timeLooking < 0) //attente avant de re vérifier
        {
            timeLooking++;
            if(timeLooking == 0)
            {
                timeLooking++;
            }
        }
        if (timeLooking > 0)
        {
            timeLooking++;
            if (timeLooking < 100)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, this.looking * Quaternion.Euler(0, -90, 0), 0.1f);
            }
            else if (timeLooking < 200)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, this.looking * Quaternion.Euler(0, 90, 0), 0.1f);
            }
            else
            {

                transform.rotation = Quaternion.Slerp(transform.rotation, this.looking, 0.1f);
                if (!carsComing())
                {
                    timeLooking = 0;
                    animator.runtimeAnimatorController = walkingAnim;
                    DestroyStepper("waitingGreen");
                    DestroyStepper("checkingCars");
                    manager.removeFirstAction();
                    goToNextStep();
                }
                else
                {
                    //on recommence à vérifier
                    timeLooking = -100;
                }
            }
        }
    }
    private bool carsComing()
    {
        if(cube != null)
        {
            Destroy(cube);
        }
        cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

        CarDetector detector = cube.AddComponent<CarDetector>();
        Renderer cubeRenderer = cube.GetComponent<Renderer>();
        cubeRenderer.material = transparent;
        float distance = 20;
        cube.transform.localScale = absVec(addRotationToVec(new Vector3(8, 2, 8), this.looking.eulerAngles.y + 90, distance * 2, true));//agrandit le cube en largeur perpendiculaire à la rotation du piéton
        cube.transform.position = addRotationToVec(this.transform.position, this.looking.eulerAngles.y, 6, false); //avance le cube sur la route en direction de la rotation du piéton
        if (detector.carsAreComing(manager.getFirstAction().pass.transform.position))
        {
            return true;
        }


        return false;
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
    private Vector3 absVec(Vector3 vec)
    {
        return new Vector3(Mathf.Abs(vec.x), vec.y, Mathf.Abs(vec.z));
    }
    private Vector3 addRotationToVec(Vector3 baseVec, float rotation, float distance, bool abs)
    {
        float x = Mathf.Round(Mathf.Cos((rotation - 90) * Mathf.Deg2Rad)) * distance;
        float z = Mathf.Round(Mathf.Sin((rotation + 90) * Mathf.Deg2Rad)) * distance;
        if (abs)
        {
            x = Mathf.Abs(x);
            z = Mathf.Abs(z);
        }
        return new Vector3(baseVec.x + x, baseVec.y, baseVec.z + z);
    }
}
