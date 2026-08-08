using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trails : MonoBehaviour
{
    public Pool myPool;
    public List<TrailRenderer> myTrails;
    public float delay = 2;

    public MissileHoming missile;

    private void Start()
    {
        missile.onImpact.AddListener(StartDelay);
    }
    private void OnEnable()
    {
        foreach (TrailRenderer trail in myTrails)
        {
            trail.Clear();
        }
    }

    public void StartDelay()
    {
        StartCoroutine(Delay());
    }

    public IEnumerator Delay()
    {
        yield return new WaitForSeconds(delay);

        myPool.ReturnPool(gameObject);
    }

}
