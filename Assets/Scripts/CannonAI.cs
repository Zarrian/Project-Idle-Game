using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using VolumetricLines;

public class CannonAI : MonoBehaviour
{
    public CanonTierSet canonSO;
    public Weapon manager;

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
    [SerializeField] private float laserWidth = 0.2f;
    [SerializeField] private Color laserColorFire = new Color(1f, 0f, 0f, 1f);
    [SerializeField] private float rotationSpeed = 180f;

    //public LineRenderer laserLine;
    private Coroutine fireEffectCoroutine;

    public UnityEvent OnFire;

    private void OnEnable()
    {
        SetStatistique();
        SetupLaser();
    }

    private void SetupLaser()
    {
        //laserLine.transform.parent = null;
        //laserLine.gameObject.SetActive(false);
        //GameObject laserObject = new GameObject("LaserBeam");
        //laserObject.transform.SetParent(transform);
        //laserObject.transform.localPosition = Vector3.zero;

        //laserLine = laserObject.AddComponent<LineRenderer>();
        //laserLine.material = new Material(Shader.Find("Sprites/Default"));
        //laserLine.startWidth = laserWidth;
        //laserLine.endWidth = laserWidth;
        //laserLine.positionCount = 0;
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
    }

    public void LookTarget()
    {
        Vector3 directionToTarget = (currenttarget.position - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget, Vector3.up);
        cannonBarrel.rotation = Quaternion.RotateTowards(cannonBarrel.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void DetectTargets()
    {
        Vector3 forwardDirection = cannonBarrel != null
            ? cannonBarrel.forward
            : transform.forward;

        targetCount = Physics.OverlapSphereNonAlloc(
            transform.position,
            rangeAttack,
            colliders,
            targetLayer
        );

        bool targetStillInRange = false;

        for (int i = 0; i < targetCount; i++)
        {
            Vector3 directionToTarget = (colliders[i].transform.position - transform.position).normalized;
            float angleToTarget = Vector3.Angle(forwardDirection, directionToTarget);

            if (angleToTarget <= visionConeAngle / 2f)
            {
                if (currenttarget == null)
                {
                    OnTargetDetected(colliders[i].gameObject);
                    targetStillInRange = true;
                    break;
                }
                else if (colliders[i].gameObject == currenttarget.gameObject)
                {
                    targetStillInRange = true;
                    break;
                }
            }
        }

        if (currenttarget != null && !targetStillInRange)
        {
            currenttarget = null;
            HideLaser();
        }
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
        float timeForLaserToBeComplete = 0.1f;

        laserLine.gameObject.SetActive(true);
        Ship target = currenttarget.GetComponent<Ship>();

        // Phase de charge : laserProgress passe de 0 à 1 en timeForLaserToBeComplete secondes
        while (laserProgress < 1)
        {
            laserProgress += Time.deltaTime / timeForLaserToBeComplete;
            UpdateLaserPosition(laserProgress);
            yield return null;
        }

        // Phase de tir
        while (elapsedTime < laserDuration)
        {
            UpdateLaserPosition(1);

            if (elapsedTime - lastTickTime >= tickInterval)
            {
                if (currenttarget == null)
                {
                    HideLaser();
                    yield break;
                }

                if (target != null)
                {
                    float damagePerTick = damage * tickInterval / laserDuration;
                    target.TakeDamage(damagePerTick, transform.position);
                }
                lastTickTime = elapsedTime;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        HideLaser();
    }

    private void UpdateLaserPosition(float progress)
    {
        if (laserLine == null || currenttarget == null)
            return;

        print(progress);
        float distance = Vector3.Distance(cannonBarrel.position, currenttarget.position) * 2;
        laserLine.EndPos = Vector3.forward * (distance * progress);
        //laserLine.EndPos = Vector3.forward * Vector3.Distance(cannonBarrel.position, currenttarget.position);
    }

    private void HideLaser()
    {
        if (laserLine != null)
        {
            laserLine.gameObject.SetActive(false);
            laserLine.EndPos = Vector3.zero;
        }
    }
}