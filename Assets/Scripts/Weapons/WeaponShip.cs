using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class WeaponShip : Weapon
{
    //Ajouttez fonction pour faire spawn des ships

    public UnitTierSet shipSO;
    public LayerMask invaderLayer;
    public LayerMask InvaderAndPlanetLayer;
    bool isCreatingShip = false;

    public Pool myPool;

    Transform _groupShip;

    float currentTimeAttack;
    private void Start()
    {
        //Save and disociate groupGameObject
        _groupShip = transform.GetChild(0);
        transform.GetChild(0).parent = null;
    }

    private void FixedUpdate()
    {
        SpawnUnits();

        //If at least 1 unit is alive, attack
        if (unitsList.Count > 0)
        {
            currentTimeAttack += Time.fixedDeltaTime;
            if (currentTimeAttack >= shipSO.tiers[0].cdAttack / unitsList.Count)
            {
                CheckAttack();
                currentTimeAttack = 0;
            }
        }
    }

    public void CheckAttack()
    {
        List<GameObject> unitInRange = new List<GameObject>();
        foreach (GameObject unit in unitsList)
        {
            Collider[] invader = Physics.OverlapSphere(unit.transform.position, shipSO.tiers[currentTier].rangeAttack, invaderLayer);
            if (invader.Length > 0)
            {
                unitInRange.Add(unit);
            }
        }

        //Shuffle list
        for (int i = unitInRange.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            (unitInRange[i], unitInRange[randomIndex]) = (unitInRange[randomIndex], unitInRange[i]);
        }

        bool attackSucess = false;
        foreach (GameObject unit in unitInRange)
        {
            Collider[] invader = Physics.OverlapSphere(unit.transform.position, shipSO.tiers[currentTier].rangeAttack, invaderLayer);

            foreach (Collider target in invader)
            {
                Vector3 direction = (target.transform.position - unit.transform.position).normalized;
                float distance = Vector3.Distance(unit.transform.position, target.transform.position);

                if (Physics.Raycast(unit.transform.position, direction, out RaycastHit hit, distance, InvaderAndPlanetLayer))
                {
                    if (hit.collider != null)
                    {
                        if (hit.collider.gameObject == target.gameObject)
                        {
                            Debug.Log("Attack success " + hit.collider.name);
                            Attack(unit.transform, target.transform);
                            attackSucess = true;
                            break;
                        }
                    }
                }
            }

            if (attackSucess == true)
                break;
        }
    }

    public void Attack(Transform ship, Transform target)
    {
        GameObject missile = myPool.GetPoolObject();
        missile.transform.position = ship.transform.position;
        missile.transform.rotation = ship.transform.rotation;
        missile.GetComponent<MissileHoming>().target = target;

        //GameObject newAttack = Instantiate(shipSO.tiers[currentTier].attack, ship.transform.position, ship.transform.rotation);
        //newAttack.GetComponent<MissileHoming>().target = target;
    }

    public void SpawnUnits()
    {
        if (unitsList.Count < shipSO.tiers[currentTier].maxUnits && isCreatingShip == false)
        {
            StartCoroutine(WaitCreateShip());
        }
    }

    public IEnumerator WaitCreateShip()
    {
        isCreatingShip = true;
        yield return new WaitForSeconds(shipSO.tiers[currentTier].cdSpawnUnits);

        CreateShip();
    }

    public void CreateShip()
    {
        //Faire une pool
        GameObject newShips = Instantiate(shipSO.tiers[currentTier].ship, transform.position, transform.rotation, _groupShip);
        unitsList.Add(newShips);
        isCreatingShip = false;
    }
}
