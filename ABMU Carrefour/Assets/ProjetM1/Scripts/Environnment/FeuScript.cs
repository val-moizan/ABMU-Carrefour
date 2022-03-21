using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FeuScript : MonoBehaviour
{

    Light red, orange, green;
    Light[] walkLights;
    private bool isWalkLightred, isLightRed;
    void Start()
    {
        red = this.transform.Find("CarLight").Find("RedLight").GetComponent<Light>();
        orange = this.transform.Find("CarLight").Find("OrangeLight").GetComponent<Light>();
        green = this.transform.Find("CarLight").Find("GreenLight").GetComponent<Light>();
        Transform[] lights = findChildren("WalkLight");
        int size = lights.Length;
        walkLights = new Light[2];
        for(int i = 0; i < size; i++)
        {
            walkLights[i] = lights[i].GetComponent<Light>();
        }
        isWalkLightred = true;
        isLightRed = true;
    }

    public bool isWalkLightRed()
    {
        return isWalkLightred;
    }
    public bool isLightGreen()
    {
        return !isLightRed;
    }
    public void setRed(){
        red.intensity = 5;
        orange.intensity = 0;
        green.intensity = 0;
        isLightRed = true;

        StartCoroutine(setWalkGreen());
    }
    public IEnumerator setWalkGreen()
    {
        
        yield return new WaitForSeconds(2);
        isWalkLightred = false;
        foreach(Light walkLight in walkLights)
        {
            walkLight.color = new Color(0, 1, 0);
        }
        
    }
    public void setOrange(){
        red.intensity = 0;
        orange.intensity = 5;
        green.intensity = 0;
        isLightRed = true;
    }

    public void setGreen(){

        StartCoroutine(setGreenDelay());
    }
    public IEnumerator setGreenDelay()
    {
        foreach (Light walkLight in walkLights)
        {
            walkLight.color = new Color(1, 0, 0);
        }
        isWalkLightred = true;
        yield return new WaitForSeconds(2);
        isLightRed = false;
        red.intensity = 0;
        orange.intensity = 0;
        green.intensity = 5;



    }
    public Transform[] findChildren(string name)
    {
        return this.transform.GetComponentsInChildren<Transform>().Where(t => t.name == name).ToArray();
    }
}
