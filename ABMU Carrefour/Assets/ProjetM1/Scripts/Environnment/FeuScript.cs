using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeuScript : MonoBehaviour
{

    Light red, orange, green, walkLight;
    private bool isWalkLightred;
    void Start()
    {
        red = this.transform.Find("CarLight").Find("RedLight").GetComponent<Light>();
        orange = this.transform.Find("CarLight").Find("OrangeLight").GetComponent<Light>();
        green = this.transform.Find("CarLight").Find("GreenLight").GetComponent<Light>();
        walkLight = this.transform.Find("PassLight").Find("WalkLight").GetComponent<Light>();
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
        walkLight.color = new Color(0, 1, 0);
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
        walkLight.color = new Color(1, 0, 0);
    }
}
