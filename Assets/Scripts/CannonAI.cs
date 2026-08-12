using UnityEngine;

public class CannonAI : MonoBehaviour
{

    public CanonTierSet canonSO;
    public Weapon manager;

    [SerializeField] private float visionConeAngle = 45f;
    [SerializeField] private float visionRange = 50f;
    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private Transform cannonBarrel;

    public Transform currenttarget;

    private Collider[] colliders = new Collider[50];
    private int targetCount;

    public float currentAttack = 0;

    public float pv;
    public float damage;
    public float cdAttack;
    public float rangeAttack;
    public int nbAttack = 1;

    private void OnEnable()
    {
        SetStatistique();
    }

    private void FixedUpdate()
    {
        DetectTargets();

        if (currenttarget != null)
        {
            LookTarget();

            currentAttack += Time.fixedDeltaTime;
            if (currentAttack >= cdAttack)
            {
                Fire();
                currentAttack = 0;
            }
        }
    }

    public void SetStatistique()
    {
        pv = canonSO.tiers[manager.currentTier].pv;
        damage = canonSO.tiers[manager.currentTier].damage;
        cdAttack = canonSO.tiers[manager.currentTier].cdAttack;
        rangeAttack = canonSO.tiers[manager.currentTier].rangeAttack;
        nbAttack = canonSO.tiers[manager.currentTier].nbAttack;
    }

    public void LookTarget()
    {
        Vector3 directionToTarget = (currenttarget.position - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget, Vector3.up);
        cannonBarrel.rotation = Quaternion.RotateTowards(cannonBarrel.rotation, targetRotation, 90f * Time.deltaTime);
    }

    private void DetectTargets()
    {
        Vector3 forwardDirection = transform.up; // Direction d'orientation du canon

        // Chercher tous les objets dans la portée
        targetCount = Physics.OverlapSphereNonAlloc(
            transform.position,
            visionRange,
            colliders,
            targetLayer
        );

        for (int i = 0; i < targetCount; i++)
        {
            Vector3 directionToTarget = (colliders[i].transform.position - transform.position).normalized;
            float angleToTarget = Vector3.Angle(forwardDirection, directionToTarget);

            // Vérifier si la cible est dans le cône de vision
            if (angleToTarget <= visionConeAngle / 2f)
            {
                if (currenttarget == null)
                    OnTargetDetected(colliders[i].gameObject);
                else if (colliders[i].gameObject != currenttarget?.gameObject) // Vérifie si ce n'est pas déjà la cible actuelle
                    OnTargetDetected(colliders[i].gameObject);
            }
        }
    }

    private void OnTargetDetected(GameObject target)
    {
        // À customiser selon vos besoins
        Debug.Log($"Canon {gameObject.name} détecte : {target.name}");
        currenttarget = target.transform;
    }

    private void Fire()
    {
        // Votre logique de tir ici
        Debug.Log($"Canon {gameObject.name} tire !");
    }

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
            return;

        Vector3 forwardDirection = transform.up;

        // Dessiner la portée (sphère)
        Gizmos.color = new Color(1f, 1f, 0f, 0.1f);
        DrawWireSphere(transform.position, visionRange, 16);

        // Dessiner le cône de vision
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        DrawVisionCone(transform.position, forwardDirection, visionRange, visionConeAngle, 16);

        // Dessiner la direction avant
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + forwardDirection * visionRange);
    }

    private void DrawWireSphere(Vector3 center, float radius, int segments)
    {
        float segmentAngle = 360f / segments;
        Vector3 prevPoint = center + new Vector3(radius, 0, 0);

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * segmentAngle * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(
                Mathf.Cos(angle) * radius,
                0,
                Mathf.Sin(angle) * radius
            );
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }

    private void DrawVisionCone(Vector3 origin, Vector3 direction, float range, float angle, int segments)
    {
        float halfAngle = angle / 2f;
        float segmentAngle = angle / segments;

        for (int i = 0; i < segments; i++)
        {
            float currentAngle = -halfAngle + i * segmentAngle;
            float nextAngle = -halfAngle + (i + 1) * segmentAngle;

            // Créer les points du cône
            Vector3 point1 = GetConePoint(origin, direction, range, currentAngle);
            Vector3 point2 = GetConePoint(origin, direction, range, nextAngle);

            Gizmos.DrawLine(origin, point1);
            Gizmos.DrawLine(point1, point2);
        }

        // Fermer le cône
        Vector3 lastPoint = GetConePoint(origin, direction, range, -halfAngle);
        Vector3 endPoint = GetConePoint(origin, direction, range, halfAngle);
        Gizmos.DrawLine(lastPoint, endPoint);
    }

    private Vector3 GetConePoint(Vector3 origin, Vector3 direction, float range, float angleOffset)
    {
        Quaternion rotation = Quaternion.AngleAxis(angleOffset, Vector3.right);
        return origin + rotation * direction * range;
    }
}