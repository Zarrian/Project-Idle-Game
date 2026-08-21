using UnityEngine;

public class WeaponCanon : Hangar
{
    public CanonTierSet canonSO;
    public CannonPlacementManager placementManager;

    private void OnEnable()
    {
        //Place les canons
        unitsList = placementManager.CreateCannonsGrid(canonSO.tiers[currentTier].maxUnits, canonSO.tiers[currentTier].canon);
    }
}
