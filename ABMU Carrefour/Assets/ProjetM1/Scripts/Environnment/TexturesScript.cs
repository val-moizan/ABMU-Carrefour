using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TexturesScript : MonoBehaviour{
    public float diviseur = 10;
    public bool carre = false;
    void Start(){
        Renderer rend = GetComponent<Renderer>();
        Vector3 bounds = GetComponent<Collider>().bounds.size;
        Transform transform = gameObject.GetComponent<Transform>();
        float sizeX = bounds.x;
        float sizeZ = bounds.z;
        float angle = (transform.eulerAngles.y + 360) % 360;
        if (angle > 180)  
            angle -= 360;
    
        if(Mathf.Abs(angle) >= 45 && Mathf.Abs(angle) <= 135){//direction Z
            float temp = sizeX;
            sizeX = sizeZ;
            sizeZ = temp;
        }
        if(sizeX > sizeZ){ //aply good texture scale to material

            rend.material.mainTextureScale = new Vector2(sizeX/diviseur, carre ? sizeZ/diviseur : 1);
            
        }else{
            rend.material.mainTextureScale = new Vector2(carre ? sizeX/diviseur : 1, sizeZ/diviseur);
        }
    }
}
