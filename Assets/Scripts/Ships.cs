using UnityEngine;

public class Ships : MonoBehaviour
{
    public WeaponShip manager;
    public UnitTierSet shipSO;

    public BirdOrbitFlight movement;

    public float damage;
    public float pv;
    public float cdAttack;
    public float rangeAttack;
    public GameObject target;

    private void Awake()
    {
        movement = GetComponent<BirdOrbitFlight>();
    }

    private void OnEnable()
    {
        SetStatsCombats();
        SetMovement();
    }
    public void SetMovement()
    {
        movement.orbitRadiusMin = shipSO.tiers[manager.currentTier].orbitRadiusMin;
        movement.orbitRadiusMax = shipSO.tiers[manager.currentTier].orbitRadiusMax;
        movement.thrust = shipSO.tiers[manager.currentTier].thrust;
        movement.maxSpeed = shipSO.tiers[manager.currentTier].maxSpeed;
        movement.maxTurnRateDegPerSec = shipSO.tiers[manager.currentTier].maxTurnRateDegPerSec;
        movement.maxBankAngle = shipSO.tiers[manager.currentTier].maxBankAngle;
        movement.bankLerpSpeed = shipSO.tiers[manager.currentTier].bankLerpSpeed;
        movement.rotationFollowSpeed = shipSO.tiers[manager.currentTier].rotationFollowSpeed;
        movement.targetReachedDistance = shipSO.tiers[manager.currentTier].targetReachedDistance;
        movement.maxTimeOnTarget = shipSO.tiers[manager.currentTier].maxTimeOnTarget;
        movement.planetLayerMask = shipSO.tiers[manager.currentTier].planetLayerMask;
        movement.planetDetectionDistance = shipSO.tiers[manager.currentTier].planetDetectionDistance;
        movement.planetCastRadius = shipSO.tiers[manager.currentTier].planetCastRadius;
        movement.shipLayerMask = shipSO.tiers[manager.currentTier].shipLayerMask;
        movement.shipDetectionRadius = shipSO.tiers[manager.currentTier].shipDetectionRadius;
        movement.avoidanceForce = shipSO.tiers[manager.currentTier].avoidanceForce;
    }

    public void SetStatsCombats()
    {
        damage = shipSO.tiers[manager.currentTier].damage;
        pv = shipSO.tiers[manager.currentTier].damage;
        cdAttack = shipSO.tiers[manager.currentTier].cdAttack;
        rangeAttack = shipSO.tiers[manager.currentTier].rangeAttack;
    }
}
