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

    [Tooltip("Temps max d'anticipation.")]
    public float maxPredictionTime = 1.2f;

    [Header("Explosion")]
    public float detonationDistance = 2f;
    public UnityEvent onImpact;
    public Pool myPool;

    public int damage = 1;

    public LayerMask enemyLayer;

    Rigidbody rb;
    Rigidbody targetRb;
    Ship targetShip;

    // Mis en cache une fois pour éviter de refaire la multiplication à
    // chaque frame dans la comparaison de distance.
    float detonationDistanceSqr;

    void Awake()
    {
        // GetComponent une seule fois pour toute la durée de vie de
        // l'objet, même s'il est réactivé plusieurs fois depuis le pool.
        rb = GetComponent<Rigidbody>();
    }

    void OnEnable()
    {
        // OnEnable (contrairement à Start) se relance à chaque fois que le
        // missile ressort du pool — c'est ici qu'il faut remettre l'état à
        // zéro, sinon un missile réutilisé garde exploded/vitesse du tir
        // précédent le temps d'un frame.
        rb.useGravity = false;
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;
        detonationDistanceSqr = detonationDistance * detonationDistance;

        targetRb = null;
        targetShip = null;
        if (target != null)
        {
            targetRb = target.GetComponent<Rigidbody>();
            targetShip = target.GetComponent<Ship>();
        }

        rb.linearVelocity = transform.forward * speed;
    }

    void FixedUpdate()
    {
        //Si la target n'est plus
        if (target.gameObject.activeSelf == false)
        {
            // La cible a été détruite en vol : on cherche la plus proche
            // cible valide restante plutôt que d'abandonner directement.
            target = FunctionUsefullManager.Instance.TryFindNearestTarget(transform, enemyLayer);
            if (target == null)
            {
                myPool.ReturnPool(gameObject);
            }
            return; // reprend le guidage normal au prochain FixedUpdate
        }

        // Positions lues une seule fois par frame et réutilisées partout
        // en dessous, plutôt que de relire transform.position/target.position
        // à chaque usage (chaque lecture recalcule depuis les matrices).
        Vector3 currentPosition = transform.position;
        Vector3 targetPosition = target.position;
        Vector3 toTarget = targetPosition - currentPosition;
        float sqrDistance = toTarget.sqrMagnitude;

        // Comparaison en distance au carré : évite un sqrt tant qu'on n'a
        // pas besoin de la vraie distance.
        if (sqrDistance <= detonationDistanceSqr)
        {
            Explode(currentPosition, targetPosition);
            return;
        }

        // À partir d'ici on a vraiment besoin de la distance réelle (pour
        // la prédiction en mètres/seconde et le multiplicateur de virage),
        // donc un seul sqrt, calculé une seule fois.
        float distance = Mathf.Sqrt(sqrDistance);

        Vector3 targetVelocity = targetRb != null ? targetRb.linearVelocity : Vector3.zero;

        //-------------------------------------
        // LEAD PURSUIT
        //-------------------------------------
        float prediction = Mathf.Clamp(distance / speed, 0f, maxPredictionTime);
        Vector3 predictedPosition = targetPosition + targetVelocity * prediction;
        Vector3 desiredDirection = (predictedPosition - currentPosition).normalized;

        //-------------------------------------
        // TURN RATE
        //-------------------------------------
        float multiplier = Mathf.Lerp(closeTurnMultiplier, 1f, Mathf.Clamp01(distance / 30f));
        Quaternion desiredRotation = Quaternion.LookRotation(desiredDirection);
        rb.MoveRotation(
            Quaternion.RotateTowards(rb.rotation, desiredRotation, turnRate * multiplier * Time.fixedDeltaTime)
        );

        //-------------------------------------
        // SPEED
        //-------------------------------------
        float currentSpeed = rb.linearVelocity.magnitude;
        currentSpeed = Mathf.MoveTowards(currentSpeed, speed, acceleration * Time.fixedDeltaTime);
        rb.linearVelocity = rb.rotation * Vector3.forward * currentSpeed;
    }

    void Explode(Vector3 currentPosition, Vector3 targetPosition)
    {

        // Buffer de sécurité (+1) au cas où Explode() serait un jour aussi
        // déclenché par une collision physique plutôt que la seule
        // détection de proximité.
        float bufferSqr = (detonationDistance + 1f) * (detonationDistance + 1f);
        if ((targetPosition - currentPosition).sqrMagnitude <= bufferSqr && targetShip != null)
        {
            targetShip.TakeDamage(damage);
        }

        onImpact?.Invoke();
        myPool.ReturnPool(gameObject);
        // exploded reste à true : il ne repasse à false que dans OnEnable(),
        // au prochain GetPoolObject() de ce missile. Le remettre ici
        // rouvrait une fenêtre d'une frame où l'objet pouvait redéclencher
        // une explosion avant que le SetActive(false) du pool ne soit effectif.
    }

}