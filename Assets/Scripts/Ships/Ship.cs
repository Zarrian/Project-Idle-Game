using System;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class Ship : MonoBehaviour, IDamageable
{

    public static Action<Ship> OnShipCreated;
    public static Action<Ship> OnShipDestroyed;
    public static Action<Ship, float> OnShipTakeDamage;

    public HangarShip managerUnit;
    public UnitTier shipSO;
    public ShipUpgradeData shipUpgradeData;

    public MovementPhysic movement;

    public float damage;
    public float pv;
    public float cdAttack;
    public float rangeAttack;
    public float nbAttack;
    public GameObject target;

    public Pool myPool;

    /// <summary>Stocke le niveau actuel d'amélioration pour chaque stat</summary>
    private Dictionary<string, int> currentUpgradeLevels = new Dictionary<string, int>();

    private void Awake()
    {
        if (myPool == null)
            myPool = transform.parent.GetComponent<Pool>();
    }

    private void OnEnable()
    {
        SetStatsCombats();
        SetMovement();

        OnShipCreated?.Invoke(this);
    }

    public void SetMovement()
    {
        movement.orbitRadiusMin = shipSO.orbitRadiusMin;
        movement.orbitRadiusMax = shipSO.orbitRadiusMax;
        movement.thrust = shipSO.thrust;
        movement.maxSpeed = shipSO.maxSpeed;
        movement.maxTurnRateDegPerSec = shipSO.maxTurnRateDegPerSec;
        movement.maxBankAngle = shipSO.maxBankAngle;
        movement.bankLerpSpeed = shipSO.bankLerpSpeed;
        movement.rotationFollowSpeed = shipSO.rotationFollowSpeed;
        movement.targetReachedDistance = shipSO.targetReachedDistance;
        movement.maxTimeOnTarget = shipSO.maxTimeOnTarget;
        movement.planetLayerMask = shipSO.planetLayerMask;
        movement.planetDetectionDistance = shipSO.planetDetectionDistance;
        movement.planetCastRadius = shipSO.planetCastRadius;
        movement.shipLayerMask = shipSO.shipLayerMask;
        movement.shipDetectionRadius = shipSO.shipDetectionRadius;
        movement.avoidanceForce = shipSO.avoidanceForce;
    }

    public void SetStatsCombats()
    {
        damage = shipSO.damage;
        pv = shipSO.pv;
        cdAttack = shipSO.cdAttack;
        rangeAttack = shipSO.rangeAttack;
        nbAttack = shipSO.nbAttack;
    }

    /// <summary>Récupère le niveau actuel d'une stat</summary>
    public int GetCurrentStatLevel(string statName)
    {
        return currentUpgradeLevels.TryGetValue(statName, out var level) ? level : 1;
    }


    /// <summary>Applique un niveau d'amélioration à une stat et réapplique les stats en Runtime</summary>
    public void ApplyUpgradeLevel(string statName, int level)
    {
        if (shipUpgradeData == null)
        {
            Debug.LogError("ShipUpgradeData manquant !", gameObject);
            return;
        }

        var step = shipUpgradeData.GetStep(statName, level);
        if (step == null)
        {
            Debug.LogWarning($"Niveau {level} introuvable pour la stat '{statName}'", gameObject);
            return;
        }

        currentUpgradeLevels[statName] = level;
        ApplyStatUpgrade(statName, step.value);
    }

    /// <summary>Applique une valeur d'amélioration à un champ spécifique</summary>
    private void ApplyStatUpgrade(string statName, float value)
    {
        switch (statName.ToLower())
        {
            case "pv":
            case "hp":
                pv = value;
                break;
            case "damage":
                damage = value;
                break;
            case "cdattack":
            case "cd attack":
                cdAttack = value;
                break;
            case "rangeattack":
            case "range attack":
            case "range":
                rangeAttack = value;
                break;
            case "nbattack":
            case "nb attack":
            case "attack":
                nbAttack = (int)value;
                break;
            case "thrust":
                shipSO.thrust = value;
                SetMovement();
                break;
            case "maxspeed":
            case "max speed":
            case "speed":
                shipSO.maxSpeed = value;
                SetMovement();
                break;
            case "maxturnratedegpersec":
            case "max turn rate":
                shipSO.maxTurnRateDegPerSec = value;
                SetMovement();
                break;
            case "maxbankangle":
            case "max bank angle":
                shipSO.maxBankAngle = value;
                SetMovement();
                break;
            case "banklersspeed":
            case "bank lerp speed":
                shipSO.bankLerpSpeed = value;
                SetMovement();
                break;
            case "rotationfollowspeed":
            case "rotation follow speed":
                shipSO.rotationFollowSpeed = value;
                SetMovement();
                break;
        }
    }



    public UnityEvent<Vector3, float> OnTakeDamage;
    public void TakeDamage(float damage, Vector3 pos)
    {
        if (gameObject.activeSelf == false)
            return;
        
        OnShipTakeDamage?.Invoke(this, damage);
        OnTakeDamage?.Invoke(pos, damage);

        float pvBefore = pv;
        pv -= damage;

        if (pv <= 0)
        {
            Death();
        }
    }

    public UnityEvent OnDeath;
    public void Death()
    {
        managerUnit.RemoveShip(gameObject);
        OnDeath?.Invoke();
        OnShipDestroyed?.Invoke(this);
    }

    public void SetPosition(Vector3 pos)
    {
        transform.position = pos;
    }
}

