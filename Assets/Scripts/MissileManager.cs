using System;
using System.Collections.Generic;
using UnityEngine;

public class MissileManager : MonoBehaviour
{

    public List<MissileHoming> activeMissiles = new List<MissileHoming>();
    List<MissileHoming> missilesToRemove = new List<MissileHoming>();

    private float detectionCheckTimer;
    public const float DETECTION_CHECK_INTERVAL = 0f;

    private void OnEnable()
    {
        MissileHoming.OnMissileCreated += HandleMissileCreated;
        MissileHoming.OnMissileDestroyed += HandleMissileDestroyed;
    }

    private void OnDisable()
    {
        MissileHoming.OnMissileCreated -= HandleMissileCreated;
        MissileHoming.OnMissileDestroyed -= HandleMissileDestroyed;
    }

    void HandleMissileCreated(MissileHoming missile)
    {
        activeMissiles.Add(missile);
    }

    private void HandleMissileDestroyed(MissileHoming missile)
    {
        missilesToRemove.Add(missile);
    }

/*    private void FixedUpdate()
    {
        detectionCheckTimer += Time.fixedDeltaTime;
        if (detectionCheckTimer < DETECTION_CHECK_INTERVAL)
            return;

        //Retire d'abord les missiles disparues
        foreach (MissileHoming missileRemoved in missilesToRemove)
        {
            activeMissiles.Remove(missileRemoved);
        }

        foreach (MissileHoming ship in activeMissiles)
        {
            ship.UpdateMissileBehavior();
        }
        detectionCheckTimer = 0f;
    }*/
}
