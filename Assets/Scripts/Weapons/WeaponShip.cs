using FunctionUseful;
using System.Collections;
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

    [SerializeField, Range(0, 100)]
    private float targetPriority = 75f;

    private const float DETECTION_CHECK_INTERVAL = 0f; // Vérifier tous les 100ms
    private float detectionCheckTimer;
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


/*        detectionCheckTimer += Time.fixedDeltaTime;
        if (detectionCheckTimer < DETECTION_CHECK_INTERVAL)
            return;

        foreach (MovementPhysic ship in movements)
        {
            ship.MovementPhysicUpdate();
        }
        detectionCheckTimer = 0f;*/

    }


    public virtual void CheckAttack()
    {
        for (int i = 0; i < shipSO.tiers[currentTier].nbAttack; i++)
        {
            //Choisis un vaisseaux joueurs random
            GameObject randomShip = unitsList[Random.Range(0, unitsList.Count)];
            //CheckEnemyInrange

            Transform target = FunctionUsefullManager.FindTarget(randomShip.transform, invaderLayer, targetPriority);

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
        movements.Add(newShip.GetComponent<MovementPhysic>());
        isCreatingShip = false;
    }

    public virtual void RemoveShip(GameObject ship)
    {
        unitsList.Remove(ship);
        movements.Remove(ship.GetComponent<MovementPhysic>());
        myPoolShips.ReturnPool(ship);
    }
}
