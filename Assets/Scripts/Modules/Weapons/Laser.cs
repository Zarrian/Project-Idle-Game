using System.Collections;
using UnityEngine;
using VolumetricLines;

public class Laser : MonoBehaviour
{
    public Pool myPool;
    public VolumetricLineBehavior laserLine;

    // Le vaisseau n'est plus un vrai parent Unity : on suit sa position
    // manuellement pour ne pas dépendre du cycle de vie de son GameObject.
    public Transform ship;
    public Transform target;

    [SerializeField] float delayLaser = 0.15f;
    [SerializeField] float laserDuration = 0.2f;
    [SerializeField] float ratioLineWidth = 1;

    public float damage;

    private Coroutine mainCoroutine;
    private Coroutine securityCoroutine;
    private bool isReturning = false;

    public void ActiveLaser(Transform ship, Transform target, float damage)
    {
        // Annule tout ce qui tournait avant (réutilisation depuis le pool)
        StopAllCoroutines();
        isReturning = false;

        laserLine.EndPos = Vector3.zero;
        laserLine.StartPos = Vector3.zero;

        // On ne parente plus réellement au vaisseau : ça évite que ce
        // GameObject soit détruit automatiquement si le vaisseau meurt.
        transform.SetParent(myPool != null ? myPool.transform : null, false);

        this.ship = ship;
        this.target = target;
        this.damage = damage;

        if (ship != null)
            transform.position = ship.position;

        float scaleValue = Mathf.Sqrt(damage) * ratioLineWidth;
        laserLine.LineWidth = scaleValue;

        mainCoroutine = StartCoroutine(LaserSequence());
        //securityCoroutine = StartCoroutine(SecurityTimeout());
    }

    private IEnumerator LaserSequence()
    {
        laserLine.gameObject.SetActive(true);

        // --- Phase 1 : le laser grandit jusqu'à la cible ---
        float laserProgress = 0f;
        float timeForLaserToBeComplete = delayLaser;

        while (laserProgress < 1f)
        {
            if (!IsShipAndTargetAlive())
            {
                yield return StartCoroutine(HideLaser(laserProgress));
                yield break;
            }

            FollowShipAndLookAtTarget();
            laserProgress += Time.deltaTime / timeForLaserToBeComplete;
            UpdateLaserPosition(laserProgress);
            yield return null;
        }

        // --- Phase 2 : dégâts tant que le laser est maintenu ---
        IDamageable damageable = target.GetComponent<IDamageable>();
        float elapsedTime = 0f;
        float tickInterval = 0.05f;
        float lastTickTime = 0f;

        while (elapsedTime < laserDuration)
        {
            if (!IsShipAndTargetAlive())
                break;

            FollowShipAndLookAtTarget();
            UpdateLaserPosition(1);

            if (elapsedTime - lastTickTime >= tickInterval)
            {
                if (damageable != null)
                {
                    float damagePerTick = damage * tickInterval / laserDuration;
                    damageable.TakeDamage(damagePerTick, transform.position);
                }

                lastTickTime = elapsedTime;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        yield return StartCoroutine(HideLaser(laserProgress));
    }

    // true seulement si le vaisseau ET la cible existent ET sont actifs.
    // Important : le vaisseau (comme la cible) peut être poolé (SetActive(false))
    // plutôt que détruit -> une simple vérif "!= null" ne suffit pas, une
    // Transform désactivée reste une référence valide en mémoire.
    private bool IsShipAndTargetAlive()
    {
        return ship != null && ship.gameObject.activeInHierarchy
            && target != null && target.gameObject.activeInHierarchy;
    }

    private void FollowShipAndLookAtTarget()
    {
        if (ship != null)
            transform.position = ship.position;

        if (target != null)
            transform.LookAt(target.position);
    }

    private void UpdateLaserPosition(float progress)
    {
        if (laserLine == null || target == null)
            return;

        float distance = Vector3.Distance(transform.position, target.position);
        laserLine.EndPos = Vector3.forward * (distance * progress);
    }

    private IEnumerator HideLaser(float progressLaserOriginal)
    {
        float laserProgress = progressLaserOriginal;
        float timeForLaserToDisappear = delayLaser;

        while (laserProgress > 0)
        {
            laserProgress -= Time.deltaTime / timeForLaserToDisappear;

            if (target != null)
            {
                UpdateLaserPosition(laserProgress);
                transform.LookAt(target.position);
            }

            yield return null;
        }

        Deactivate();
    }

    // Point d'entrée unique pour "éteindre" le laser. Ne touche PAS au pool
    // directement : ça se passe uniquement dans OnDisable, pour garantir
    // qu'il n'y a jamais deux retours au pool pour le même objet.
    private void Deactivate()
    {
        if (gameObject.activeSelf)
            gameObject.SetActive(false);
        else
            ReturnToPool(); // déjà désactivé (rare) : on force quand même le retour
    }

    private void OnDisable()
    {
        StopAllCoroutines();

        if (laserLine != null)
            laserLine.gameObject.SetActive(false);

        ReturnToPool();
    }

    // Seul et unique endroit qui appelle myPool.ReturnPool(), protégé par
    // isReturning pour être idempotent quel que soit le chemin emprunté.
    private void ReturnToPool()
    {
        if (isReturning)
            return;

        isReturning = true;

        ship = null;
        target = null;

        if (myPool != null)
            myPool.ReturnPool(gameObject);
    }
}