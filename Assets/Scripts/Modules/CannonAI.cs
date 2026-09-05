using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using VolumetricLines;

public class CannonAI : MonoBehaviour
{
    public CanonTierSet canonSO;
    public Hangar manager;

    [SerializeField] private float visionConeAngle = 45f;
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

    public VolumetricLineBehavior laserLine;

    [Header("Laser Settings")]
    [SerializeField] private float rotationSpeed = 180f;

    private Coroutine fireEffectCoroutine;
    private Coroutine hideLaserCoroutine;
    public UnityEvent OnFire;

    // === OPTIMISATION : Caching des valeurs ===
    private float visionConeHalfAngleCos; // Cosinus du demi-angle du cône
    private float rangeAttackSqr; // Distance au carré
    private Vector3 cachedForwardDirection;
    private float detectionCheckTimer = 0f;
    private const float DETECTION_CHECK_INTERVAL = 0.1f; // Vérifier tous les 100ms

    [SerializeField] private float laserDelay = 0.15f;

    private void OnEnable()
    {
        SetStatistique();
        SetupLaser();

        // === OPTIMISATION : Précalculer les valeurs ===
        visionConeHalfAngleCos = Mathf.Cos(visionConeAngle * 0.5f * Mathf.Deg2Rad);
        rangeAttackSqr = rangeAttack * rangeAttack;
    }

    private void SetupLaser()
    {
        // Vide intentionnellement
    }

