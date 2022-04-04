using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    private float moveSpeed = 0.5f;
    private float scrollSpeed = 10f;
    private int i = 0;
    public LineRenderer Line;
    Vector3 p1, p2;
    bool isLineStarted;
    void Start()
    {
        Line = this.gameObject.AddComponent<LineRenderer>();
        Line.startColor = Color.blue;
        Line.endColor = Color.blue;
        // set width of the renderer
        Line.startWidth = 0.3f;
        Line.endWidth = 0.3f;



    }

    // Update is called once per frame
    void Update()
    {
        
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = this.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                
                if (i == 0)
                {
                    i++;
                    p1 = hit.point;
                }
                else
                {
                    i = 0;
                    CarrefourScript sc = getCarrefourByPos(hit.point);
                    ActionManager m = new ActionManager();
                    sc.addActionsToManager(p1, hit.point, m);
                    int z = 0;
                    Line.positionCount = m.getActions().Count;
                    foreach (Action a in m.getActions())
                    {
                        Vector3 copy = new Vector3(a.waypoint.x, a.waypoint.y+1, a.waypoint.z);
                        Line.SetPosition(z, copy);
                        z++;
                    }
                }
               
                
            }
        }
        if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0) {
            transform.position += moveSpeed * new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        }

        if (Input.GetAxis("Mouse ScrollWheel") != 0) {
            transform.position += scrollSpeed * new Vector3(0, -Input.GetAxis("Mouse ScrollWheel"), 0);
        }
        
    }
    private CarrefourScript getCarrefourByPos(Vector3 pos)
    {
        CarrefourScript[] list = FindObjectsOfType<CarrefourScript>();
        List<CarrefourScript> array = new List<CarrefourScript>(list);
        //tri des carrefour en fonction de leur distance par rapport au point
        array = array.OrderBy(o => Vector3.Distance(pos, o.gameObject.transform.position)).ToList();
        return array.Count == 0 ? null : array[0];

    }
    private void UpdateLine()
    {
        Line.positionCount++;
        Line.SetPosition(Line.positionCount - 1, GetWorldCoordinate(Input.mousePosition));
    }

    private Vector3 GetWorldCoordinate(Vector3 mousePosition)
    {
        Vector3 mousePos = new Vector3(mousePosition.x, mousePosition.y, 1);
        return Camera.main.ScreenToWorldPoint(mousePos);
    }
}
