using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ABMU.Core;

using UnityEngine.AI;
using System.Linq;

public class AgentNavigation : AbstractAgent
{
    Controller nCont;
    NavMeshAgent nmAgent;
    Vector3 target, finalDestination;

    public RuntimeAnimatorController walkingAnim, idleAnim;
    public Material transparent;

    int timeSpentStoped = 0;
    ActionManager manager;
    int timeLooking;
    Quaternion looking;
    Animator animator;
    GameObject cube;
    bool isThug, isStoped = false, isStaying = false;
    CarrefourScript currentCarrefour;
    public override void Init()
    {
        base.Init();
        nCont = GameObject.FindObjectOfType<Controller>();
        nmAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        // cam = GameObject.Find("Camera").GetComponent<Camera>();
        this.manager = new ActionManager();

        //this.transform.position = new Vector3(7.5f, 2, -220 + 115);
        nmAgent.Warp(this.transform.position);
        goToNextStep();
        float rand = Random.Range(0f, 100f);
        if (rand < nCont.thugProbability)
        {
            isThug = true;
            this.transform.Find("DummyMesh").GetComponent<Renderer>().material.SetColor("_Color", new Color(1f, 0f, 0f));
        }
        else
        {
            isThug = false;
            this.transform.Find("DummyMesh").GetComponent<Renderer>().material.SetColor("_Color", new Color(0f, 1f, 0f));
        }

        CreateStepper(checkInFront, 1, 100);
    }

    void checkInFront()
    {
        CarrefourScript newCarrefour = getCarrefourByPos(this.transform.position);

        if(newCarrefour != null && newCarrefour != currentCarrefour) //on arrive à un nouveau carrefour
        {
            updatePath(finalDestination, newCarrefour); //update du chemin à partir du nouveau carrefour
            return;
        }



        Vector3 checkPos = Utils.addRotationToVec(this.transform.position, this.transform.rotation.eulerAngles.y, 1f, false);
        if(timeSpentStoped == 10000)
        {
            timeSpentStoped = -100;
        }
        if (Utils.detect(checkPos, new Vector3(1, 3, 1), this.gameObject, false, true, false) && timeSpentStoped >= 0)
        {
            isStoped = true;
            nmAgent.isStopped = true;
            animator.runtimeAnimatorController = idleAnim;
            timeSpentStoped++;
            return;
        }
        if (isStoped && target != null && !isStaying) //il est stopé, mais a plus rien devant lui
        {
            timeSpentStoped = 0;
            nmAgent.isStopped = false;
            animator.runtimeAnimatorController = walkingAnim;
        }
    }
    public void SetTarget(GameObject obj)
    {

        finalDestination = nCont.GetRandomPointInObject(obj, nCont.agentPrefab);
        //finalDestination = new Vector3(7.5f, 2, -7.5f - 220);

        currentCarrefour = getCarrefourByPos(this.transform.position);

        currentCarrefour.addActionsToManager(this.transform.position, finalDestination, manager);

        goToNextStep();

    }
    public void updatePath(Vector3 destination, CarrefourScript carrefour)
    {
        currentCarrefour = carrefour;
        manager.clearAllActions(); //reset tout

        currentCarrefour.addActionsToManager(this.transform.position, finalDestination, manager); //recalcul du path depuis le nouveau carrefour

        goToNextStep();

    }
    private CarrefourScript getCarrefourByPos(Vector3 pos)
    {
        CarrefourScript [] list = FindObjectsOfType<CarrefourScript>();
        List<CarrefourScript> array = new List<CarrefourScript>(list);
        //tri des carrefour en fonction de leur distance par rapport au point
        array = array.OrderBy(o => Vector3.Distance(pos, o.gameObject.transform.position)).ToList();
        return array.Count == 0 ? null : array[0];

    }
    public void goToNextStep()
    {
        timeLooking = 0;
        if (manager.getFirstAction() != null)
        {
            target = manager.getFirstAction().waypoint;
            nmAgent.SetDestination(target);
            nmAgent.isStopped = false;
            CreateStepper(CheckDistToTarget, 1, 100);
            animator.runtimeAnimatorController = walkingAnim;
            isStaying = false;
        }
        else
        {
            SetNewTarget();
        }
    }
    void CheckDistToTarget()
    {

        float d = Vector3.Distance(this.transform.position, target);
        float distX = this.transform.position.x - target.x;
        float distZ = this.transform.position.z - target.z;
        float distH = Mathf.Sqrt(distX * distX + distZ * distZ);
        float distV = this.transform.position.y - target.y;
        //Bounds rb = nCont.agentPrefab.transform.Find("Body").GetComponent<Collider>().bounds;
        float speed = Mathf.Sqrt(nmAgent.velocity.x * nmAgent.velocity.x + nmAgent.velocity.z * nmAgent.velocity.z);
        float scale = this.transform.localScale.y / 2;
        animator.speed = speed / (nmAgent.speed * scale); //Vitesse d'animation qui correspond à la vitesse du piéton
        //float agentHeight = 2;// rb.size.y;
        if (distH <= nCont.distToTargetThreshold)
        { //reached target
            DestroyStepper("CheckDistToTarget");
            CreateStepper(Stay);
            isStaying = true;
            nmAgent.isStopped = true;
        }

    }


