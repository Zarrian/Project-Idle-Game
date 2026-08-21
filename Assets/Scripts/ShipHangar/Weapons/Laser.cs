using System.Collections;
using UnityEngine;
using VolumetricLines;

public class Laser : MonoBehaviour
{
    public Pool myPool;
    public VolumetricLineBehavior laserLine;
    public Transform target;

    public float damage;
    public Coroutine laserCoroutine;

    public void ActiveLaser(Transform ship, Transform target, float damage)
    {
        laserLine.EndPos = Vector3.zero;
        laserLine.StartPos = Vector3.zero;

        transform.parent = ship;
        transform.localPosition = Vector3.zero;

        this.target = target;
        this.damage = damage;


        if (laserCoroutine != null)
            StopCoroutine(laserCoroutine);

        laserCoroutine = StartCoroutine(LaserFireEffect());
    }

    private IEnumerator LaserFireEffect()
    {
        float laserDuration = 0.2f;
        float elapsedTime = 0f;
        float tickInterval = 0.05f;
        float lastTickTime = 0f;

        float laserProgress = 0;
        float timeForLaserToBeComplete = 0.1f;

        laserLine.gameObject.SetActive(true);
        IDamageable damageable = target.GetComponent<IDamageable>();

        while (laserProgress < 1)
        {
            transform.LookAt(target.position);
            laserProgress += Time.deltaTime / timeForLaserToBeComplete;
            UpdateLaserPosition(laserProgress);
            yield return null;
        }

        while (elapsedTime < laserDuration)
        {
            transform.LookAt(target.position);
            UpdateLaserPosition(1);

            if (elapsedTime - lastTickTime >= tickInterval)
            {
                if (target.gameObject.activeSelf == false)
                {
                    StartCoroutine( HideLaser(laserProgress));
                    yield break;
                }

                if (damageable != null)
                {
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

        StartCoroutine(HideLaser(laserProgress));
    }

    private void UpdateLaserPosition(float progress)
    {
        if (laserLine == null || target == null)
            return;

        float distance = Vector3.Distance(transform.position, target.position) * 2;
        laserLine.EndPos = Vector3.forward * (distance * progress);
    }

    private IEnumerator HideLaser(float progressLaserOriginal)
    {
        float laserProgress = progressLaserOriginal;
        float timeForLaserToDisappear = 0.5f;

        while (laserProgress > 0)
        {
            laserProgress -= Time.deltaTime / timeForLaserToDisappear;
            UpdateLaserPosition(laserProgress);
            transform.LookAt(target.position);
            yield return null;
        }

        laserLine.gameObject.SetActive(false);
        myPool.ReturnPool(gameObject);

        yield return null;
    }

}