    private void FixedUpdate()
    {
        // === OPTIMISATION : Ne pas vérifier tous les frames ===
        detectionCheckTimer += Time.fixedDeltaTime;
        if (detectionCheckTimer >= DETECTION_CHECK_INTERVAL)
        {
            DetectTargets();
            detectionCheckTimer = 0f;
        }

        if (currenttarget != null)
        {
            LookTarget();

            currentAttack += Time.fixedDeltaTime;
            if (currentAttack >= cdAttack)
            {
                Fire(currenttarget.gameObject);
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

        // === OPTIMISATION : Mettre à jour les caches ===
        rangeAttackSqr = rangeAttack * rangeAttack;
    }

    public void LookTarget()
    {
        Vector3 directionToTarget = (currenttarget.position - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget, Vector3.up);
        cannonBarrel.rotation = Quaternion.RotateTowards(cannonBarrel.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    // === OPTIMISATION MAJEURE : DetectTargets ultra-optimisé ===
    private void DetectTargets()
    {
        cachedForwardDirection = cannonBarrel != null ? cannonBarrel.forward : transform.forward;
        Vector3 origin = cannonBarrel != null ? cannonBarrel.position : transform.position;

        targetCount = Physics.OverlapSphereNonAlloc(
            transform.position,
            rangeAttack,
            colliders,
            targetLayer
        );

        bool targetStillInRange = false;
        Vector3 currentPos = transform.position;

        for (int i = 0; i < targetCount; i++)
        {
            Collider col = colliders[i];

            // === OPTIMISATION : Vérifier IDamageable en premier (early exit) ===
            IDamageable damageable = col.GetComponent<IDamageable>();
            if (damageable == null)
                continue;

            Vector3 targetPos = col.transform.position;

            // === OPTIMISATION : Vérifier la distance avant l'angle ===
            Vector3 directionToTarget = targetPos - currentPos;
            float sqrDistance = directionToTarget.sqrMagnitude;

            // Early exit si trop loin
            if (sqrDistance > rangeAttackSqr)
                continue;

            // === OPTIMISATION : Utiliser dot product au lieu de Vector3.Angle ===
            float distance = Mathf.Sqrt(sqrDistance);
            Vector3 normalizedDirection = directionToTarget / distance;

            float dotProduct = Vector3.Dot(cachedForwardDirection, normalizedDirection);

            // === OPTIMISATION : Comparer directement avec le cosinus ===
            if (dotProduct >= visionConeHalfAngleCos)
            {
                if (currenttarget == null)
                {
                    // Vérifie la ligne de vue réelle avant de valider la cible
                    Transform validatedTarget = ValidateLineOfSight(origin, col.transform);
                    if (validatedTarget == null)
                        continue;

                    OnTargetDetected(validatedTarget.gameObject);
                    targetStillInRange = true;
                    break;
                }
                else if (col.gameObject == currenttarget.gameObject)
                {
                    // Vérifie que la ligne de vue vers la cible actuelle est toujours dégagée
                    Transform validatedTarget = ValidateLineOfSight(origin, col.transform);
                    if (validatedTarget != null)
                    {
                        currenttarget = validatedTarget;
                        targetStillInRange = true;
                        break;
                    }
                    // Ligne de vue bloquée : on continue à chercher un autre candidat
                }
            }
        }

        if (currenttarget != null && !targetStillInRange)
        {
            currenttarget = null;
            StartHideLaser();
        }
    }

    /// <summary>
    /// Vérifie par raycast qu'on peut effectivement toucher la cible candidate.
    /// Si un autre vaisseau ennemi (IDamageable sur targetLayer) est touché
    /// en premier, celui-ci est retourné à la place. Retourne null si aucune
    /// cible valide n'est en ligne de vue.
    /// </summary>
    private Transform ValidateLineOfSight(Vector3 origin, Transform candidate)
    {
        Vector3 direction = (candidate.position - origin).normalized;
        float distance = Vector3.Distance(origin, candidate.position);

        if (Physics.Raycast(origin, direction, out RaycastHit hit, distance, targetLayer))
        {
            // La cible candidate est bien ce qui est touché en premier
            if (hit.transform == candidate || hit.transform.IsChildOf(candidate))
                return candidate;

            // Un autre vaisseau ennemi a été touché avant : on le prend pour cible
            IDamageable hitDamageable = hit.transform.GetComponent<IDamageable>();
            if (hitDamageable != null)
                return hit.transform;

            // Obstacle sans IDamageable : pas de ligne de vue valide
            return null;
        }

        // Rien touché sur targetLayer avant la distance de la cible : pas de ligne de vue
        return null;
    }

    private void OnTargetDetected(GameObject target)
    {
        currenttarget = target.transform;
    }

    private void Fire(GameObject target)
    {
        if (currenttarget == null)
        {
            Debug.Log($"Canon {gameObject.name} tire !");
            return;
        }

        if (fireEffectCoroutine != null)
            StopCoroutine(fireEffectCoroutine);

        if (hideLaserCoroutine != null)
        {
            StopCoroutine(hideLaserCoroutine);
            hideLaserCoroutine = null;
        }

        fireEffectCoroutine = StartCoroutine(LaserFireEffect());
        OnFire?.Invoke();
    }

    private IEnumerator LaserFireEffect()
    {
        float laserDuration = 0.5f;
        float elapsedTime = 0f;
        float tickInterval = 0.1f;
        float lastTickTime = 0f;

        float laserProgress = 0;
        float timeForLaserToBeComplete = laserDelay;

        laserLine.gameObject.SetActive(true);
        Ship target = currenttarget.GetComponent<Ship>();

        while (laserProgress < 1)
        {
            laserProgress += Time.deltaTime / timeForLaserToBeComplete;
            UpdateLaserPosition(laserProgress);
            yield return null;
        }

        while (elapsedTime < laserDuration)
        {
            UpdateLaserPosition(1);

            if (elapsedTime - lastTickTime >= tickInterval)
            {
                if (currenttarget == null)
                {
                    StartHideLaser();
                    yield break;
                }

                if (target != null)
                {
                    IDamageable damageable = target.GetComponent<IDamageable>();
                    if (damageable != null)
                    {
                        float damagePerTick = damage * tickInterval / laserDuration;
                        damageable.TakeDamage(damagePerTick, transform.position);
                    }


                }
                lastTickTime = elapsedTime;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        StartHideLaser();
    }

    private void UpdateLaserPosition(float progress)
    {
        if (laserLine == null || currenttarget == null)
            return;

        float distance = Vector3.Distance(cannonBarrel.position, currenttarget.position) * 0.9f;
        laserLine.EndPos = Vector3.forward * (distance * progress);
    }

    private IEnumerator HideLaser()
    {
        float laserProgress = 1;
        float timeForLaserToDisappear = laserDelay;

        while (laserProgress > 0)
        {
            laserProgress -= Time.deltaTime / timeForLaserToDisappear;

            if (currenttarget != null)
            {
                UpdateLaserPosition(laserProgress);
                LookTarget();
            }

            yield return null;
        }

        Deactivate();
        hideLaserCoroutine = null;
    }

    private void StartHideLaser()
    {
        if (hideLaserCoroutine != null)
            StopCoroutine(hideLaserCoroutine);

        hideLaserCoroutine = StartCoroutine(HideLaser());
    }

    public void Deactivate()
    {
        if (laserLine != null)
        {
            laserLine.gameObject.SetActive(false);
            laserLine.EndPos = Vector3.zero;
        }
    }
}