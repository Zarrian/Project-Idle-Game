using UnityEngine;

public class WeaponCanon : Hangar
{
    public CanonTierSet canonSO;
    public CanonTier canon;
    public CannonPlacementManager placementManager;

    private void OnEnable()
    {
        canon.CopyFrom(canonSO.tiers[currentTier]);
        //Place les canons
        unitsList = placementManager.CreateCannonsGrid(canon.maxUnits, canon.canon);
    }
}
