using System;
using UnityEngine;
using UnityEngine.Events;

public class Ship : MonoBehaviour
{

    public static Action<Ship> OnShipCreated;
    public static Action<Ship> OnShipDestroyed;
    public static Action<Ship, float> OnShipTakeDamage;

    public WeaponShip managerUnit;
    public UnitTierSet shipSO;

    public MovementPhysic movement;

    public float damage;
    public float pv;
    public float cdAttack;
    public float rangeAttack;
    public float nbAttack;
    public GameObject target;

    public Pool myPool;

    private void Awake()
    {
        //movement = GetComponent<BirdOrbitFlight>();

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
        movement.orbitRadiusMin = shipSO.tiers[managerUnit.currentTier].orbitRadiusMin;
        movement.orbitRadiusMax = shipSO.tiers[managerUnit.currentTier].orbitRadiusMax;
        movement.thrust = shipSO.tiers[managerUnit.currentTier].thrust;
        movement.maxSpeed = shipSO.tiers[managerUnit.currentTier].maxSpeed;
        movement.maxTurnRateDegPerSec = shipSO.tiers[managerUnit.currentTier].maxTurnRateDegPerSec;
        movement.maxBankAngle = shipSO.tiers[managerUnit.currentTier].maxBankAngle;
        movement.bankLerpSpeed = shipSO.tiers[managerUnit.currentTier].bankLerpSpeed;
        movement.rotationFollowSpeed = shipSO.tiers[managerUnit.currentTier].rotationFollowSpeed;
        movement.targetReachedDistance = shipSO.tiers[managerUnit.currentTier].targetReachedDistance;
        movement.maxTimeOnTarget = shipSO.tiers[managerUnit.currentTier].maxTimeOnTarget;
        movement.planetLayerMask = shipSO.tiers[managerUnit.currentTier].planetLayerMask;
        movement.planetDetectionDistance = shipSO.tiers[managerUnit.currentTier].planetDetectionDistance;
        movement.planetCastRadius = shipSO.tiers[managerUnit.currentTier].planetCastRadius;
        movement.shipLayerMask = shipSO.tiers[managerUnit.currentTier].shipLayerMask;
        movement.shipDetectionRadius = shipSO.tiers[managerUnit.currentTier].shipDetectionRadius;
        movement.avoidanceForce = shipSO.tiers[managerUnit.currentTier].avoidanceForce;
    }

    public void SetStatsCombats()
    {
        damage = shipSO.tiers[managerUnit.currentTier].damage;
        pv = shipSO.tiers[managerUnit.currentTier].pv;
        cdAttack = shipSO.tiers[managerUnit.currentTier].cdAttack;
        rangeAttack = shipSO.tiers[managerUnit.currentTier].rangeAttack;
        nbAttack = shipSO.tiers[managerUnit.currentTier].nbAttack;
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
