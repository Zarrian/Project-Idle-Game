using System.Collections.Generic;
using UnityEngine;

public class SpawnerEnemy : WeaponShip
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

        // On décale temporairement spawnPoint, le temps de l'appel à
        // base.CreateShip() (qui spawn à spawnPoint.position), puis on le
        // remet à sa place — spawnPoint garde son rôle de référence stable
        // entre deux spawns.
        Vector3 originalSpawnPosition = transform.position;
        transform.position = spawnPosition;

        base.CreateShip();

        //transform.position = originalSpawnPosition;
    }
}