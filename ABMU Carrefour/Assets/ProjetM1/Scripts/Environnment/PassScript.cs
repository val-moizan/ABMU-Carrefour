using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class PassScript : MonoBehaviour
{
    private FeuScript feu;
    void Start()
    {
        this.feu = getNearestLight();
    }

    public FeuScript getLight()
    {
        return this.feu;
    }
    private FeuScript getNearestLight()
    {
        FeuScript[] list = FindObjectsOfType<FeuScript>();
        List<FeuScript> array = new List<FeuScript>(list);
        array = array.OrderBy(o => Vector3.Distance(this.transform.position, o.gameObject.transform.position)).ToList();
        return array.Count == 0 ? null : (FeuScript)array[0];
    }
    public bool isOpen()
    {
        return !feu.isWalkLightRed();
    }
}
