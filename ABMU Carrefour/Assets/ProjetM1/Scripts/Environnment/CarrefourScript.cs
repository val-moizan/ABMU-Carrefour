using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ABMU;

public class CarrefourScript : MonoBehaviour
{
    Controller nCont;
    Renderer floorRenderer;
    Bounds bounds;
    int timer = 0;
    void Start()
    {
        nCont = GameObject.FindObjectOfType<Controller>();

       // floorRenderer = this.transform.GetChild(0).GetComponent<Renderer>();
      //  this.GetComponent<Renderer>().enabled = false;
        
       
    }

    private void LateUpdate() {
        timer ++;
        int value = 2000;
        if(timer == value){
            GameObject[] objs = GameObject.FindGameObjectsWithTag("Feu X");
            foreach(GameObject obj in objs){
                FeuScript s = obj.GetComponent<FeuScript>();
                StartCoroutine(setRed(s));
            }

            GameObject[] objs2 = GameObject.FindGameObjectsWithTag("Feu Z");
            foreach(GameObject obj in objs2){
                FeuScript s = obj.GetComponent<FeuScript>();
                StartCoroutine(setGreen(s));
            }
            //Debug.Log("Z");
        }else if(timer == value * 2){
            GameObject[] objs = GameObject.FindGameObjectsWithTag("Feu Z");
            foreach(GameObject obj in objs){
                FeuScript s = obj.GetComponent<FeuScript>();
                StartCoroutine(setRed(s));
            }

            GameObject[] objs2 = GameObject.FindGameObjectsWithTag("Feu X");
            foreach(GameObject obj in objs2){
                FeuScript s = obj.GetComponent<FeuScript>();
                StartCoroutine(setGreen(s));
            }
            //Debug.Log("X");
            timer = 0;
        }
        
        
    }

    public IEnumerator setRed(FeuScript s){
        s.setOrange();
        yield return new WaitForSeconds(1);
        s.setRed();
    }

    public IEnumerator setGreen(FeuScript s){
        yield return new WaitForSeconds(1);
        s.setGreen();
    }


}
