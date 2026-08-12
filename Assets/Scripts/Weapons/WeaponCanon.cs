using UnityEngine;

public class WeaponCanon : Weapon
{
    public CanonTierSet canonSO;
    public CannonPlacementManager placementManager;

    private void Awake()
    {
        //Place les canons
        unitsList = placementManager.CreateCannonsGrid(canonSO.tiers[currentTier].maxUnits, canonSO.tiers[currentTier].canon);
    }
}
