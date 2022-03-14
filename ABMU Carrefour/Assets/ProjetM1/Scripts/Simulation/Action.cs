using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action
{
    public GameObject pass;
    public Vector3 waypoint, otherSide;
    public Action(GameObject pass, Vector3 waypoint, Vector3 otherSide)
    {
        this.pass = pass;
        this.waypoint = waypoint;
        this.otherSide = otherSide;
    }
}
