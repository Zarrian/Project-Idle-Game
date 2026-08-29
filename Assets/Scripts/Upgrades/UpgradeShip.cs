using Project.UI;
using UnityEngine;

public class UpgradeShip : Upgrades
{
    public UnitTier unit;
    public ShipUpgradeData shipUpgradeData;
    public ProfileUIShip profilUIShip;

    protected override void Start()
    {
        base.Start();
        unit = hangar.unit;
    }

    public void TryUpgradeStats(string statName)
    {
        // Exemple d'amélioration de la stat "damage"
        TryUpgradeStat(statName);
    }

    public override void GeneratePanels()
    {
        base.GeneratePanels();

        panelUpgrade = ManagerPanelUpgrade.instance.GeneratePanelUpgrade();
        panelUpgrade.unit = unit;

        profilUIShip = TacticalPanelUI.instance.InstantiateUIPanel(hangar);
        profilUIShip.panelUpgrade = panelUpgrade;
    }

    /// <summary>
    /// Tente d'améliorer une stat du vaisseau. Vérifie les ressources disponibles
    /// et applique l'amélioration si suffisant.
    /// </summary>
    /// <param name="statName">Nom de la stat à améliorer (ex: "damage", "pv", "maxSpeed")</param>
    /// <returns>true si l'amélioration a réussi, false sinon</returns>
    public bool TryUpgradeStat(string statName)
    {
        if (shipUpgradeData == null)
        {
            Debug.LogError("ShipUpgradeData manquant !", gameObject);
            return false;
        }

        // Récupère le niveau actuel
        int currentLevel = unit.GetCurrentStatLevel(statName);
        int nextLevel = currentLevel + 1;

        // Récupère le palier suivant
        var nextStep = shipUpgradeData.GetStep(statName, nextLevel);
        if (nextStep == null)
        {
            Debug.LogWarning($"Stat '{statName}' est déjà au niveau maximum ({currentLevel})", gameObject);
            return false;
        }
        print(nextStep);

        // Vérifie les ressources disponibles
        if (!CanAffordUpgrade(nextStep))
        {
            Debug.Log($"Ressources insuffisantes pour améliorer '{statName}' au niveau {nextLevel}");
            return false;
        }

        // Déduit les ressources
        DeductResources(nextStep);

        // Applique l'amélioration directement au ScriptableObject UnitTier
        unit.ApplyUpgrade(statName, nextStep.value);
        unit.SetCurrentUpgradeLevel(statName, nextLevel);

        Debug.Log($"'{statName}' amélioré au niveau {nextLevel}");
        return true;
    }

    /// <summary>
    /// Vérifie si les ressources sont suffisantes pour une amélioration
    /// </summary>
    private bool CanAffordUpgrade(StatUpgradeStep step)
    {
        var deathStar = DeathStar.instance;
        if (deathStar == null)
        {
            Debug.LogError("DeathStar instance non trouvée !");
            return false;
        }

        bool canAfford =
            deathStar.GetAmount(DeathStar.Ressources.Metal) >= step.costMetal &&
            deathStar.GetAmount(DeathStar.Ressources.Electricity) >= step.costElectricity &&
            deathStar.GetAmount(DeathStar.Ressources.Uranium) >= step.costUranium;

        return canAfford;
    }

    /// <summary>
    /// Déduit les ressources nécessaires pour l'amélioration
    /// </summary>
    private void DeductResources(StatUpgradeStep step)
    {
        var deathStar = DeathStar.instance;
        if (deathStar != null)
        {
            deathStar.ChangeRessources(DeathStar.Ressources.Metal, -step.costMetal);
            deathStar.ChangeRessources(DeathStar.Ressources.Electricity, -step.costElectricity);
            deathStar.ChangeRessources(DeathStar.Ressources.Uranium, -step.costUranium);
        }
    }

}
