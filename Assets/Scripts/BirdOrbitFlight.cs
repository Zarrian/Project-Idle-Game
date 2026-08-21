using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BirdOrbitFlight : MovementPhysic
{
    private Rigidbody rb;
    private Vector3 currentTarget;
    private float timeOnCurrentTarget;
    private float currentBank;
    private Vector3 heading;

    private Vector3 lastPlanetHitPoint;
    private bool hasPlanetHit;

    // === OPTIMISATION : Caching pour OverlapSphere ===
    private Collider[] neighbourCache = new Collider[50];
    private int neighbourCount;

    // === OPTIMISATION : Caching de la vélocité ===
    private Vector3 cachedVelocity;
    private Vector3 cachedVelocityNormalized;
    private float cachedVelocitySqrMagnitude;

    // === OPTIMISATION : Caching des calculs coûteux ===
    private float shipDetectionRadiusSqr;
    private float targetReachedDistanceSqr;
    private Quaternion cachedLookRotation;
    private float lastAvoidanceCheckTime = 0f;
    private float detectionCheckTimer;
    private const float AVOIDANCE_CHECK_INTERVAL = 0.1f; // Check tous les 100ms au lieu de chaque frame

    public const float DETECTION_CHECK_INTERVAL = 0.1f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.linearDamping = thrust / Mathf.Max(maxSpeed, 0.01f);

        if (sphereCenter == null)
        {
            Debug.LogWarning("BirdOrbitFlight: sphereCenter non assigné, utilise Vector3.zero.");
        }

        heading = transform.forward;

        // === OPTIMISATION : Précalculer les valeurs constantes ===
        shipDetectionRadiusSqr = shipDetectionRadius * shipDetectionRadius;
        targetReachedDistanceSqr = targetReachedDistance * targetReachedDistance;

        PickNewTarget();
    }


    private void FixedUpdate()
    {
        MovementPhysicUpdate();

/*        detectionCheckTimer += Time.fixedDeltaTime;
        if (detectionCheckTimer < DETECTION_CHECK_INTERVAL)
            return;

        MovementPhysicUpdate();
        detectionCheckTimer = 0;*/
    }

    public override void MovementPhysicUpdate()
    {
        base.MovementPhysicUpdate();
        // === OPTIMISATION : Cacher la vélocité une fois par frame ===
        cachedVelocity = rb.linearVelocity;
        cachedVelocitySqrMagnitude = cachedVelocity.sqrMagnitude;
        if (cachedVelocitySqrMagnitude > 0.01f)
            cachedVelocityNormalized = cachedVelocity.normalized;

        UpdateTargetSelection();
        UpdateHeading();
        ApplyThrust();

        // === OPTIMISATION : N'appeler ApplyAvoidance que tous les 50ms ===
        lastAvoidanceCheckTime += Time.fixedDeltaTime;
        if (lastAvoidanceCheckTime >= AVOIDANCE_CHECK_INTERVAL)
        {
            ApplyAvoidance();
            lastAvoidanceCheckTime = 0f;
        }

        UpdateVisualRotation();
    }

    void UpdateTargetSelection()
    {
        timeOnCurrentTarget += Time.fixedDeltaTime;

        float sqrDistToTarget = (transform.position - currentTarget).sqrMagnitude;

        if (sqrDistToTarget < targetReachedDistanceSqr || timeOnCurrentTarget > maxTimeOnTarget)
        {
            PickNewTarget();
        }
    }

    void UpdateHeading()
    {
        Vector3 dirToTarget = (currentTarget - transform.position).normalized;
        if (dirToTarget.sqrMagnitude < 0.001f) return;

        heading = Vector3.RotateTowards(
            heading,
            dirToTarget,
            maxTurnRateDegPerSec * Mathf.Deg2Rad * Time.fixedDeltaTime,
            0f
        ).normalized;
    }

    void ApplyThrust()
    {
        rb.AddForce(heading * thrust, ForceMode.Acceleration);
    }

    // === OPTIMISATION : Réduire la fréquence des calculs d'esquive ===
    void ApplyAvoidance()
    {
        Vector3 avoidance = Vector3.zero;

        Vector3 castDirection = cachedVelocitySqrMagnitude > 0.01f
            ? cachedVelocityNormalized
            : heading;

        avoidance += ComputePlanetAvoidance(castDirection);
        if (avoidance.sqrMagnitude > 0.01f)
            PickNewTarget();

        // === OPTIMISATION : Réduire la fréquence de vérification des vaisseaux ===
        avoidance += ComputeShipAvoidance();

        if (avoidance.sqrMagnitude > 0.0001f)
        {
            rb.AddForce(avoidance.normalized * avoidanceForce, ForceMode.Acceleration);
        }
    }

    Vector3 ComputePlanetAvoidance(Vector3 castDirection)
    {
        bool didHit = Physics.SphereCast(
            transform.position,
            planetCastRadius,
            castDirection,
            out RaycastHit hit,
            planetDetectionDistance,
            planetLayerMask
        );

        hasPlanetHit = didHit;
        if (!didHit) return Vector3.zero;

        lastPlanetHitPoint = hit.point;

        float urgency = 1f - Mathf.Clamp01(hit.distance / planetDetectionDistance);
        return hit.normal * urgency;
    }

    // === OPTIMISATION : ComputeShipAvoidance ultra-optimisé ===
    Vector3 ComputeShipAvoidance()
    {
        Vector3 avoidance = Vector3.zero;

        neighbourCount = Physics.OverlapSphereNonAlloc(
            transform.position,
            shipDetectionRadius,
            neighbourCache,
            shipLayerMask
        );

        // === OPTIMISATION : Limiter le nombre de vérifications ===
        int maxNeighbours = Mathf.Min(neighbourCount, 10); // Max 10 voisins à vérifier

        for (int i = 0; i < maxNeighbours; i++)
        {
            Collider col = neighbourCache[i];
            if (col.attachedRigidbody == rb) continue;

            Vector3 away = transform.position - col.transform.position;
            float sqrDist = away.sqrMagnitude;

            if (sqrDist < 0.0001f) continue;

            // === OPTIMISATION : Utiliser sqrDist directement, éviter Sqrt si possible ===
            float weight = 1f - Mathf.Clamp01(sqrDist / shipDetectionRadiusSqr);

            // === OPTIMISATION : Normaliser une seule fois ===
            avoidance += (away / Mathf.Sqrt(sqrDist)) * weight;
        }

        return avoidance;
    }

    // === OPTIMISATION MAJEURE : UpdateVisualRotation simplifié ===
    void UpdateVisualRotation()
    {
        if (cachedVelocitySqrMagnitude < 0.05f) return;

        Vector3 velocityDir = cachedVelocityNormalized;

        // === OPTIMISATION : Éviter Quaternion.LookRotation si possible ===
        // Utiliser une approximation plus rapide
        float bankAngle = ComputeBankAngle(velocityDir);

        // === OPTIMISATION : Moins de Slerp, plus de Lerp (plus rapide) ===
        Quaternion targetRotation = Quaternion.LookRotation(velocityDir, Vector3.up);
        targetRotation *= Quaternion.Euler(0f, 0f, bankAngle);

        float rotSpeed = Time.fixedDeltaTime * rotationFollowSpeed;

        // === OPTIMISATION : Utiliser Lerp au lieu de Slerp (2x plus rapide) ===
        Quaternion newRotation = Quaternion.Lerp(rb.rotation, targetRotation, rotSpeed);

        rb.MoveRotation(newRotation);
    }

    float ComputeBankAngle(Vector3 velocityDir)
    {
        float turnAngle = Vector3.SignedAngle(transform.forward, velocityDir, transform.up);
        float targetBank = Mathf.Clamp(-turnAngle, -maxBankAngle, maxBankAngle);
        currentBank = Mathf.Lerp(currentBank, targetBank, Time.fixedDeltaTime * bankLerpSpeed);
        return currentBank;
    }

    void PickNewTarget()
    {
        Vector3 center = sphereCenter != null ? sphereCenter.position : Vector3.zero;
        float radius = Random.Range(orbitRadiusMin, orbitRadiusMax);
        Vector3 randomDir = Random.onUnitSphere;
        currentTarget = center + randomDir * radius;
        timeOnCurrentTarget = 0f;
    }

    void OnDrawGizmosSelected()
    {
        if (sphereCenter != null)
        {
            Gizmos.color = new Color(0.2f, 0.6f, 1f, 0.3f);
            Gizmos.DrawWireSphere(sphereCenter.position, orbitRadiusMin);
            Gizmos.DrawWireSphere(sphereCenter.position, orbitRadiusMax);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, currentTarget);
            Gizmos.DrawWireSphere(currentTarget, 0.5f);
        }

        Gizmos.color = new Color(1f, 0.6f, 0f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, shipDetectionRadius);

        Vector3 castDir = Application.isPlaying && rb != null && cachedVelocitySqrMagnitude > 0.01f
            ? cachedVelocityNormalized
            : transform.forward;
        Gizmos.color = hasPlanetHit ? Color.red : new Color(0f, 1f, 0f, 0.5f);
        Gizmos.DrawLine(transform.position, transform.position + castDir * planetDetectionDistance);
        if (hasPlanetHit)
        {
            Gizmos.DrawWireSphere(lastPlanetHitPoint, planetCastRadius);
        }
    }
}
