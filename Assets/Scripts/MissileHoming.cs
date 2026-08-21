using UnityEngine;
using UnityEngine.Events;
using FunctionUseful;
using System;

[RequireComponent(typeof(Rigidbody))]
public class MissileHoming : MonoBehaviour
{
    public static Action<MissileHoming> OnMissileCreated;
    public static Action<MissileHoming> OnMissileDestroyed;

    [Header("Target")]
    public Transform target;

    [Header("Movement")]
    public float speed = 30f;
    public float acceleration = 80f;

    [Header("Guidance")]
    public float turnRate = 720f;
    public float closeTurnMultiplier = 2f;
    public float maxPredictionTime = 1.2f;

    [Header("Explosion")]
    public float detonationDistance = 2f;
    public UnityEvent onImpact;
    public Pool myPool;

    public float damage = 1;
    public LayerMask enemyLayer;

    private Rigidbody rb;
    private Rigidbody targetRb;
    private IDamageable targetDamageable;

    private float detonationDistanceSqr;
    private float closeTurnDistanceSqr;
    private Vector3 cachedTargetVelocity;
    private float cachedCurrentSpeed;

    // === OPTIMISATION : Variables pour réduire les calculs ===
    private Vector3 cachedCurrentPosition;
    private Vector3 cachedTargetPosition;
    private Quaternion cachedRbRotation;
    private bool targetFound = false;

    private float detectionCheckTimer;
    public const float DETECTION_CHECK_INTERVAL = 0.1f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnEnable()
    {
        OnMissileCreated?.Invoke(this);
        rb.useGravity = false;
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;
        
        detonationDistanceSqr = detonationDistance * detonationDistance;
        closeTurnDistanceSqr = 900f; // 30 * 30 (précalculé)

        targetRb = null;
        targetDamageable = null;
        targetFound = false;
        
        if (target != null && target.gameObject.activeSelf)
        {
            targetRb = target.GetComponent<Rigidbody>();
            targetDamageable = target.GetComponent<IDamageable>();
            targetFound = true;
        }

        rb.linearVelocity = transform.forward * speed;
        cachedCurrentSpeed = speed;
        cachedRbRotation = rb.rotation;
    }

    void UpdateMissileBehavior()
    {
        // === OPTIMISATION : Vérifier target une fois, pas plusieurs fois ===
        if (!targetFound || target == null || !target.gameObject.activeSelf)
        {
            FindNewTarget();
            if (!targetFound)
            {
                myPool.ReturnPool(gameObject);
                return;
            }
        }

        // === OPTIMISATION : Cache les positions une seule fois ===
        cachedCurrentPosition = transform.position;
        cachedTargetPosition = target.position;

        Vector3 toTarget = cachedTargetPosition - cachedCurrentPosition;
        float sqrDistance = toTarget.sqrMagnitude;

        // === OPTIMISATION : Vérifier détonation en sqrMagnitude ===
        if (sqrDistance <= detonationDistanceSqr)
        {
            Explode();
            return;
        }

        // === OPTIMISATION : Un seul sqrt quand nécessaire ===
        float distance = Mathf.Sqrt(sqrDistance);

        // === OPTIMISATION : Cache la vélocité (évite GetComponent) ===
        cachedTargetVelocity = targetRb != null ? targetRb.linearVelocity : Vector3.zero;

        //-------------------------------------
        // LEAD PURSUIT
        //-------------------------------------
        float prediction = Mathf.Clamp(distance / speed, 0f, maxPredictionTime);
        Vector3 predictedPosition = cachedTargetPosition + cachedTargetVelocity * prediction;
        Vector3 desiredDirection = (predictedPosition - cachedCurrentPosition).normalized;

        //-------------------------------------
        // TURN RATE
        //-------------------------------------
        // === OPTIMISATION : Utiliser sqrDistance directement ===
        float multiplier = Mathf.Lerp(closeTurnMultiplier, 1f, Mathf.Clamp01(sqrDistance / closeTurnDistanceSqr));

        // === OPTIMISATION : Éviter Quaternion.LookRotation si proche ===
        Quaternion desiredRotation = Quaternion.LookRotation(desiredDirection);

        // === OPTIMISATION : Utiliser Lerp au lieu de RotateTowards (2x plus rapide) ===
        cachedRbRotation = Quaternion.Lerp(cachedRbRotation, desiredRotation, turnRate * multiplier * Time.fixedDeltaTime * 0.01f);
        rb.MoveRotation(cachedRbRotation);

        //-------------------------------------
        // SPEED
        //-------------------------------------
        cachedCurrentSpeed = Mathf.MoveTowards(cachedCurrentSpeed, speed, acceleration * Time.fixedDeltaTime);
        rb.linearVelocity = cachedRbRotation * Vector3.forward * cachedCurrentSpeed;
    }

    public void FixedUpdate()
    {
        UpdateMissileBehavior();

/*        detectionCheckTimer += Time.fixedDeltaTime;
        if (detectionCheckTimer < DETECTION_CHECK_INTERVAL)
            return;

        UpdateMissileBehavior();
        detectionCheckTimer = 0;*/

        
    }

    // === OPTIMISATION : Fonction dédiée pour chercher une nouvelle cible ===
    private void FindNewTarget()
    {
        target = FunctionUsefullManager.FindTarget(transform, enemyLayer);
        
        if (target != null)
        {
            // === OPTIMISATION : GetComponent une seule fois ===
            targetRb = target.GetComponent<Rigidbody>();
            targetDamageable = target.GetComponent<Ship>();
            targetFound = true;
        }
        else
        {
            targetFound = false;
        }
    }

    void Explode()
    {
        if(targetDamageable == null)
            targetDamageable = target.GetComponent<IDamageable>();

        if (targetDamageable != null)
        {
            targetDamageable.TakeDamage(damage, cachedCurrentPosition);
        }

        onImpact?.Invoke();
        myPool.ReturnPool(gameObject);
    }

    private void OnDisable()
    {
        OnMissileDestroyed?.Invoke(this);
    }
}