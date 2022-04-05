using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class PassScript : MonoBehaviour
{
    private FeuScript feu;
    private List<GameObject> waitingPedestrians = new List<GameObject>();
    void Start()
    {
        this.feu = getNearestLight();
    }
    private void Update()
    {
    }
    public FeuScript getLight()
    {
        return this.feu;
    }
    private FeuScript getNearestLight()
    {
        FeuScript[] list = FindObjectsOfType<FeuScript>();
        List<FeuScript> array = new List<FeuScript>(list);
        //tri des feux en fonction de leur distance par rapport au passage pi�ton
        array = array.OrderBy(o => Vector3.Distance(this.transform.position, o.gameObject.transform.position)).ToList();
        return array.Count == 0 ? null : array[0];
    }

    /// <returns>true si le feu est vert pour les pi�tons</returns>
    public bool isOpen()
    {
        return !feu.isWalkLightRed();
    }

    /// <returns>true si il y a des pi�tons sur le passage pi�ton</returns>
    public bool containsPedestrians()
    {
        Vector3 checkPos = this.transform.position;
        checkPos.y += 1.5f;
        Collider[] hitColliders = Physics.OverlapBox(checkPos, this.transform.Find("BaseCube").localScale / 2, this.transform.Find("BaseCube").rotation, 1);
        foreach (Collider col in hitColliders) //d�tecte les objets au dessus du passage pi�ton
        {
            if (col.transform.GetComponent<AgentNavigation>() != null) //agent d�t�ct�e
            {
                return true;
            }
        }

        return false;
    }
    /// <returns>true si il y a des voitures sur le passage pi�ton</returns>
    public bool containsCars()
    {
        Vector3 checkPos = this.transform.position;
        checkPos.y += 1f;
        Collider[] hitColliders = Physics.OverlapBox(checkPos, this.transform.Find("BaseCube").localScale / 2, this.transform.Find("BaseCube").rotation, 1);
        foreach (Collider col in hitColliders) //d�tecte les objets au dessus du passage pi�ton
        {
            if (col.transform.GetComponent<CarNavigation>() != null) //car d�t�ct�e
            {
                return true;
            }
        }

        return false;
    }

    public bool hasWaitingPedestrians()
    {
        return waitingPedestrians.Count > 0;
    }
    public void addWaitingPedestrian(GameObject pedestrian)
    {
      //  removeWaitingPedestrian(pedestrian); //�vite les doublette
        waitingPedestrians.Add(pedestrian);
     //   Debug.Log(this.name + " : " + waitingPedestrians.Count);
    }

    public IEnumerator removeWaitingPedestrian(GameObject pedestrian)
    {
        yield return new WaitForSeconds(2); //attend 2s avant de retirer le pi�ton (le temps qu'il commence � marcher sur le passage pi�ton)
        waitingPedestrians.Remove(pedestrian);
       // Debug.Log(this.name + " : " + waitingPedestrians.Count);
    }
}
