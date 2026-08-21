using UnityEngine;

public class WeaponMissile : Weapon
{
    public Pool myPool;
    public override void Attack(Transform ship, Transform target, float damage)
    {
        base.Attack(ship, target, damage);
        GameObject missile = myPool.GetPoolObject();
        MissileHoming missileHoming = missile.GetComponent<MissileHoming>();
        missileHoming.target = target;
        missileHoming.damage = damage;

        missile.transform.position = ship.transform.position;
        missile.transform.rotation = ship.transform.rotation;
    }
}
