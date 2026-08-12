using UnityEngine;
using System.Collections;
using UnityEngine.Events;

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

    [Header("Laser Settings")]
    [SerializeField] private float laserWidth = 0.2f;
    [SerializeField] private Color laserColorIdle = new Color(0f, 1f, 0f, 0.5f);
    [SerializeField] private Color laserColorFire = new Color(1f, 0f, 0f, 1f);

    public LineRenderer laserLine;
    private Coroutine fireEffectCoroutine;

    private void OnEnable()
    {
        SetStatistique();
        SetupLaser();
    }

    private void SetupLaser()
    {
        // Créer un GameObject pour le LineRenderer
        GameObject laserObject = new GameObject("LaserBeam");
        laserObject.transform.SetParent(transform);
        laserObject.transform.localPosition = Vector3.zero;

        laserLine = laserObject.AddComponent<LineRenderer>();
        laserLine.material = new Material(Shader.Find("Sprites/Default"));
        laserLine.startWidth = laserWidth;
        laserLine.endWidth = laserWidth;
        laserLine.startColor = laserColorIdle;
        laserLine.endColor = laserColorIdle;
        laserLine.positionCount = 0;
    }

    private void FixedUpdate()
    {
        DetectTargets();

        if (currenttarget != null)
        {
            LookTarget();
            //UpdateLaserPosition();

            currentAttack += Time.fixedDeltaTime;
            if (currentAttack >= cdAttack)
            {
                Fire(currenttarget.gameObject);
                currentAttack = 0;
            }
        }
        else
        {
            //HideLaser();
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

    [SerializeField] private float rotationSpeed = 180f; // À ajuster dans l'Inspecteur
    
    public void LookTarget()
    {
        Vector3 directionToTarget = (currenttarget.position - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget, Vector3.up);
        cannonBarrel.rotation = Quaternion.RotateTowards(cannonBarrel.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void DetectTargets()
    {
        // Utiliser la direction du barrel (pas transform.up)
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
            //Debug.Log($"Canon {gameObject.name} a perdu sa cible");
            currenttarget = null;
            //HideLaser();
        }
    }

    private void OnTargetDetected(GameObject target)
    {
        currenttarget = target.transform;
        //DrawLaser();
    }

    public void DrawLaser()
    {
        if (laserLine == null || currenttarget == null)
            return;

        laserLine.startColor = laserColorIdle;
        laserLine.endColor = laserColorIdle;
    }

    private void UpdateLaserPosition()
    {
        if (laserLine == null || currenttarget == null)
            return;

        Vector3 startPos = cannonBarrel != null ? cannonBarrel.position : transform.position;
        Vector3 endPos = currenttarget.position;

        laserLine.positionCount = 2;
        laserLine.SetPosition(0, startPos);
        laserLine.SetPosition(1, endPos);
    }

    public UnityEvent OnFire;
    private void Fire(GameObject target)
    {
        if (currenttarget == null)
        {
            Debug.Log($"Canon {gameObject.name} tire !");
            return;
        }

        // Arrêter l'effet précédent
        if (fireEffectCoroutine != null)
            StopCoroutine(fireEffectCoroutine);

        //fireEffectCoroutine = StartCoroutine(LaserFireEffect());
        OnFire?.Invoke();
        target.GetComponent<Ship>().TakeDamage(damage, transform.position);
        //Instantie aussi un autre VFX ?
    }

 /*   private IEnumerator LaserFireEffect()
    {
        float laserDuration = 0.33f; // Durée courte pour l'effet de tir
        float elapsedTime = 0f;

        // Changer la couleur du laser à rouge/intense
        laserLine.startColor = laserColorFire;
        laserLine.endColor = laserColorFire;

        while (elapsedTime < laserDuration)
        {
            elapsedTime += Time.deltaTime;

            // Pulsation optionnelle
            float pulse = Mathf.Sin(elapsedTime * 10f) * 0.5f + 0.5f;
            laserLine.startWidth = laserWidth * (0.8f + pulse * 0.4f);
            laserLine.endWidth = laserWidth * (0.8f + pulse * 0.4f);

            yield return null;
        }

        // Revenir à la couleur normale
        laserLine.startColor = laserColorIdle;
        laserLine.endColor = laserColorIdle;
        laserLine.startWidth = laserWidth;
        laserLine.endWidth = laserWidth;
    }*/

    private void HideLaser()
    {
        if (laserLine != null)
        {
            laserLine.positionCount = 0;
        }
    }

}