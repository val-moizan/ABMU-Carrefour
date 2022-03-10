using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ABMU;
using UnityEngine.AI;
using System.Linq;
public class CarrefourScript : MonoBehaviour
{
    Controller nCont;
    Renderer floorRenderer;
    Bounds bounds;
    int timer = 0;
    public int delay = 2000;
    void OnValidate()
    {
        delay = Mathf.Max(delay, 1000);
    }
    void Start()
    {
        nCont = GameObject.FindObjectOfType<Controller>();
        timer = delay-1;
    }

    private void LateUpdate() {
        timer ++;
        
        if(timer == delay){
            updateLights("X", "Z");
        }else if(timer == delay * 2){
            updateLights("Z", "X");
            timer = 0;
        }
        
        
    }
    /// <param name="start">Point de départ</param>
    /// <param name="end">Point d'arrivé</param>
    /// <returns>Retourne les waypoints par lequel le piéton devra passer</returns>
    public void addActionsToManager(Vector3 start, Vector3 end, ActionManager manager){
        Vector3 middle = this.transform.position;
        bool crossX = shouldCrossAxis(start.x, end.x, middle.x);
        bool crossZ = shouldCrossAxis(start.z, end.z, middle.z);
        List<Action> actions = new List<Action>();
        if (crossX){
            Vector3 from = getRelativeVector(start);
            
            GameObject pass = getNearestObject(start, GameObject.FindGameObjectsWithTag("PassPietX"));
            KeyValuePair<Vector3, Vector3> vecs = getWaypointsNearPass(pass, from, true);
            manager.addAction(new Action(pass, vecs.Key));
            actions.Add(new Action(null, vecs.Key));
            start = vecs.Value; //update la prochaine position après le passage piéton pour la suite
        }
        if (crossZ){
            Vector3 from = getRelativeVector(start);

            GameObject pass = getNearestObject(start, GameObject.FindGameObjectsWithTag("PassPietZ"));
            KeyValuePair<Vector3, Vector3> vecs = getWaypointsNearPass(pass, from, false);
            manager.addAction(new Action(pass, vecs.Key));
            actions.Add(new Action(pass, vecs.Key));
        }
        actions.Add(new Action(null, end));
        manager.addAction(new Action(null, end));
       // DEBUGTODELET(actions);
    }
    // TODO: delete
    public void DEBUGTODELET(List<Action> objs)
    {
        int size = objs.Count;
        for (int index = 0; index < size; index++)
        {
            Action ac = ((Action)objs[index]);
            nCont.addWaypoint("" + index, ac.waypoint);
        }
    }
    public Action addAction(Action current, Action newAction){
        if(current == null){
            current = newAction;
            return current;
        }
            
        if(current.next != null)
            addAction(current.next, newAction);
        current.next = newAction;
        return current;
    }
    /// <param name="from"></param>
    /// <returns>Vecteur type (1, y , -1) correspondant aux coordonnées d'où il vient</returns>
    public Vector3 getRelativeVector(Vector3 from)
    {
        return new Vector3(from.x / Mathf.Abs(from.x), from.y, from.z / Mathf.Abs(from.z));
    }

    /// <param name="pass">L'objet passage piéton</param>
    /// <param name="from">Le point de départ du piéton</param>
    /// <param name="x"> Sur l'axe x ?</param>
    /// <returns>Les points de départ et d'arrivé du passage piéton</returns>
    public KeyValuePair<Vector3, Vector3> getWaypointsNearPass(GameObject pass, Vector3 from, bool x)
    {
        Bounds rb = pass.GetComponent<Collider>().bounds;
        Vector3 start = rb.center;
        Vector3 end = rb.center;
        float sidewalkHeight = 0.25f;
        start.y += sidewalkHeight;
        end.y += sidewalkHeight;
        float width = 10;
        float height = 4;
        float security = 1;
        width += security * 2;
        if (x){
            start.x += from.x == 1 ? (width / 2) : -(width / 2);
            start.z += Random.Range(-(height / 2), height / 2);
            end.x -= from.x == 1 ? (width / 2) : -(width / 2);
        } else{
            start.z += from.z == 1 ? (width / 2) : -(width / 2);
            start.x += Random.Range(-(height / 2), height / 2);
            end.z -= from.z == 1 ? (width / 2) : -(width / 2);
        }
        return new KeyValuePair<Vector3, Vector3>(start, end);
    }

    public GameObject getNearestObject(Vector3 pos, GameObject[] objs)
    {
        List<GameObject> array = new List<GameObject>(objs);
        array = array.OrderBy(o => Vector3.Distance(pos, o.transform.position)).ToList();
        return array.Count == 0 ? null : (GameObject)array[0];
    }
    public bool shouldCrossAxis(float p1, float p2, float middle){
        return (p1-middle) * (p2-middle) < 0;
    }
    public GameObject[] getLightsInAxis(string axis)
    {
        string feu = "Feu " + axis.ToUpper();
        return GameObject.FindGameObjectsWithTag(feu);
    }
    public void updateLights(string openedAxis, string closedAxis){
        GameObject[] objs = getLightsInAxis(closedAxis);
        foreach (GameObject obj in objs){
            FeuScript s = obj.GetComponent<FeuScript>();
            StartCoroutine(setRed(s));
        }

        objs = getLightsInAxis(openedAxis);
        foreach (GameObject obj in objs){
            FeuScript s = obj.GetComponent<FeuScript>();
            StartCoroutine(setGreen(s));
        }
    }
    public IEnumerator setRed(FeuScript s){
        s.setOrange();
        yield return new WaitForSeconds(1);
        s.setRed();
    }

    public IEnumerator setGreen(FeuScript s){

        s.setGreen();
        yield return new WaitForSeconds(0);
    }


}
