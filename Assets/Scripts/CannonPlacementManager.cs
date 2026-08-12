using System.Collections.Generic;
using UnityEngine;

public class CannonPlacementManager : MonoBehaviour
{
    public Pool myPool;
    [SerializeField] private Transform deathStar;
    //[SerializeField] public int cannonCount = 12;
    [SerializeField] private float distanceFromSurface = 1f;

    private Transform cannonsParent;

    private void Start()
    {
        if (deathStar == null)
        {
            deathStar = GameObject.Find("DeathStar").transform;
        }
    }

    public List<GameObject> CreateCannonsGrid(int cannonCount, GameObject cannonPrefab)
    {
        List<GameObject> cannons = new List<GameObject>();
        // Créer le parent "Cannons"
        GameObject cannonsContainer = new GameObject("Cannons");
        cannonsContainer.transform.SetParent(deathStar);
        cannonsContainer.transform.localPosition = Vector3.zero;
        cannonsContainer.transform.localRotation = Quaternion.identity;
        cannonsParent = cannonsContainer.transform;

        float planetRadius = deathStar.localScale.x / 2f;
        float distanceTotal = planetRadius + distanceFromSurface;

        // Distribution en grille sphérique (Fibonacci sphere)
        for (int i = 0; i < cannonCount; i++)
        {
            float phi = Mathf.Acos(1f - 2f * i / cannonCount);
            float theta = Mathf.Sqrt(cannonCount * Mathf.PI) * phi;

            Vector3 position = new Vector3(
                distanceTotal * Mathf.Sin(phi) * Mathf.Cos(theta),
                distanceTotal * Mathf.Cos(phi),
                distanceTotal * Mathf.Sin(phi) * Mathf.Sin(theta)
            );

            // Instancier le canon
            GameObject cannon = myPool.GetPoolObject();
            cannon.name = "Cannon_" + i;
            cannon.transform.localPosition = position;

            // Orienter vers l'extérieur (direction radiale)
            Vector3 outwardDirection = position.normalized;
            cannon.transform.localRotation = Quaternion.FromToRotation(Vector3.up, outwardDirection);

            cannons.Add(cannon);
        }

        return cannons;
    }


}