using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponShip : Weapon
{
    //Ajouttez fonction pour faire spawn des ships

    public UnitTierSet shipSO;
    public LayerMask invaderLayer;
    public LayerMask InvaderAndPlanetLayer;
    public bool isCreatingShip = false;

    public Pool myPoolMissile;
    public Pool myPoolShips;

    public float currentTimeAttack;
    public virtual void Start()
    {
        //Save and disociate groupGameObject
        transform.GetChild(0).parent = null;
    }

    public virtual void FixedUpdate()
    {
        SpawnUnits();

        //If at least 1 unit is alive, attack
        if (unitsList.Count > 0)
        {
            currentTimeAttack += Time.fixedDeltaTime;
            if (currentTimeAttack >= shipSO.tiers[currentTier].cdAttack / unitsList.Count)
            {
                CheckAttack();
                currentTimeAttack = 0;
            }
        }
    }


    public virtual void CheckAttack()
    {
        for (int i = 0; i < shipSO.tiers[currentTier].nbAttack; i++)
        {
            //Choisis un vaisseaux joueurs random
            GameObject randomShip = unitsList[Random.Range(0, unitsList.Count)];
            //CheckEnemyInrange
            List<GameObject> unitInRange = new List<GameObject>();
            Collider[] EnemyShip = Physics.OverlapSphere(randomShip.transform.position, shipSO.tiers[currentTier].rangeAttack, invaderLayer);

            if (EnemyShip.Length > 0)
            {
                GameObject target = ChoseEnemy(EnemyShip);

                Vector3 direction = (target.transform.position - randomShip.transform.position).normalized;
                float distance = Vector3.Distance(randomShip.transform.position, target.transform.position);
                if (Physics.Raycast(randomShip.transform.position, direction, out RaycastHit hit, distance, InvaderAndPlanetLayer))
                {
                    Attack(randomShip.transform, target.transform);
                }
                else // recommance
                    CheckAttack();
            }
        }
    }

    [SerializeField, Range(0, 100)]
    private float targetPriority = 75f; // 100 = proche, 0 = loin
   public virtual GameObject ChoseEnemy(Collider[] enemyCollider)
    {
        List<Collider> EnemyActif = new List<Collider>();
        foreach (Collider enemy in enemyCollider)
        {
            if(enemy.gameObject.activeSelf == true)
                EnemyActif.Add(enemy);
        }

        if (EnemyActif == null || EnemyActif.Count == 0)
            return null;

        // Cas extrêmes
        if (targetPriority <= 0)
            return EnemyActif[EnemyActif.Count - 1].gameObject;

        if (targetPriority >= 100)
            return EnemyActif[0].gameObject;

        float t = targetPriority / 100f;

        // Plus t est grand, plus on favorise les petits indices.
        float random = Mathf.Pow(Random.value, Mathf.Lerp(3f, 0.35f, t));

        int index = Mathf.RoundToInt(random * (EnemyActif.Count - 1));

        return EnemyActif[index].gameObject;
    }

    public virtual void Attack(Transform ship, Transform target)
    {
        GameObject missile = myPoolMissile.GetPoolObject();
        MissileHoming missileHoming = missile.GetComponent<MissileHoming>();
        missileHoming.target = target;
        missileHoming.damage = shipSO.tiers[currentTier].damage;

        missile.transform.position = ship.transform.position;
        missile.transform.rotation = ship.transform.rotation;
        
    }

    public virtual void SpawnUnits()
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

    public virtual void CreateShip()
    {
        //Faire une pool
        //GameObject newShips = Instantiate(shipSO.tiers[currentTier].ship, transform.position, transform.rotation, _groupShip);
        GameObject newShip = myPoolShips.GetPoolObject();
        newShip.transform.position = transform.position;
        newShip.transform.rotation = transform.rotation;
        unitsList.Add(newShip);
        isCreatingShip = false;
    }

    public virtual void RemoveShip(GameObject ship)
    {
        unitsList.Remove(ship);
        myPoolShips.ReturnPool(ship);
    }
}
