using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Vaisseau "Collector" : va d'un point A à sa cible (target) en ligne quasi
/// droite (contrairement à BirdOrbitFlight, le taux de virage est très
/// élevé, donc presque aucune courbe visible sauf pour esquiver). Une fois
/// à portée de la cible, déclenche onArrival (l'action réelle sera branchée
/// plus tard). Reprend le même système d'évitement planète/vaisseaux que
/// BirdOrbitFlight. Pesanteur nulle : on est dans l'espace.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class CollectorShip : MovementPhysic
{
    [Header("Cible")]
    public Transform target;


    [Header("Guidage — quasi-linéaire")]
    [Tooltip("Taux de virage très élevé pour une trajectoire presque droite. Ne baisse cette valeur que si tu veux réintroduire de la courbe.")]
    public float turnRateDegPerSec = 400f;

    [Header("Arrivée")]
    [Tooltip("Distance à la cible en dessous de laquelle le Collector est considéré arrivé")]
    public float arrivalDistance = 3f;

    [Tooltip("Déclenché à l'arrivée sur la cible — branche ici l'action réelle plus tard (collecte, docking, etc.)")]
    public UnityEvent onArrival;

    [Header("Ralentissement à l'approche")]
    [Tooltip("Distance à partir de laquelle le Collector commence à ralentir en approchant de sa cible")]
    public float decelerationDistance = 15f;

    [Tooltip("Fraction minimale de la poussée conservée juste avant l'arrivée (évite un arrêt trop brutal/instable)")]
    [Range(0.05f, 1f)]
    public float minThrustFactor = 0.15f;

    [Tooltip("Force de freinage active, appliquée à l'opposé de la vélocité en approchant. 0 = freinage passif uniquement (juste la baisse de poussée). Monte cette valeur pour un freinage plus marqué et plus rapide.")]
    public float brakingForce = 15f;

    private Rigidbody rb;
    private Vector3 heading;
    private bool hasArrived;

    // Debug/gizmo uniquement.
    private Vector3 lastPlanetHitPoint;
    private bool hasPlanetHit;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false; // dans l'espace, pas de gravité
        rb.linearDamping = thrust / Mathf.Max(maxSpeed, 0.01f);

        heading = transform.forward;

        if (target == null)
        {
            Debug.LogWarning("CollectorShip: aucune target assignée.");
        }
    }

    void FixedUpdate()
    {
        if (hasArrived || target == null) return;

        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        if (distanceToTarget <= arrivalDistance)
        {
            Arrive();
            return;
        }

        UpdateHeading();
        ApplyThrust(distanceToTarget);
        ApplyBraking(distanceToTarget);
        ApplyAvoidance();
        UpdateVisualRotation();
    }

    void Arrive()
    {
        hasArrived = true;
        rb.linearVelocity = Vector3.zero;
        onArrival?.Invoke();
    }

    /// <summary>
    /// Courbe le cap vers la cible à vitesse angulaire plafonnée — le
    /// plafond est ici volontairement très haut, donc la trajectoire reste
    /// quasiment rectiligne en conditions normales.
    /// </summary>
    void UpdateHeading()
    {
        Vector3 dirToTarget = (target.position - transform.position).normalized;
        heading = Vector3.RotateTowards(
            heading,
            dirToTarget,
            turnRateDegPerSec * Mathf.Deg2Rad * Time.fixedDeltaTime,
            0f
        ).normalized;
    }

    void ApplyThrust(float distanceToTarget)
    {
        rb.AddForce(heading * thrust * GetThrustFactor(distanceToTarget), ForceMode.Acceleration);
    }

    /// <summary>
    /// 1 = pleine poussée (loin de la cible), diminue progressivement à
    /// l'approche jusqu'à minThrustFactor. Comme le drag du Rigidbody est
    /// calibré sur la poussée max (voir Start), réduire la poussée réduit
    /// automatiquement la vitesse d'équilibre atteinte : le ralentissement
    /// vient de la physique elle-même, pas d'un frein artificiel plaqué
    /// par-dessus.
    /// </summary>
    float GetThrustFactor(float distanceToTarget)
    {
        float t = Mathf.Clamp01(distanceToTarget / decelerationDistance);
        return Mathf.Lerp(minThrustFactor, 1f, t);
    }

    /// <summary>
    /// Force active à l'opposé de la vélocité actuelle, qui monte en
    /// intensité plus on approche de la cible. Contrairement à la baisse
    /// de poussée (ApplyThrust), c'est un vrai frein : brakingForce se
    /// règle indépendamment du drag du Rigidbody, donc c'est le levier à
    /// utiliser pour intensifier le ralentissement.
    /// </summary>
    void ApplyBraking(float distanceToTarget)
    {
        if (brakingForce <= 0f) return;

        Vector3 velocity = rb.linearVelocity;
        if (velocity.sqrMagnitude < 0.01f) return;

        float t = Mathf.Clamp01(distanceToTarget / decelerationDistance);
        float brakeFactor = 1f - t; // 0 loin de la cible, 1 tout près

        rb.AddForce(-velocity.normalized * brakingForce * brakeFactor, ForceMode.Acceleration);
    }

    /// <summary>
    /// Même logique d'évitement que BirdOrbitFlight : force additionnelle
    /// superposée à la poussée normale, qui se traduit naturellement dans
    /// la rotation visuelle puisque celle-ci suit la vélocité réelle.
    /// </summary>
    void ApplyAvoidance()
    {
        Vector3 avoidance = Vector3.zero;
        Vector3 castDirection = rb.linearVelocity.sqrMagnitude > 0.01f
            ? rb.linearVelocity.normalized
            : heading;

        avoidance += ComputePlanetAvoidance(castDirection);
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

    Vector3 ComputeShipAvoidance()
    {
        Vector3 avoidance = Vector3.zero;
        Collider[] neighbours = Physics.OverlapSphere(transform.position, shipDetectionRadius, shipLayerMask);

        foreach (Collider col in neighbours)
        {
            if (col.attachedRigidbody == rb) continue;

            Vector3 away = transform.position - col.transform.position;
            float dist = away.magnitude;
            if (dist < 0.01f) continue;

            float weight = 1f - Mathf.Clamp01(dist / shipDetectionRadius);
            avoidance += (away / dist) * weight;
        }

        return avoidance;
    }

    /// <summary>La rotation affichée suit la vélocité réelle, comme sur les oiseaux et les missiles.</summary>
    void UpdateVisualRotation()
    {
        Vector3 velocity = rb.linearVelocity;
        if (velocity.sqrMagnitude < 0.05f) return;

        Quaternion lookRotation = Quaternion.LookRotation(velocity.normalized, Vector3.up);
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, lookRotation, Time.fixedDeltaTime * rotationFollowSpeed));
    }

    void OnDrawGizmosSelected()
    {
        if (target != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, target.position);
            Gizmos.DrawWireSphere(target.position, arrivalDistance);
        }

        Gizmos.color = new Color(1f, 0.6f, 0f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, shipDetectionRadius);

        Vector3 castDir = Application.isPlaying && rb != null && rb.linearVelocity.sqrMagnitude > 0.01f
            ? rb.linearVelocity.normalized
            : transform.forward;
        Gizmos.color = hasPlanetHit ? Color.red : new Color(0f, 1f, 0f, 0.5f);
        Gizmos.DrawLine(transform.position, transform.position + castDir * planetDetectionDistance);
        if (hasPlanetHit)
        {
            Gizmos.DrawWireSphere(lastPlanetHitPoint, planetCastRadius);
        }
    }
}