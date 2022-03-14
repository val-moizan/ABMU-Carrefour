using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FeuScript : MonoBehaviour
{

    Light red, orange, green;
    Light[] walkLights;
    private bool isWalkLightred;
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
    }

    public bool isWalkLightRed(){
        return isWalkLightred;
    }
    public void setRed(){
        red.intensity = 1;
        orange.intensity = 0;
        green.intensity = 0;


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
        orange.intensity = 1;
        green.intensity = 0;
    }

    public void setGreen(){
        red.intensity = 0;
        orange.intensity = 0;
        green.intensity = 1;
        isWalkLightred = true;
        foreach (Light walkLight in walkLights)
        {
            walkLight.color = new Color(1, 0, 0);
        }
    }
    public Transform[] findChildren(string name)
    {
        return this.transform.GetComponentsInChildren<Transform>().Where(t => t.name == name).ToArray();
    }
}
