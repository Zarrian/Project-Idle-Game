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

    public override void SetTier()
    {
        base.SetTier();

        canon.CopyFrom(canonSO.tiers[currentTier]);

        if (unitsList != null && unitsList.Count > 0)
        {
            placementManager.ClearCannons(unitsList);
            unitsList.Clear();
        }

        unitsList = placementManager.CreateCannonsGrid(
            canon.maxUnits,
            canon.canon
        );
    }
}
