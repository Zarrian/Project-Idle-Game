using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Missile à tête chercheuse : suit une cible mobile avec un taux de virage
/// plus élevé et plus direct que le vol des oiseaux (moins de "grande
/// courbe WWI", juste une légère inertie de rotation). Le hit final est
/// garanti par une distance de détonation : dès que le missile entre dans
/// cette zone autour de la cible, il explose directement — indépendamment
/// d'une vraie collision physique, ce qui évite les ratés si la vitesse
/// relative est trop grande pour que la physique détecte le contact.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class MissileHoming : MonoBehaviour
{
    [Header("Cible")]
    public Transform target;

    [Header("Propulsion")]
    public float thrust = 40f;
    public float maxSpeed = 25f;

    [Header("Guidage")]
    [Tooltip("Vitesse de rotation du cap vers la cible, en degrés/seconde. Plus haut = trajectoire plus directe, moins de courbe.")]
    public float turnRateDegPerSec = 120f;

    [Tooltip("Vitesse à laquelle la rotation visuelle rattrape la vélocité réelle")]
    public float rotationFollowSpeed = 12f;

    [Header("Détonation")]
    [Tooltip("Distance à la cible en dessous de laquelle le missile explose automatiquement, même sans collision physique")]
    public float detonationDistance = 2f;

    [Tooltip("Déclenché à l'explosion : branche ici tes effets (VFX, dégâts, son...)")]
    public UnityEvent onImpact;

    private Rigidbody rb;
    private Vector3 heading;
    private bool hasExploded = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.linearDamping = thrust / Mathf.Max(maxSpeed, 0.01f);

        heading = transform.forward;

        if (target == null)
        {
            Debug.LogWarning("MissileHoming: aucune target assignée.");
        }
    }

    void FixedUpdate()
    {
        if (hasExploded || target == null) return;

        if (IsCloseEnoughToDetonate())
        {
            Explode();
            return;
        }

        UpdateHeading();
        ApplyThrust();
        UpdateVisualRotation();
    }

    bool IsCloseEnoughToDetonate()
    {
        return Vector3.Distance(transform.position, target.position) <= detonationDistance;
    }

    /// <summary>Courbe le cap vers la cible à vitesse angulaire plafonnée.</summary>
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

    void ApplyThrust()
    {
        rb.AddForce(heading * thrust, ForceMode.Acceleration);
    }

    /// <summary>La rotation affichée suit la vélocité réelle, comme sur les oiseaux.</summary>
    void UpdateVisualRotation()
    {
        Vector3 velocity = rb.linearVelocity;
        if (velocity.sqrMagnitude < 0.05f) return;

        Quaternion lookRotation = Quaternion.LookRotation(velocity.normalized, Vector3.up);
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, lookRotation, Time.fixedDeltaTime * rotationFollowSpeed));
    }

    public GameObject explosion;
    /// <summary>Explosion "manuelle", déclenchée par la proximité de la cible.</summary>
    void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        Instantiate(explosion, transform.position, transform.rotation);

        onImpact?.Invoke();
        Destroy(gameObject);
    }

    /// <summary>Explosion par vraie collision physique (au cas où ça touche avant la distance de détonation).</summary>
    void OnCollisionEnter(Collision collision)
    {
        Explode();
    }
}
