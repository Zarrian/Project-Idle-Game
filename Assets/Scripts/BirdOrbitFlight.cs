using UnityEngine;

/// <summary>
/// Fait voler un objet (oiseau, avion...) de façon autonome autour d'une sphère,
/// en vol libre (pas collé à la surface). Le mouvement est propulsé par un
/// Rigidbody (poussée constante vers l'avant) mais le cap de vol est contraint
/// à un taux max de degrés/seconde, ce qui oblige l'objet à faire de longues
/// courbes larges pour changer de direction — effet "avion de la Grande Guerre"
/// qui n'a pas la maniabilité pour tourner sec.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class BirdOrbitFlight : MonoBehaviour
{
    [Header("Sphère de référence")]
    [Tooltip("Centre de la sphère autour de laquelle on tourne")]
    public Transform sphereCenter;

    [Tooltip("Distance min/max au centre à laquelle on choisit les points cibles")]
    public float orbitRadiusMin = 15f;
    public float orbitRadiusMax = 25f;

    [Header("Propulsion")]
    [Tooltip("Force de poussée constante vers l'avant")]
    public float thrust = 20f;

    [Tooltip("Vitesse max (limitée via le drag du Rigidbody, voir Start)")]
    public float maxSpeed = 12f;

    [Header("Maniabilité (l'essentiel de l'effet WWI)")]
    [Tooltip("Vitesse à laquelle le CAP DE VOL (invisible, pas la rotation visuelle) se courbe vers la cible, en degrés/seconde. Plus c'est bas, plus les courbes sont larges et lentes.")]
    public float maxTurnRateDegPerSec = 25f;

    [Tooltip("Inclinaison en roulis dans les virages, purement visuel")]
    public float maxBankAngle = 45f;
    public float bankLerpSpeed = 2f;

    [Tooltip("Vitesse à laquelle la rotation visuelle rattrape la vélocité réelle. Haut = colle presque instantanément à la vélocité, bas = léger flottement/inertie visuelle.")]
    public float rotationFollowSpeed = 8f;

    [Header("Comportement de vol")]
    [Tooltip("Distance à laquelle on considère la cible atteinte et on en choisit une nouvelle")]
    public float targetReachedDistance = 4f;

    [Tooltip("Temps max avant de forcer un changement de cible même si pas atteinte (évite les boucles infinies)")]
    public float maxTimeOnTarget = 20f;

    [Header("Évitement — Planète")]
    [Tooltip("Layer(s) sur lesquels se trouve le collider de la planète")]
    public LayerMask planetLayerMask;

    [Tooltip("Distance de détection du SphereCast envoyé devant le vaisseau")]
    public float planetDetectionDistance = 15f;

    [Tooltip("Rayon du SphereCast, à peu près la taille du vaisseau + une marge")]
    public float planetCastRadius = 1.5f;

    [Header("Évitement — Autres vaisseaux")]
    [Tooltip("Layer(s) sur lesquels se trouvent les autres vaisseaux détectables")]
    public LayerMask shipLayerMask;

    [Tooltip("Rayon dans lequel on détecte les vaisseaux voisins")]
    public float shipDetectionRadius = 8f;

    [Header("Évitement — Force")]
    [Tooltip("Intensité de la force d'esquive ajoutée par-dessus la poussée normale")]
    public float avoidanceForce = 60f;

    private Rigidbody rb;
    private Vector3 currentTarget;
    private float timeOnCurrentTarget;
    private float currentBank;

    // Cap de pilotage : direction dans laquelle on POUSSE, distincte de la
    // rotation visuelle. Se courbe progressivement vers la cible.
    private Vector3 heading;

    // Debug/gizmo uniquement.
    private Vector3 lastPlanetHitPoint;
    private bool hasPlanetHit;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;

        // Le drag linéaire agit comme une résistance de l'air : ça plafonne
        // naturellement la vitesse atteinte par la poussée constante.
        rb.linearDamping = thrust / Mathf.Max(maxSpeed, 0.01f);

        if (sphereCenter == null)
        {
            Debug.LogWarning("BirdOrbitFlight: sphereCenter non assigné, utilise Vector3.zero.");
        }

        heading = transform.forward;
        PickNewTarget();
    }

    // La boucle physique reste volontairement une simple liste d'étapes :
    // chaque responsabilité vit dans sa propre méthode ci-dessous.
    void FixedUpdate()
    {
        UpdateTargetSelection();
        UpdateHeading();
        ApplyThrust();
        ApplyAvoidance();
        UpdateVisualRotation();
    }

    /// <summary>
    /// Choisit une nouvelle cible sur la sphère quand l'actuelle est
    /// atteinte, ou si ça traîne trop longtemps (évite les boucles infinies).
    /// </summary>
    void UpdateTargetSelection()
    {
        timeOnCurrentTarget += Time.fixedDeltaTime;
        float distToTarget = Vector3.Distance(transform.position, currentTarget);

        if (distToTarget < targetReachedDistance || timeOnCurrentTarget > maxTimeOnTarget)
        {
            PickNewTarget();
        }
    }

    /// <summary>
    /// Courbe le cap de pilotage ("heading") vers la cible à vitesse
    /// angulaire plafonnée. C'est ce vecteur qui reçoit la poussée, pas
    /// forcément la rotation affichée. C'est cette limite qui empêche les
    /// virages serrés et force les grandes courbes.
    /// </summary>
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

    /// <summary>
    /// Poussée constante le long du cap de pilotage, comme un moteur d'avion
    /// qui ne s'arrête jamais.
    /// </summary>
    void ApplyThrust()
    {
        rb.AddForce(heading * thrust, ForceMode.Acceleration);
    }

    /// <summary>
    /// Force d'esquive additionnelle, superposée à la poussée normale.
    /// Comme elle agit directement sur la vélocité, et que la rotation
    /// visuelle suit cette vélocité (voir UpdateVisualRotation), l'esquive
    /// se voit naturellement dans le vol sans rien toucher d'autre.
    /// </summary>
    void ApplyAvoidance()
    {
        Vector3 avoidance = Vector3.zero;
        Vector3 castDirection = rb.linearVelocity.sqrMagnitude > 0.01f
            ? rb.linearVelocity.normalized
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

    /// <summary>
    /// SphereCast physique devant le vaisseau, dans la direction du
    /// mouvement réel (pas juste le heading), pour détecter la planète
    /// même en pleine courbe.
    /// </summary>
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

        // Plus l'impact est proche, plus on pousse fort. hit.normal pointe
        // vers l'extérieur de la sphère, donc "s'éloigner de la surface" =
        // exactement ce qu'on veut.
        float urgency = 1f - Mathf.Clamp01(hit.distance / planetDetectionDistance);
        return hit.normal * urgency;
    }

    /// <summary>
    /// Détection simple par overlap des vaisseaux voisins, avec une force
    /// de fuite pondérée par la proximité.
    /// </summary>
    Vector3 ComputeShipAvoidance()
    {
        Vector3 avoidance = Vector3.zero;
        Collider[] neighbours = Physics.OverlapSphere(transform.position, shipDetectionRadius, shipLayerMask);

        foreach (Collider col in neighbours)
        {
            if (col.attachedRigidbody == rb) continue; // on ignore son propre collider

            Vector3 away = transform.position - col.transform.position;
            float dist = away.magnitude;
            if (dist < 0.01f) continue;

            float weight = 1f - Mathf.Clamp01(dist / shipDetectionRadius);
            avoidance += (away / dist) * weight;
        }

        return avoidance;
    }

    /// <summary>
    /// La rotation affichée suit la VÉLOCITÉ RÉELLE du Rigidbody, pas la
    /// cible ni même le heading. Comme la vélocité met un instant à
    /// "rattraper" le cap à cause de l'inertie physique, le nez pointe
    /// toujours là où l'objet va vraiment, jamais là où il voudrait aller.
    /// </summary>
    void UpdateVisualRotation()
    {
        Vector3 velocity = rb.linearVelocity;
        if (velocity.sqrMagnitude < 0.05f) return;

        Vector3 velocityDir = velocity.normalized;
        Quaternion lookRotation = Quaternion.LookRotation(velocityDir, Vector3.up);
        lookRotation *= Quaternion.Euler(0f, 0f, ComputeBankAngle(velocityDir));

        Quaternion newRotation = Quaternion.Slerp(
            rb.rotation,
            lookRotation,
            Time.fixedDeltaTime * rotationFollowSpeed
        );
        rb.MoveRotation(newRotation);
    }

    /// <summary>
    /// Roulis (bank) dans le sens du virage, purement visuel, basé sur
    /// l'écart entre le nez actuel et la direction de vélocité.
    /// </summary>
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

        // Point aléatoire uniforme sur une sphère de ce rayon.
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

        // Zone de détection des autres vaisseaux.
        Gizmos.color = new Color(1f, 0.6f, 0f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, shipDetectionRadius);

        // Portée du SphereCast vers la planète.
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
