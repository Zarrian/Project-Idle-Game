using System;
using UnityEngine;

public interface IDamageable
{
    public abstract void TakeDamage(float damage, Vector3 pos);

    public abstract void Death();
}
