using Project.UI;
using UnityEngine;

public class UpgradeShip : Upgrades
{
    public UnitTier unit;
    public ProfileUIShip profilUIShip;

    public override void GeneratePanels()
    {
        base.GeneratePanels();

        panelUpgrade = ManagerPanelUpgrade.instance.GeneratePanelUpgrade();
        panelUpgrade.unit = unit;

        profilUIShip = TacticalPanelUI.instance.InstantiateUIPanel(hangar);
        profilUIShip.panelUpgrade = panelUpgrade;
    }
}
