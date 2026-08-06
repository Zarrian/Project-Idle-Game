using UnityEngine;
using UnityEngine.Events;

public class Ship : MonoBehaviour
{
    public WeaponShip managerUnit;
    public UnitTierSet shipSO;

    public BirdOrbitFlight movement;

    public float damage;
    public float pv;
    public float cdAttack;
    public float rangeAttack;
    public GameObject target;

    public Pool myPool;

    private void Awake()
    {
        movement = GetComponent<BirdOrbitFlight>();

        if (myPool == null)
            myPool = transform.parent.GetComponent<Pool>();
    }

    private void OnEnable()
    {
        SetStatsCombats();
        SetMovement();
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
        pv = shipSO.tiers[managerUnit.currentTier].damage;
        cdAttack = shipSO.tiers[managerUnit.currentTier].cdAttack;
        rangeAttack = shipSO.tiers[managerUnit.currentTier].rangeAttack;
    }

    public UnityEvent OnTakeDamage;
    public void TakeDamage(float damage)
    {
        pv -= damage;

        if (pv <= 0)
        {
            Death();
        }

        OnTakeDamage?.Invoke();
    }

    public UnityEvent OnDeath;
    public void Death()
    {
        OnDeath?.Invoke();

        managerUnit.unitsList.Remove(gameObject);
        myPool.ReturnPool(gameObject);
    }
}
