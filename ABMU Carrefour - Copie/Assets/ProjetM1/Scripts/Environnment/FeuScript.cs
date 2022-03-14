using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeuScript : MonoBehaviour
{

    Light red, orange, green;
    private bool isred;
    void Start()
    {
        red = this.transform.Find("RedLight").GetComponent<Light>();
        orange = this.transform.Find("OrangeLight").GetComponent<Light>();
        green = this.transform.Find("GreenLight").GetComponent<Light>();
        isred = true;
    }

    public bool isRed(){
        return isred;
    }
    public void setRed(){
        red.intensity = 1;
        orange.intensity = 0;
        green.intensity = 0;
        isred = true;
    }

    public void setOrange(){
        red.intensity = 0;
        orange.intensity = 1;
        green.intensity = 0;
        isred = true;
    }

    public void setGreen(){
        red.intensity = 0;
        orange.intensity = 0;
        green.intensity = 1;
        isred = false;
    }
}
