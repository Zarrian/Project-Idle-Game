using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerStatistiques : MonoBehaviour
{
    public static ManagerStatistiques instance;

    public List<Ship> allships;
    public List<Ship> shipPlayer;
    public List<Ship> shipInvaders;

    public LayerMask playerShipLayerMask;
    public LayerMask invaderLayerMask;

    public float playerDamageLast10Seconds;
    public float invaderDamageLast10Seconds;

    public float playerDPS;
    public float invaderDPS;

    public float playerCurrentPV;
    public float invaderCurrentPV;


    private void Awake()
    {
        instance = this;
    }

    private void OnEnable()
    {
        Ship.OnShipCreated += HandleShipCreated;
        Ship.OnShipTakeDamage += HandleShipTakeDamage;
        Ship.OnShipDestroyed += HandleShipDestroyed;

        StartCoroutine(UpdateDamage());
    }

    private void OnDisable()
    {
        Ship.OnShipCreated -= HandleShipCreated;
        Ship.OnShipTakeDamage -= HandleShipTakeDamage;
        Ship.OnShipDestroyed -= HandleShipDestroyed;
    }

    public IEnumerator UpdateDamage()
    {
        playerDamageLast10Seconds = GetPlayerDamageLast10Seconds();
        invaderDamageLast10Seconds = GetInvaderDamageLast10Seconds();
        playerDPS = GetPlayerDPS(); invaderDPS = GetInvaderDPS();
        yield return new WaitForSeconds(0.2f);
        StartCoroutine(UpdateDamage());
    }

    private void HandleShipCreated(Ship ship)
    {
        allships.Add(ship);

        if ((playerShipLayerMask.value & (1 << ship.gameObject.layer)) != 0)
        {
            shipPlayer.Add(ship);
            playerCurrentPV += ship.pv;
        }
        else if ((invaderLayerMask.value & (1 << ship.gameObject.layer)) != 0)
        {
            shipInvaders.Add(ship);
            invaderCurrentPV += ship.pv;
        }
    }


    private void HandleShipTakeDamage(Ship ship, float damage)
    {
        if ((playerShipLayerMask.value & (1 << ship.gameObject.layer)) != 0)
        {
            playerDamageInstances.Add(new DamageInstance(damage));
            playerCurrentPV -= damage;
        }
        else if ((invaderLayerMask.value & (1 << ship.gameObject.layer)) != 0)
        {
            invaderDamageInstances.Add(new DamageInstance(damage));
            invaderCurrentPV -= damage;
        }
    }


    private void HandleShipDestroyed(Ship ship)
    {
        // Retirer le Ship des listes
        allships.Remove(ship);
        shipPlayer.Remove(ship);
        shipInvaders.Remove(ship);

        // Retirer ses PV restants
        // (utile si le Ship est détruit alors qu'il lui reste des PV)
        if ((playerShipLayerMask.value & (1 << ship.gameObject.layer)) != 0)
        {
            playerCurrentPV -= ship.pv;
        }
        else if ((invaderLayerMask.value & (1 << ship.gameObject.layer)) != 0)
        {
            invaderCurrentPV -= ship.pv;
        }
    }


    // =========================
    // DAMAGE / DPS
    // =========================

    private struct DamageInstance
    {
        public float damage;
        public float time;

        public DamageInstance(float damage)
        {
            this.damage = damage;
            time = Time.time;
        }
    }

    private List<DamageInstance> playerDamageInstances = new();
    private List<DamageInstance> invaderDamageInstances = new();


    public float GetPlayerDamageLast10Seconds()
    {
        return GetDamageLast10Seconds(playerDamageInstances);
    }

    public float GetInvaderDamageLast10Seconds()
    {
        return GetDamageLast10Seconds(invaderDamageInstances);
    }


    private float GetDamageLast10Seconds(List<DamageInstance> damageInstances)
    {
        float totalDamage = 0f;
        float cutoff = Time.time - 10f;

        for (int i = damageInstances.Count - 1; i >= 0; i--)
        {
            if (damageInstances[i].time < cutoff)
            {
                damageInstances.RemoveAt(i);
            }
            else
            {
                totalDamage += damageInstances[i].damage;
            }
        }

        return totalDamage;
    }


    public float GetPlayerDPS()
    {
        return GetPlayerDamageLast10Seconds() / 10f;
    }

    public float GetInvaderDPS()
    {
        return GetInvaderDamageLast10Seconds() / 10f;
    }
}