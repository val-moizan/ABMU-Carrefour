using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action
{
    public Action next;
    public GameObject pass;
    public Vector3 waypoint;
    public Action(GameObject pass, Vector3 waypoint)
    {
        this.pass = pass;
        this.waypoint = waypoint;
    }
}
