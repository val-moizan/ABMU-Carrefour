using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ABMU;
using UnityEngine.AI;
using System.Linq;
public class CarrefourScript : MonoBehaviour
{
    int timer = 0;
    public int delay = 2000;
    List<Vector3> debugW = new List<Vector3>();
    void OnValidate()
    {
        delay = Mathf.Max(delay, 2000);
    }
    void Start()
    {
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

    private void OnDrawGizmos()
    {
        for (int i = 0; i < debugW.Count; i++)
        {
            Vector3 t = debugW[i];
            Gizmos.color = Color.cyan;
            if(i == 0)
            {
                Gizmos.color = Color.green;
            }else if(i == debugW.Count - 1)
            {
                Gizmos.color = Color.red;
            }
          //  Gizmos.DrawSphere(t, 0.5f);

        }
    }
    /// <param name="start">Point de départ</param>
    /// <param name="end">Point d'arrivé</param>
    /// <returns>Retourne les waypoints par lequel le piéton devra passer</returns>
    public void addActionsToManager(Vector3 start, Vector3 end, ActionManager manager){
        Vector3 middle = this.transform.position;
        bool crossX = shouldCrossAxis(start.x, end.x, middle.x);
        bool crossZ = shouldCrossAxis(start.z, end.z, middle.z);
        debugW.Clear();
        debugW.Add(start);
        if (crossX){
            Vector3 from = getRelativeVector(start);
            
            GameObject pass = getNearestObject(start, GameObject.FindGameObjectsWithTag("PassPietX"));
            KeyValuePair<Vector3, Vector3> vecs = getWaypointsNearPass(pass, from, true);
            manager.addAction(new Action(pass, vecs.Key, vecs.Value));//point au début de passage piéton (il devra verif avant de passer au prochain)
            manager.addAction(new Action(null, vecs.Value, Vector3.zero)); //point après le passage piéton (pas besoin de verif avant de passer ua prochain)
            debugW.Add(vecs.Key);
            debugW.Add(vecs.Value);
            start = vecs.Value; //update la prochaine position après le passage piéton pour la suite
        }
        if (crossZ){
            Vector3 from = getRelativeVector(start);

            GameObject pass = getNearestObject(start, GameObject.FindGameObjectsWithTag("PassPietZ"));
            KeyValuePair<Vector3, Vector3> vecs = getWaypointsNearPass(pass, from, false);
            manager.addAction(new Action(pass, vecs.Key, vecs.Value)); //point au début de passage piéton (il devra verif avant de passer au prochain)
            manager.addAction(new Action(null, vecs.Value, Vector3.zero)); //point après le passage piéton (pas besoin de verif avant de passer ua prochain)
            debugW.Add(vecs.Key);
            debugW.Add(vecs.Value);
        }
        debugW.Add(end);
        manager.addAction(new Action(null, end, Vector3.zero));

    }
    /// <param name="from"></param>
    /// <returns>Vecteur type (1, y , -1) correspondant aux coordonnées d'où il vient en fonction du centre du carrefour</returns>
    public Vector3 getRelativeVector(Vector3 from)
    {
        float x = from.x - this.transform.position.x;
        float z = from.z - this.transform.position.z;
        return new Vector3(x / Mathf.Abs(x), from.y, z / Mathf.Abs(z));
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
        return array.Count == 0 ? null : array[0];
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
        float feuDistance = 16;
        float roadSize = 108;
        GameObject way = new GameObject("Waypoint0");
        way.transform.parent = list;
        way.transform.position = new Vector3(xMiddle + roadSize, 2, zMiddle + roadDecalage);
        way.AddComponent<WaypointInformation>();
        waypoints.addWaypoint(way);

        way = new GameObject("Waypoint1");
        way.transform.parent = list;
        way.transform.position = new Vector3(xMiddle + feuDistance, 2, zMiddle + roadDecalage);
        WaypointInformation info = way.AddComponent<WaypointInformation>();
        info.feu = getNearestFeu(way.transform.position);
        waypoints.addWaypoint(way);

        way = new GameObject("Waypoint2");
        way.transform.parent = list;
        way.transform.position = new Vector3(xMiddle + roadDecalage, 2, zMiddle + roadDecalage);
        info = way.AddComponent<WaypointInformation>();
        info.isCenter = true;
        waypoints.addCenterWaypoint(way);
        waypoints.addWaypoint(way);

        way = new GameObject("Waypoint3");
        way.transform.parent = list;
        way.transform.position = new Vector3(xMiddle + roadDecalage, 2, zMiddle + roadSize);
        info = way.AddComponent<WaypointInformation>();
        info.isOut = true;
        waypoints.addWaypoint(way);

        way = new GameObject("Waypoint4");
        way.transform.parent = list;
        way.transform.position = new Vector3(xMiddle - roadDecalage, 2, zMiddle + roadSize);
        way.AddComponent<WaypointInformation>();
        waypoints.addWaypoint(way);

        way = new GameObject("Waypoint5");
        way.transform.parent = list;
        way.transform.position = new Vector3(xMiddle - roadDecalage, 2, zMiddle + feuDistance);
        info = way.AddComponent<WaypointInformation>();
        info.feu = getNearestFeu(way.transform.position);
        waypoints.addWaypoint(way);

        way = new GameObject("Waypoint6");
        way.transform.parent = list;
        way.transform.position = new Vector3(xMiddle - roadDecalage, 2, zMiddle + roadDecalage);
        info = way.AddComponent<WaypointInformation>();
        info.isCenter = true;
        waypoints.addCenterWaypoint(way);
        waypoints.addWaypoint(way);

        way = new GameObject("Waypoint7");
        way.transform.parent = list;
        way.transform.position = new Vector3(xMiddle - roadSize, 2, zMiddle + roadDecalage);
        info = way.AddComponent<WaypointInformation>();
        info.isOut = true;
        waypoints.addWaypoint(way);

        way = new GameObject("Waypoint8");
        way.transform.parent = list;
        way.transform.position = new Vector3(xMiddle - roadSize, 2, zMiddle - roadDecalage);
        way.AddComponent<WaypointInformation>();
        waypoints.addWaypoint(way);

        way = new GameObject("Waypoint9");
        way.transform.parent = list;
        way.transform.position = new Vector3(xMiddle - feuDistance, 2, zMiddle - roadDecalage);
        info = way.AddComponent<WaypointInformation>();
        info.feu = getNearestFeu(way.transform.position);
        waypoints.addWaypoint(way);

        way = new GameObject("Waypoint10");
        way.transform.parent = list;
        way.transform.position = new Vector3(xMiddle - roadDecalage, 2, zMiddle - roadDecalage);
        info = way.AddComponent<WaypointInformation>();
        info.isCenter = true;
        waypoints.addCenterWaypoint(way);
        waypoints.addWaypoint(way);

        way = new GameObject("Waypoint11");
        way.transform.parent = list;
        way.transform.position = new Vector3(xMiddle - roadDecalage, 2, zMiddle - roadSize);
        info = way.AddComponent<WaypointInformation>();
        info.isOut = true;
        waypoints.addWaypoint(way);

        way = new GameObject("Waypoint12");
        way.transform.parent = list;
        way.transform.position = new Vector3(xMiddle + roadDecalage, 2, zMiddle - roadSize);
        way.AddComponent<WaypointInformation>();
        waypoints.addWaypoint(way);

        way = new GameObject("Waypoint13");
        way.transform.parent = list;
        way.transform.position = new Vector3(xMiddle + roadDecalage, 2, zMiddle - feuDistance);
                info = way.AddComponent<WaypointInformation>();
        info.feu = getNearestFeu(way.transform.position);
        waypoints.addWaypoint(way);

        way = new GameObject("Waypoint14");
        way.transform.parent = list;
        way.transform.position = new Vector3(xMiddle + roadDecalage, 2, zMiddle - roadDecalage);
        info = way.AddComponent<WaypointInformation>();
        info.isCenter = true;
        waypoints.addCenterWaypoint(way);
        waypoints.addWaypoint(way);

        way = new GameObject("Waypoint15");
        way.transform.parent = list;
        way.transform.position = new Vector3(xMiddle + roadSize, 2, zMiddle - roadDecalage);
        info = way.AddComponent<WaypointInformation>();
        info.isOut = true;
        waypoints.addWaypoint(way);
    }

}
