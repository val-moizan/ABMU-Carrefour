using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Utils
{
    public static Quaternion getLookingRotation(Vector3 position, Vector3 destination)
    {
        Vector3 lookPos = destination - position;
        lookPos.y = 0;
        return Quaternion.LookRotation(lookPos);
    }

    /// <summary>
    /// Ajoute une direction � un vecteur.
    /// </summary>
    /// <param name="baseVec">Vecteur de base</param>
    /// <param name="rotation">Direction a ajouter</param>
    /// <param name="distance">Distance a ajouter</param>
    /// <param name="abs">Si le vecteur doit �tre en valeurs absolues</param>
    /// <returns>Le nouveau vecteur</returns>
    public static Vector3 addRotationToVec(Vector3 baseVec, float rotation, float distance, bool abs)
    {
        float x = Mathf.Cos((rotation - 90) * Mathf.Deg2Rad) * distance;
        float z = Mathf.Sin((rotation + 90) * Mathf.Deg2Rad) * distance;
        if (abs)
        {
            x = Mathf.Abs(x);
            z = Mathf.Abs(z);
        }
        return new Vector3(baseVec.x + x, baseVec.y, baseVec.z + z);
    }

    /// <summary>
    /// Fonctions utilis� pour la d�t�ction d'objets 
    /// </summary>
    /// <param name="center">point central</param>
    /// <param name="extend">extand du box</param>
    /// <param name="except">exception dans la detection</param>
    /// <param name="agents">verif si agents pr�sent?</param>
    /// <param name="cars">verif si cars pr�sent?</param>
    /// <param name="checkMovements">verif si objets en mouvement?</param>
    /// <returns>true if detected</returns>
    public static bool detect(Vector3 center, Vector3 extend, GameObject except, bool agents, bool cars, bool checkMovements)
    {
        Collider[] hitColliders = Physics.OverlapBox(center, extend / 2, Quaternion.identity, 1);
        foreach (Collider col in hitColliders) //d�tecte les objets dans le cube
        {
            if (col.gameObject == except) continue; //detecte l'objet actuelle

            if (cars && col.transform.GetComponent<CarNavigation>() != null) //voiture d�t�ct�e
            {
                if (checkMovements)
                {
                    NavMeshAgent ag = col.GetComponent<NavMeshAgent>();
                    if (ag.velocity.x != 0 || ag.velocity.z != 0)
                    {
                        return true;
                    }
                }
                else
                {
                    return true;
                }


            }
            else if (agents && col.transform.GetComponent<AgentNavigation>() != null) //agent d�t�ct�e
            {
                if (checkMovements)
                {
                    NavMeshAgent ag = col.GetComponent<NavMeshAgent>();
                    if (ag.velocity.x != 0 || ag.velocity.z != 0)
                    {
                        return true;
                    }
                }
                else
                {
                    return true;
                }
            }
        }
        return false;
    }
    /// <summary>
    /// same as detect mais verif si la voiture ne tourne pas � droite
    /// </summary>
    /// <returns></returns>
    public static bool detectNonTurningCars(Vector3 center, Vector3 extend, GameObject except)
    {
        Collider[] hitColliders = Physics.OverlapBox(center, extend / 2, Quaternion.identity, 1);
        foreach (Collider col in hitColliders) //d�tecte les objets dans le cube
        {
            if (col.gameObject == except) continue; //detecte l'objet actuelle

            if (col.transform.GetComponent<CarNavigation>() != null) //voiture d�t�ct�e
            {
                if(!col.transform.GetComponent<CarNavigation>().turningRight && !col.transform.GetComponent<CarNavigation>().turningLeft && !col.transform.GetComponent<CarNavigation>().isStoped)
                    return true;
            }
        }
        return false;
    }
}
