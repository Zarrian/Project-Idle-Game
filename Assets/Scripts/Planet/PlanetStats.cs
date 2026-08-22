using UnityEngine;
using FunctionUseful;
using System;
using System.Collections;

public class PlanetStats : MonoBehaviour, IDamageable
{
    public PlanetSO planetSO;

    public float hpMax;
    public float hp;
    public float hpRegen;

    public Action OnDeath;
    public void Death()
    {
        Time.timeScale = 0;

        OnDeath?.Invoke();
    }

    public Action OnTakeDamage;
    public void TakeDamage(float damage, Vector3 pos)
    {
        hp -= damage;

        //FX de shockWave avec un bouclier énergétique
        OnTakeDamage?.Invoke();

        if (hp <= 0)
            Death();

    }

    private void Awake()
    {
        hpMax = planetSO.HP;
        hp = planetSO.HP;
        hpRegen = planetSO.HpRegen;

        StartCoroutine(RegenPVConstante(1, hpRegen));
    }

    public Action OnRegenPV;
    public IEnumerator RegenPVConstante(float delay, float amountHpRegen)
    {
        yield return new WaitForSeconds(delay);

        hp += amountHpRegen;
        hp = Math.Clamp(hp, 0, hpMax);

        OnRegenPV?.Invoke();
        StartCoroutine(RegenPVConstante(1, hpRegen));
    }

    public void RegenPV(float delay, float amountHpRegen)
    {
        hp += amountHpRegen;
        hp = Math.Clamp(hp, 0, hpMax);

        OnRegenPV?.Invoke();
    }
}
