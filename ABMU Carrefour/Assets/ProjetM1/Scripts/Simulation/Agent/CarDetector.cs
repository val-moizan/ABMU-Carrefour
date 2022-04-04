using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CarDetector : MonoBehaviour
{
    /// <summary>
    /// Detect les voitures dans le cube
    /// </summary>
    public List<Transform> detectAllCars()
    {
     
        Collider[] hitColliders = Physics.OverlapBox(gameObject.transform.position, transform.localScale / 2, Quaternion.identity, 1);
        List<Transform> results = new List<Transform>();
        foreach(Collider col in hitColliders)
        {
            if(col.transform == this.transform)
            {
                continue;
            }
            if(col.transform.GetComponent<CarNavigation>() != null) //si l'object a le component carnavigation
            {
                results.Add(col.transform);
            }
        }
        return results;
    }
    /// <param name="passPos">Position du passage piéton</param>
    /// <returns>true si des voitures sont en approche</returns>
    public bool carsAreComing(Vector3 passPos)
    {
        List<Transform> list = detectAllCars(); //récupère toutes les voitures présentent dans le cube
        foreach (Transform trans in list)
        {
            Quaternion angleToPassPieton = Quaternion.LookRotation((passPos - trans.position).normalized); //angle pour regarder le passage piéton
            float angleDiff = Quaternion.Angle(angleToPassPieton, trans.rotation); //différence avec l'angle de la voiture
            if (angleDiff < 90)
            {
                return true;
            }
        }
        return false;
    }
}
