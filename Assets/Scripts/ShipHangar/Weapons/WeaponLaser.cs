using UnityEngine;

public class WeaponLaser : Weapon
{
    public Pool myPool;

    public override void Attack(Transform ship, Transform target, float damage)
    {
        base.Attack(ship, target, damage);

        myPool.GetPoolObject().GetComponent<Laser>().ActiveLaser(ship, target, damage);
    }

}
