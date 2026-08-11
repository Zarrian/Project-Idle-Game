using System.Collections;
using UnityEngine;

public class Collector : WeaponShip
{

    public override void Start()
    {
        base.Start();

        StartCoroutine(FindScrap());
    }

    public IEnumerator FindScrap()
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

        StartCoroutine(FindScrap());
    }

    public override void FixedUpdate()
    {
        //base.FixedUpdate();

        SpawnUnits();
    }

    [Header("Recherche de scrap")]
    public LayerMask scrapLayer;
    public float scrapSearchRadius = 50f;

    public void FindNearestScrap(CollectorShip collector)
    {
        print("trytofindNearestScrap");
        Collider[] scraps = Physics.OverlapSphere(collector.transform.position, scrapSearchRadius, scrapLayer);
        if (scraps.Length == 0) return;

        // Trié du plus proche au plus loin, pour pouvoir passer au suivant si
        // le plus proche est déjà visé par un autre collector.
        System.Array.Sort(scraps, (a, b) =>
            (a.transform.position - collector.transform.position).sqrMagnitude.CompareTo(
            (b.transform.position - collector.transform.position).sqrMagnitude));

        Transform chosenTarget = null;
        foreach (Collider scrap in scraps)
        {
            if (!IsAlreadyTargeted(scrap.transform, collector.transform))
            {
                chosenTarget = scrap.transform;
                print(chosenTarget);
                break;
            }
        }

        // Tous les scraps à portée sont déjà pris par d'autres collectors :
        // on n'assigne rien plutôt que de forcer un doublon.
        if (chosenTarget == null) return;

        collector.target = chosenTarget;

    }

    /// <summary>Vrai si un AUTRE collector de UnitList vise déjà ce scrap.</summary>
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
