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
        //tri des feux en fonction de leur distance par rapport au passage piéton
        array = array.OrderBy(o => Vector3.Distance(this.transform.position, o.gameObject.transform.position)).ToList();
        return array.Count == 0 ? null : array[0];
    }

    /// <returns>true si le feu est vert pour les piétons</returns>
    public bool isOpen()
    {
        return !feu.isWalkLightRed();
    }

    /// <returns>true si il y a des piétons sur le passage piéton</returns>
    public bool containsPedestrians()
    {
        Vector3 checkPos = this.transform.position;
        checkPos.y += 1.5f;
        Collider[] hitColliders = Physics.OverlapBox(checkPos, this.transform.Find("BaseCube").localScale / 2, this.transform.Find("BaseCube").rotation, 1);
        foreach (Collider col in hitColliders) //détecte les objets au dessus du passage piéton
        {
            if (col.transform.GetComponent<AgentNavigation>() != null) //agent détéctée
            {
                return true;
            }
        }

        return false;
    }
    /// <returns>true si il y a des voitures sur le passage piéton</returns>
    public bool containsCars()
    {
        Vector3 checkPos = this.transform.position;
        checkPos.y += 1f;
        Collider[] hitColliders = Physics.OverlapBox(checkPos, this.transform.Find("BaseCube").localScale / 2, this.transform.Find("BaseCube").rotation, 1);
        foreach (Collider col in hitColliders) //détecte les objets au dessus du passage piéton
        {
            if (col.transform.GetComponent<CarNavigation>() != null) //car détéctée
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
      //  removeWaitingPedestrian(pedestrian); //évite les doublette
        waitingPedestrians.Add(pedestrian);
     //   Debug.Log(this.name + " : " + waitingPedestrians.Count);
    }

    public IEnumerator removeWaitingPedestrian(GameObject pedestrian)
    {
        yield return new WaitForSeconds(2); //attend 2s avant de retirer le piéton (le temps qu'il commence à marcher sur le passage piéton)
        waitingPedestrians.Remove(pedestrian);
       // Debug.Log(this.name + " : " + waitingPedestrians.Count);
    }
}
