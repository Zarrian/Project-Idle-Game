using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class MissileHoming : MonoBehaviour
{
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
    private Ship targetShip; // === OPTIMISATION : Cache du composant Ship ===

    private float detonationDistanceSqr;
    private float closeTurnDistance = 30f; // === OPTIMISATION : Constante pour lerp ===
    private float closeTurnDistanceSqr; // === OPTIMISATION : Distance au carré ===

    // === OPTIMISATION : Cache de la vélocité cible ===
    private Vector3 cachedTargetVelocity;
    private Vector3 cachedTargetPosition;
    private float cachedCurrentSpeed;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnEnable()
    {
        rb.useGravity = false;
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;
        
        detonationDistanceSqr = detonationDistance * detonationDistance;
        closeTurnDistanceSqr = closeTurnDistance * closeTurnDistance; // === OPTIMISATION ===

        targetRb = null;
        targetShip = null; // === OPTIMISATION ===
        
        if (target != null)
        {
            targetRb = target.GetComponent<Rigidbody>();
            targetShip = target.GetComponent<Ship>(); // === OPTIMISATION : Cache une seule fois ===
        }

        rb.linearVelocity = transform.forward * speed;
        cachedCurrentSpeed = speed; // === OPTIMISATION ===
    }

    void FixedUpdate()
    {
        // === OPTIMISATION : Vérifier si target est vraiment null/inactif ===
        if (target == null || !target.gameObject.activeSelf)
        {
            target = FunctionUsefullManager.Instance.TryFindNearestTarget(transform, enemyLayer);
            if (target == null)
            {
                myPool.ReturnPool(gameObject);
                return;
            }
            
            targetRb = target.GetComponent<Rigidbody>();
            targetShip = target.GetComponent<Ship>(); // === OPTIMISATION : Cache ici aussi ===
        }

        // === OPTIMISATION : Ne pas refaire GetComponent si déjà en cache ===
        if (targetRb == null && target != null)
        {
            targetRb = target.GetComponent<Rigidbody>();
        }
        
        if (targetShip == null && target != null)
        {
            targetShip = target.GetComponent<Ship>();
        }

        // === OPTIMISATION : Lecture une seule fois ===
        Vector3 currentPosition = transform.position;
        Vector3 targetPosition = target.position;
        Vector3 toTarget = targetPosition - currentPosition;
        float sqrDistance = toTarget.sqrMagnitude;

        // === OPTIMISATION : Comparaison en distance au carré ===
        if (sqrDistance <= detonationDistanceSqr)
        {
            Explode(targetPosition);
            return;
        }

        // === OPTIMISATION : Un seul sqrt ===
        float distance = Mathf.Sqrt(sqrDistance);

        // === OPTIMISATION : Cache la vélocité cible ===
        cachedTargetVelocity = targetRb != null ? targetRb.linearVelocity : Vector3.zero;

        //-------------------------------------
        // LEAD PURSUIT
        //-------------------------------------
        float prediction = Mathf.Clamp(distance / speed, 0f, maxPredictionTime);
        Vector3 predictedPosition = targetPosition + cachedTargetVelocity * prediction;
        Vector3 desiredDirection = (predictedPosition - currentPosition).normalized;

        //-------------------------------------
        // TURN RATE
        //-------------------------------------
        // === OPTIMISATION : Utiliser sqrDistance au lieu de distance pour le lerp ===
        float multiplier = Mathf.Lerp(closeTurnMultiplier, 1f, Mathf.Clamp01(sqrDistance / closeTurnDistanceSqr));
        Quaternion desiredRotation = Quaternion.LookRotation(desiredDirection);
        rb.MoveRotation(
            Quaternion.RotateTowards(rb.rotation, desiredRotation, turnRate * multiplier * Time.fixedDeltaTime)
        );

        //-------------------------------------
        // SPEED
        //-------------------------------------
        // === OPTIMISATION : Utiliser cachedCurrentSpeed au lieu de relire rb.linearVelocity.magnitude ===
        cachedCurrentSpeed = Mathf.MoveTowards(cachedCurrentSpeed, speed, acceleration * Time.fixedDeltaTime);
        rb.linearVelocity = rb.rotation * Vector3.forward * cachedCurrentSpeed;
    }

    void Explode(Vector3 targetPosition)
    {
        // === OPTIMISATION : Utiliser le cache au lieu de GetComponent ===
        if (targetShip != null)
        {
            targetShip.TakeDamage(damage, transform.position);
        }

        onImpact?.Invoke();
        myPool.ReturnPool(gameObject);
    }
}