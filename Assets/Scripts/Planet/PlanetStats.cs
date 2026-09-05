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

    public Action<Vector3, float> OnTakeDamage;
    public void TakeDamage(float damage, Vector3 pos)
    {
        hp -= damage;

        //FX de shockWave avec un bouclier �nerg�tique
        OnTakeDamage?.Invoke(pos, damage);

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
        // while(true) au lieu de se relancer soi-m�me via StartCoroutine :
        // une seule coroutine vit ici au lieu d'en recr�er une nouvelle �
        // chaque tick.
        while (true)
        {
            yield return new WaitForSeconds(delay);

            hp += amountHpRegen;
            hp = Math.Clamp(hp, 0, hpMax);

            OnRegenPV?.Invoke();
        }
    }

    public void RegenPV(float delay, float amountHpRegen)
    {
        hp += amountHpRegen;
        hp = Math.Clamp(hp, 0, hpMax);

        OnRegenPV?.Invoke();
    }
}
