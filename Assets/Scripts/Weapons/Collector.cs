using UnityEngine;

public class Collector : WeaponShip
{
    public override void FixedUpdate()
    {
        //base.FixedUpdate();

        SpawnUnits();
    }

    public void FindNearestScrap()
    {

    }
}
