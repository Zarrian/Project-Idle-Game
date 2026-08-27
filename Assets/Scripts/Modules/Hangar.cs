using System.Collections.Generic;
using UnityEngine;

public class Hangar : MonoBehaviour
{
    //public UnitTier weaponSO;

    public List<GameObject> unitsList;
    public List<MovementPhysic> movements;

    public int currentTier;

    public virtual void SetTier()
    {
        //Copie les valeurs du bon scriptableObject
        //unit.CopyFrom(shipSO.tiers[currentTier]);
    }

    //public int maxUnits;
    //public int currentUnits;
    //
    //public float cdSpawnUnits;
    //public float cdAttack;

}

