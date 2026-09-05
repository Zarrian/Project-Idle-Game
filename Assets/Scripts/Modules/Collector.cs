using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collector : HangarShip
{
    private Coroutine findScrapCoroutine;

    public override void OnEnable()
    {
        base.OnEnable();

        // Garde contre les r�-activations (SetActive false/true) : sans �a,
        // chaque OnEnable empilait une chaine FindScrap() infinie de plus,
        // qui ne s'arr�tait jamais (fuite de coroutines).
        if (findScrapCoroutine != null)
            StopCoroutine(findScrapCoroutine);

        findScrapCoroutine = StartCoroutine(FindScrap());
    }

    private void OnDisable()
    {
        if (findScrapCoroutine != null)
        {
            StopCoroutine(findScrapCoroutine);
            findScrapCoroutine = null;
        }
    }

    public IEnumerator FindScrap()
    {
        // while(true) au lieu de se relancer soi-m�me via StartCoroutine :
        // une seule coroutine vit ici au lieu d'en recr�er une nouvelle
        // chaque seconde.
        while (true)
        {
            yield return new WaitForSeconds(1);

            if (unitsList.Count > 0)
            {
                foreach (GameObject collectorGO in unitsList)
                {
                    CollectorShip collector = collectorGO.GetComponent<CollectorShip>();

                    if (collector.target == null)
                    {
                        FindNearestScrap(collector);
                    }
                }
            }
        }
    }

    public override void FixedUpdate()
    {
        //base.FixedUpdate();

        SpawnUnits();
    }

    [Header("Recherche de scrap")]
    public LayerMask scrapLayer;
    public float scrapSearchRadius = 50f;

    // Buffer r�utilis� pour �viter d'allouer un nouveau tableau � chaque
    // appel de Physics.OverlapSphere (un par collector sans cible, chaque
    // seconde) : source de garbage continue si beaucoup de collectors.
    private readonly Collider[] scrapOverlapBuffer = new Collider[32];

    public void FindNearestScrap(CollectorShip collector)
    {
        int scrapCount = Physics.OverlapSphereNonAlloc(collector.transform.position, scrapSearchRadius, scrapOverlapBuffer, scrapLayer);
        if (scrapCount == 0) return;

        // Tri� du plus proche au plus loin, pour pouvoir passer au suivant si
        // le plus proche est d�j� vis� par un autre collector.
        System.Array.Sort(scrapOverlapBuffer, 0, scrapCount, Comparer<Collider>.Create((a, b) =>
            (a.transform.position - collector.transform.position).sqrMagnitude.CompareTo(
            (b.transform.position - collector.transform.position).sqrMagnitude)));

        Transform chosenTarget = null;
        for (int i = 0; i < scrapCount; i++)
        {
            Collider scrap = scrapOverlapBuffer[i];
            if (!IsAlreadyTargeted(scrap.transform, collector.transform))
            {
                chosenTarget = scrap.transform;
                break;
            }
        }

        // Tous les scraps � port�e sont d�j� pris par d'autres collectors :
        // on n'assigne rien plut�t que de forcer un doublon.
        if (chosenTarget == null) return;

        collector.target = chosenTarget;

    }

    /// <summary>Vrai si un AUTRE collector de UnitList vise d�j� ce scrap.</summary>
    bool IsAlreadyTargeted(Transform scrap, Transform excludingCollector)
    {
        foreach (GameObject unit in unitsList)
        {
            if (unit.transform == excludingCollector) continue;

            CollectorShip otherCollector = unit.GetComponent<CollectorShip>();
            if (otherCollector != null && otherCollector.target == scrap)
            {
                return true;
            }
        }
        return false;
    }
}