    /// <summary>
    /// Fonction appelé à chaque waypoint
    /// Appelé en boucle tant qu'il ne peut pas aller au prochain waypoint
    /// </summary>
    void Stay()
    {
        
        GameObject passage = manager.getFirstAction().pass;
        if (passage != null) //Il y a un passage piéton à traverser
        {

            if (isLightGreen(passage)) //passage pieton vert
            {
                DestroyStepper("Stay");
                manager.removeFirstAction();
                goToNextStep();
            }
            else
            {
                
                if (this.nmAgent.velocity.x == 0 && this.nmAgent.velocity.z == 0) //une fois à l'arrêt
                {
                    
                    looking = this.transform.rotation;
                    passage.GetComponent<PassScript>().addWaitingPedestrian(this.gameObject);
                    CreateStepper(waitingGreen, 1, 100); //attend le feu
                    if (isThug)
                    {
                        CreateStepper(checkingCars, 1, 100); //verif si voiture si "voyou"
                    }

                    DestroyStepper("Stay");
                    timeLooking = 1;
                    animator.runtimeAnimatorController = idleAnim;
                }
            }

        }
        else
        {
            //pas de passage piéton, on avance
            DestroyStepper("Stay");
            manager.removeFirstAction();
            goToNextStep();
        }
    }

    void SetNewTarget()
    {
        SetTarget(nCont.GetRandomObject(nCont.GetAllSidewalks()));
    }
    /// <summary>
    /// Appelé en boucle tant qu'il ne peut pas aller au prochain waypoint
    /// </summary>
    void waitingGreen()
    {
       // Debug.Log("Waiting green...");
        GameObject passage = manager.getFirstAction().pass;
        if(passage != null)
        {
            if (isLightGreen(manager.getFirstAction().pass)) //le feu est passé au vert
            {
                StartCoroutine(manager.getFirstAction().pass.GetComponent<PassScript>().removeWaitingPedestrian(this.gameObject));
                animator.runtimeAnimatorController = walkingAnim;
                DestroyStepper("waitingGreen");
                if (isThug)
                {
                    DestroyStepper("checkingCars");
                }
                manager.removeFirstAction();
                goToNextStep();
            }
        }

    }

    void checkingCars()
    {
        if (timeLooking < 0) //attente avant de re vérifier
        {
            timeLooking++;
            if (timeLooking == 0)
            {
                timeLooking++;
            }
        }
        if (timeLooking > 0)
        {
            timeLooking++;
            Transform[] children = GetComponentsInChildren<Transform>(true);
            Transform head = this.transform;
            for (int z = 0; z < children.Length; z++)
            {
                Transform child = children[z];
                if (child.name.Equals("B-neck"))
                {
                    //head = child;
                }
            }

            if (timeLooking < 100)
            {

                head.rotation = Quaternion.Slerp(head.rotation, this.looking * Quaternion.Euler(0, -90, 0), 0.1f);
            }
            else if (timeLooking < 200)
            {
                head.rotation = Quaternion.Slerp(head.rotation, this.looking * Quaternion.Euler(0, 90, 0), 0.1f);
            }
            else
            {

                head.rotation = Quaternion.Slerp(head.rotation, this.looking, 0.1f);
                if (!carsComing())
                {
                    StartCoroutine(manager.getFirstAction().pass.GetComponent<PassScript>().removeWaitingPedestrian(this.gameObject));
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
        cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

        CarDetector detector = cube.AddComponent<CarDetector>();
        Renderer cubeRenderer = cube.GetComponent<Renderer>();
        cubeRenderer.material = transparent;
        float distance = nCont.viewDistance;
        cube.transform.localScale = absVec(Utils.addRotationToVec(new Vector3(8, 2, 8), this.looking.eulerAngles.y + 90, distance * 2, true));//agrandit le cube en largeur perpendiculaire à la rotation du piéton
        cube.transform.position = Utils.addRotationToVec(this.transform.position, this.looking.eulerAngles.y, 6, false); //avance le cube sur la route en direction de la rotation du piéton
        if (detector.carsAreComing(manager.getFirstAction().pass.transform.position) && !manager.getFirstAction().pass.GetComponent<PassScript>().containsCars())
        {
            Destroy(cube);
            return true;
        }
        Destroy(cube);
        return false;
    }
    private bool isLightGreen(GameObject passage)
    {
        PassScript script = passage.GetComponent<PassScript>();
        faceTarget(manager.getFirstAction().otherSide); //regarder de l'autre coté du passage
        return script.isOpen() && !script.containsCars();
    }
    private void faceTarget(Vector3 destination)
    {
        Quaternion rotation = Utils.getLookingRotation(transform.position, destination);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 0.1f);
    }
    private Vector3 absVec(Vector3 vec)
    {
        return new Vector3(Mathf.Abs(vec.x), vec.y, Mathf.Abs(vec.z));
    }

}
