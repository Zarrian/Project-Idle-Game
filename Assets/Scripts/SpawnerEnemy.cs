using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerEnemy : HangarShip
{
    [Header("Zone de spawn")]
    [Tooltip("Centre de référence de la zone de spawn (ex: le centre de la planète)")]
    public Transform sphereCenter;

    [Tooltip("Distance minimale du centre à laquelle un ennemi peut spawn")]
    public float minSpawnRadius = 2500f;

    [Tooltip("Distance maximale du centre à laquelle un ennemi peut spawn")]
    public float maxSpawnRadius = 4000f;


    public override void CreateShip()
    {
        // Random.onUnitSphere donne une direction aléatoire uniforme sur
        // n'importe quel axe (pas juste horizontal). On la combine avec une
        // distance tirée entre min et max pour obtenir un point n'importe
        // où sur une coquille sphérique autour de sphereCenter.
        Vector3 randomDirection = Random.onUnitSphere;
        float randomDistance = Random.Range(minSpawnRadius, maxSpawnRadius);
        Vector3 spawnPosition = sphereCenter.position + randomDirection * randomDistance;

        GameObject newShip = myPoolShips.GetPoolObject();
        newShip.transform.position = spawnPosition;
        newShip.transform.rotation = transform.rotation;
        unitsList.Add(newShip);
        movements.Add(newShip.GetComponent<MovementPhysic>());
        isCreatingShip = false;

        //StartCoroutine(Replacement(newShip.transform, spawnPosition));
    }

    public IEnumerator Replacement(Transform NewShip, Vector3 spawnPosition)
    {
        yield return new WaitForSeconds(0.1f);

        NewShip.GetComponent<Ship>().SetPosition(spawnPosition);
    }

    private void OnDrawGizmos()
    {
        if (sphereCenter == null)
            return;

        // Dessiner la sphère min (rouge)
        Gizmos.color = Color.red;
        DrawWireSphere(sphereCenter.position, minSpawnRadius, 16);

        // Dessiner la sphère max (vert)
        Gizmos.color = Color.green;
        DrawWireSphere(sphereCenter.position, maxSpawnRadius, 16);
    }

    private void DrawWireSphere(Vector3 center, float radius, int segments)
    {
        float segmentAngle = 360f / segments;
        Vector3 prevPoint = center + new Vector3(radius, 0, 0);

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * segmentAngle * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(
                Mathf.Cos(angle) * radius,
                0,
                Mathf.Sin(angle) * radius
            );
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }
}