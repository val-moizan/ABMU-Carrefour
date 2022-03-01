using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeuScript : MonoBehaviour
{

    Light red, orange, green;
    // Start is called before the first frame update
    void Start()
    {
        red = this.transform.Find("RedLight").GetComponent<Light>();
        orange = this.transform.Find("OrangeLight").GetComponent<Light>();
        green = this.transform.Find("GreenLight").GetComponent<Light>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator setRed(){
        setOrange();
        Debug.Log("Orange");
        yield return new WaitForSeconds(1);
        red.intensity = 1;
        orange.intensity = 0;
        green.intensity = 0;
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
    }
}
