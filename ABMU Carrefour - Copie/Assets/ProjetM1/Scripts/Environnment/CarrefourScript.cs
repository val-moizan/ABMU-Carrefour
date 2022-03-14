using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ABMU;
using UnityEngine.AI;

public class CarrefourScript : MonoBehaviour
{
    Controller nCont;
    Renderer floorRenderer;
    Bounds bounds;
    int timer = 0;
    public int delay = 2000;

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
