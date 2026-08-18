using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BirdOrbitFlight : MovementPhysic
{
    private Rigidbody rb;
    private Vector3 currentTarget;
    private float timeOnCurrentTarget;
    private float currentBank;
    private Vector3 heading;

    // Debug/gizmo uniquement
    private Vector3 lastPlanetHitPoint;
    private bool hasPlanetHit;

    // === OPTIMISATION : Caching pour OverlapSphere ===
    private Collider[] neighbourCache = new Collider[50];
    private int neighbourCount;

    // === OPTIMISATION : Caching de la vélocité ===
    private Vector3 cachedVelocity;
    private Vector3 cachedVelocityNormalized;
    private float cachedVelocitySqrMagnitude;

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
        PickNewTarget();
    }

    void FixedUpdate()
    {
        // === OPTIMISATION : Cacher la vélocité une fois par frame ===
        cachedVelocity = rb.linearVelocity;
        cachedVelocitySqrMagnitude = cachedVelocity.sqrMagnitude;
        if (cachedVelocitySqrMagnitude > 0.01f)
            cachedVelocityNormalized = cachedVelocity.normalized;

        UpdateTargetSelection();
        UpdateHeading();
        ApplyThrust();
        ApplyAvoidance();
        UpdateVisualRotation();
    }

    void UpdateTargetSelection()
    {
        timeOnCurrentTarget += Time.fixedDeltaTime;
        
        // === OPTIMISATION : sqrMagnitude au lieu de Distance ===
        float sqrDistToTarget = (transform.position - currentTarget).sqrMagnitude;
        float sqrTargetReachedDistance = targetReachedDistance * targetReachedDistance;

        if (sqrDistToTarget < sqrTargetReachedDistance || timeOnCurrentTarget > maxTimeOnTarget)
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

    void ApplyAvoidance()
    {
        Vector3 avoidance = Vector3.zero;
        
        // === OPTIMISATION : Utiliser la vélocité cachée ===
        Vector3 castDirection = cachedVelocitySqrMagnitude > 0.01f
            ? cachedVelocityNormalized
            : heading;

        avoidance += ComputePlanetAvoidance(castDirection);
        if (avoidance.sqrMagnitude > 0.01f)
            PickNewTarget();

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

    // === OPTIMISATION : OverlapSphereNonAlloc au lieu de OverlapSphere ===
    Vector3 ComputeShipAvoidance()
    {
        Vector3 avoidance = Vector3.zero;
        
        // Utiliser l'array en cache et récupérer le count
        neighbourCount = Physics.OverlapSphereNonAlloc(
            transform.position,
            shipDetectionRadius,
            neighbourCache,
            shipLayerMask
        );

        for (int i = 0; i < neighbourCount; i++)
        {
            Collider col = neighbourCache[i];
            if (col.attachedRigidbody == rb) continue;

            Vector3 away = transform.position - col.transform.position;
            float sqrDist = away.sqrMagnitude;
            
            // === OPTIMISATION : sqrMagnitude au lieu de magnitude ===
            if (sqrDist < 0.0001f) continue;

            float shipDetectionRadiusSqr = shipDetectionRadius * shipDetectionRadius;
            float weight = 1f - Mathf.Clamp01(sqrDist / shipDetectionRadiusSqr);
            
            avoidance += (away / Mathf.Sqrt(sqrDist)) * weight;
        }

        return avoidance;
    }

    void UpdateVisualRotation()
    {
        // === OPTIMISATION : Utiliser la vélocité cachée ===
        if (cachedVelocitySqrMagnitude < 0.05f) return;

        Vector3 velocityDir = cachedVelocityNormalized;
        Quaternion lookRotation = Quaternion.LookRotation(velocityDir, Vector3.up);
        lookRotation *= Quaternion.Euler(0f, 0f, ComputeBankAngle(velocityDir));

        Quaternion newRotation = Quaternion.Slerp(
            rb.rotation,
            lookRotation,
            Time.fixedDeltaTime * rotationFollowSpeed
        );
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
