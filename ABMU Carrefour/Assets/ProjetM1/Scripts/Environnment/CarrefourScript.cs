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
        generateWaypoints();
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

        if (crossX){
            Vector3 from = getRelativeVector(start);
            
            GameObject pass = getNearestObject(start, GameObject.FindGameObjectsWithTag("PassPietX"));
            KeyValuePair<Vector3, Vector3> vecs = getWaypointsNearPass(pass, from, true);
            manager.addAction(new Action(pass, vecs.Key, vecs.Value));

            start = vecs.Value; //update la prochaine position après le passage piéton pour la suite
        }
        if (crossZ){
            Vector3 from = getRelativeVector(start);

            GameObject pass = getNearestObject(start, GameObject.FindGameObjectsWithTag("PassPietZ"));
            KeyValuePair<Vector3, Vector3> vecs = getWaypointsNearPass(pass, from, false);
            manager.addAction(new Action(pass, vecs.Key, vecs.Value));
        }
        manager.addAction(new Action(null, end, Vector3.zero));

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
        float random = Random.Range(-(height / 2), height / 2);
        if (x){
            start.x += from.x == 1 ? (width / 2) : -(width / 2);
            start.z += random;
            end.x -= from.x == 1 ? (width / 2) : -(width / 2);
            end.z += random;
        } else{
            start.z += from.z == 1 ? (width / 2) : -(width / 2);
            start.x += random;
            end.z -= from.z == 1 ? (width / 2) : -(width / 2);
            end.x += random;
        }
        return new KeyValuePair<Vector3, Vector3>(start, end);
    }

    public GameObject getNearestObject(Vector3 pos, GameObject[] objs)
    {
        List<GameObject> array = new List<GameObject>(objs);
        array = array.OrderBy(o => Vector3.Distance(pos, o.transform.position)).ToList();
        return array.Count == 0 ? null : (GameObject)array[0];
    }
    public FeuScript getNearestFeu(Vector3 pos)
    {
        FeuScript[] list = FindObjectsOfType<FeuScript>();
        GameObject[] objs = new GameObject[list.Length];
        for(int i = 0; i < list.Length; i++)
        {
            objs[i] = list[i].gameObject;
        }
        return getNearestObject(pos, objs).GetComponent<FeuScript>();
    }
    /// <returns>true si middle est entre p1 et p2</returns>
    public bool shouldCrossAxis(float p1, float p2, float middle){
        return (p1-middle) * (p2-middle) < 0;
    }
    public GameObject[] getLightsInAxis(string axis)
    {
        string feu = "Feu " + axis.ToUpper();
        return GameObject.FindGameObjectsWithTag(feu);
    }
    /// <summary>
    /// met à jour le carrefour
    /// </summary>
    /// <param name="openedAxis">l'axe des feu à mettre au vert ("X" ou "Z")</param>
    /// <param name="closedAxis">l'axe des feu à mettre au rouge ("X" ou "Z")</param>
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


    public void generateWaypoints()
    {
        GameObject head = new GameObject("Waypoints");
        Transform list = head.transform;
        Waypoints waypoints = head.AddComponent<Waypoints>();
        head.tag = "CarWaypoints";
        list.parent = this.transform;

        float roadDecalage = 2.5f;
        float xMiddle = transform.position.x;
        float zMiddle = transform.position.z;
        GameObject way = new GameObject("Waypoint");
        way.transform.parent = list;
        way.transform.position = new Vector3(xMiddle + 30, 2, zMiddle + roadDecalage);
        waypoints.addWaypoint(way);

        way = new GameObject("Waypoint");
        way.transform.parent = list;
        way.transform.position = new Vector3(xMiddle + 13, 2, zMiddle + roadDecalage);
        WaypointInformation info = way.AddComponent<WaypointInformation>();
        info.feu = getNearestFeu(way.transform.position);
        waypoints.addWaypoint(way);

        way = new GameObject("Waypoint");
        way.transform.parent = list;
        way.transform.position = new Vector3(xMiddle + roadDecalage, 2, zMiddle + roadDecalage);
        info = way.AddComponent<WaypointInformation>();
        info.isCenter = true;
        waypoints.addCenterWaypoint(way);
        waypoints.addWaypoint(way);

        way = new GameObject("Waypoint");
        way.transform.parent = list;
        way.transform.position = new Vector3(xMiddle + roadDecalage, 2, zMiddle + 30);
        waypoints.addWaypoint(way);

        way = new GameObject("Waypoint");
        way.transform.parent = list;
        way.transform.position = new Vector3(xMiddle - roadDecalage, 2, zMiddle + 30);
        waypoints.addWaypoint(way);

        way = new GameObject("Waypoint");
        way.transform.parent = list;
        way.transform.position = new Vector3(xMiddle - roadDecalage, 2, zMiddle + 30);
        waypoints.addWaypoint(way);

        way = new GameObject("Waypoint");
        way.transform.parent = list;
        way.transform.position = new Vector3(xMiddle - roadDecalage, 2, zMiddle + 13);
        info = way.AddComponent<WaypointInformation>();
        info.feu = getNearestFeu(way.transform.position);
        waypoints.addWaypoint(way);

        way = new GameObject("Waypoint");
        way.transform.parent = list;
        way.transform.position = new Vector3(xMiddle - roadDecalage, 2, zMiddle + roadDecalage);
        info = way.AddComponent<WaypointInformation>();
        info.isCenter = true;
        waypoints.addCenterWaypoint(way);
        waypoints.addWaypoint(way);

        way = new GameObject("Waypoint");
        way.transform.parent = list;
        way.transform.position = new Vector3(xMiddle - 30, 2, zMiddle + roadDecalage);
        waypoints.addWaypoint(way);

        way = new GameObject("Waypoint");
        way.transform.parent = list;
        way.transform.position = new Vector3(xMiddle - 30, 2, zMiddle - roadDecalage);
        waypoints.addWaypoint(way);

        way = new GameObject("Waypoint");
        way.transform.parent = list;
        way.transform.position = new Vector3(xMiddle - 13, 2, zMiddle - roadDecalage);
        info = way.AddComponent<WaypointInformation>();
        info.feu = getNearestFeu(way.transform.position);
        waypoints.addWaypoint(way);

        way = new GameObject("Waypoint");
        way.transform.parent = list;
        way.transform.position = new Vector3(xMiddle - roadDecalage, 2, zMiddle - roadDecalage);
        info = way.AddComponent<WaypointInformation>();
        info.isCenter = true;
        waypoints.addCenterWaypoint(way);
        waypoints.addWaypoint(way);

        way = new GameObject("Waypoint");
        way.transform.parent = list;
        way.transform.position = new Vector3(xMiddle - roadDecalage, 2, zMiddle - 30);
        waypoints.addWaypoint(way);

        way = new GameObject("Waypoint");
        way.transform.parent = list;
        way.transform.position = new Vector3(xMiddle + roadDecalage, 2, zMiddle - 30);
        waypoints.addWaypoint(way);

        way = new GameObject("Waypoint");
        way.transform.parent = list;
        way.transform.position = new Vector3(xMiddle + roadDecalage, 2, zMiddle - 13);
                info = way.AddComponent<WaypointInformation>();
        info.feu = getNearestFeu(way.transform.position);
        waypoints.addWaypoint(way);

        way = new GameObject("Waypoint");
        way.transform.parent = list;
        way.transform.position = new Vector3(xMiddle + roadDecalage, 2, zMiddle - roadDecalage);
        info = way.AddComponent<WaypointInformation>();
        info.isCenter = true;
        waypoints.addCenterWaypoint(way);
        waypoints.addWaypoint(way);

        way = new GameObject("Waypoint");
        way.transform.parent = list;
        way.transform.position = new Vector3(xMiddle + 30, 2, zMiddle - roadDecalage);
        waypoints.addWaypoint(way);
    }

}
