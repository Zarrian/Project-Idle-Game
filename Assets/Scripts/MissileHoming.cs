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

    public Pool myPoolExplosion;
    public Pool myPool;

    Rigidbody rb;
    Rigidbody targetRb;

    bool exploded;

    public int damage = 1;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        rb.useGravity = false;
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;

        if (target != null)
            targetRb = target.GetComponent<Rigidbody>();

        rb.linearVelocity = transform.forward * speed;
    }

    void FixedUpdate()
    {
        if (exploded || target == null)
            return;

        Vector3 targetVelocity = Vector3.zero;

        if (targetRb != null)
            targetVelocity = targetRb.linearVelocity;

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance <= detonationDistance)
        {
            Explode();
            return;
        }

        //-------------------------------------
        // LEAD PURSUIT
        //-------------------------------------

        float prediction = Mathf.Clamp(distance / speed, 0f, maxPredictionTime);

        Vector3 predictedPosition =
            target.position +
            targetVelocity * prediction;

        Vector3 desiredDirection =
            (predictedPosition - transform.position).normalized;

        //-------------------------------------
        // TURN RATE
        //-------------------------------------

        float multiplier = Mathf.Lerp(
            closeTurnMultiplier,
            1f,
            Mathf.Clamp01(distance / 30f));

        Quaternion desiredRotation =
            Quaternion.LookRotation(desiredDirection);

        rb.MoveRotation(
            Quaternion.RotateTowards(
                rb.rotation,
                desiredRotation,
                turnRate * multiplier * Time.fixedDeltaTime));

        //-------------------------------------
        // SPEED
        //-------------------------------------

        float currentSpeed = rb.linearVelocity.magnitude;

        currentSpeed = Mathf.MoveTowards(
            currentSpeed,
            speed,
            acceleration * Time.fixedDeltaTime);

        rb.linearVelocity = rb.rotation * Vector3.forward * currentSpeed;
    }


    void Explode()
    {
        if (exploded)
            return;

        exploded = true;

        GameObject explosion = myPoolExplosion.GetPoolObject();

        //Check if close to the enemy
        float distance = Vector3.Distance(transform.position, target.position);
        if (distance <= detonationDistance+1)
            target.GetComponent<Ship>().TakeDamage(damage);

        //VFX
        explosion.transform.position = transform.position;
        explosion.transform.rotation = transform.rotation;

        onImpact?.Invoke();

        myPool.ReturnPool(gameObject);

        exploded = false;
    }
}