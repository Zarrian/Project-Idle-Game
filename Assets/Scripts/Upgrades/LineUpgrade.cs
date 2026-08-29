using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LineUpgrade : MonoBehaviour
{

    public TextMeshProUGUI statsName;
    public TextMeshProUGUI levelActuelStats;
    public TextMeshProUGUI currentValue;
    public TextMeshProUGUI nextValue;

    public TextMeshProUGUI costMetal;
    public TextMeshProUGUI costElectricity;
    public TextMeshProUGUI costUranium;

    public Button button;
    public UpgradeShip upgradeShip;

    [Tooltip("Couleur utilisée quand la ressource est suffisante")]
    public Color affordableColor = Color.green;

    [Tooltip("Couleur utilisée quand la ressource est insuffisante")]
    public Color notAffordableColor = Color.red;

    [Tooltip("Couleur utilisée quand le coût de cette ressource est nul")]
    public Color noCostColor = Color.gray;

    /// <summary>Doit être assigné par PanelUpgrade avant d'appeler SetLineInfo().</summary>
    public ShipUpgradeData shipUpgradeData;

    [Tooltip("Identifiant de la stat affichée par cette ligne, ex: 'pv', 'damage'")]
    public string statId;

    /// <summary>Doit être assigné par PanelUpgrade avant d'appeler SetLineInfo().</summary>
    public int currentLevel;

    /// <summary>Coûts à afficher, préparés par RefreshCosts() puis appliqués par SetCostText().</summary>
    private readonly List<(TextMeshProUGUI text, int cost, float available)> pendingCosts = new List<(TextMeshProUGUI, int, float)>();

    private void OnEnable()
    {
        if (DeathStar.instance != null)
            DeathStar.instance.onResourceChanged.AddListener(HandleResourceChanged);

        button.onClick.AddListener(OnUpgradeButtonClicked);
    }

    private void OnDisable()
    {
        if (DeathStar.instance != null)
            DeathStar.instance.onResourceChanged.RemoveListener(HandleResourceChanged);

        button.onClick.RemoveListener(OnUpgradeButtonClicked);
    }

    public void OnUpgradeButtonClicked()
    {
        if (upgradeShip != null)
        {
            bool success = upgradeShip.TryUpgradeStat(statId);
            if (success)
            {
                // Relit le niveau réel après l'upgrade, plutôt que de réutiliser
                // l'ancienne valeur locale (c'était la cause du non-rafraîchissement).
                currentLevel = upgradeShip.GetCurrentStatLevel(statId);
                SetLineInfo();
            }
        }
    }

    /// <summary>
    /// Appelé chaque fois qu'une ressource change (peu importe laquelle) :
    /// on rafraîchit simplement l'affichage des coûts de cette ligne.
    /// </summary>
    private void HandleResourceChanged(DeathStar.Ressources type, float amount)
    {
        RefreshCosts();
        SetCostText();
    }

    /// <summary>
    /// Remplit la ligne avec les infos d'une stat : niveau actuel, valeur
    /// actuelle, valeur au prochain niveau, et coûts des ressources
    /// (colorés selon disponibilité). Utilise shipUpgradeData, statId et
    /// currentLevel, qui doivent être assignés avant l'appel.
    /// </summary>
    public void SetLineInfo()
    {
        if (shipUpgradeData == null)
            return;

        var currentStep = shipUpgradeData.GetStep(statId, currentLevel);
        var nextStep = shipUpgradeData.GetStep(statId, currentLevel + 1);

        // Niveau actuel
        levelActuelStats.text = currentLevel.ToString();
        statsName.text = statId;

        // Valeur actuelle
        currentValue.text = currentStep != null ? currentStep.value.ToString() : "-";

        // Si aucun palier suivant, niveau max atteint
        if (nextStep == null)
        {
            nextValue.text = "MAX";
            pendingCosts.Clear();
            pendingCosts.Add((costMetal, 0, 0));
            pendingCosts.Add((costElectricity, 0, 0));
            pendingCosts.Add((costUranium, 0, 0));
            SetCostText();
            return;
        }

        // Valeur au prochain niveau
        nextValue.text = nextStep.value.ToString();

        RefreshCosts();
        SetCostText();
    }

    /// <summary>
    /// Recalcule les coûts (metal/électricité/uranium) du prochain niveau,
    /// en comparant aux ressources actuellement disponibles, et les stocke
    /// dans pendingCosts en attendant l'affichage via SetCostText().
    /// </summary>
    private void RefreshCosts()
    {
        pendingCosts.Clear();

        if (shipUpgradeData == null)
            return;

        var nextStep = shipUpgradeData.GetStep(statId, currentLevel + 1);
        if (nextStep == null)
        {
            pendingCosts.Add((costMetal, 0, 0));
            pendingCosts.Add((costElectricity, 0, 0));
            pendingCosts.Add((costUranium, 0, 0));
            return;
        }

        var deathStar = DeathStar.instance;

        float availableMetal = deathStar != null ? deathStar.GetAmount(DeathStar.Ressources.Metal) : 0;
        float availableElectricity = deathStar != null ? deathStar.GetAmount(DeathStar.Ressources.Electricity) : 0;
        float availableUranium = deathStar != null ? deathStar.GetAmount(DeathStar.Ressources.Uranium) : 0;

        pendingCosts.Add((costMetal, nextStep.costMetal, availableMetal));
        pendingCosts.Add((costElectricity, nextStep.costElectricity, availableElectricity));
        pendingCosts.Add((costUranium, nextStep.costUranium, availableUranium));
    }

    /// <summary>
    /// Affiche les coûts préparés par RefreshCosts() : gris s'il n'y a pas
    /// de coût, vert si la ressource est suffisante, rouge sinon.
    /// </summary>
    private void SetCostText()
    {
        foreach (var (text, cost, available) in pendingCosts)
        {
            text.text = cost.ToString();

            if (cost <= 0)
                text.color = noCostColor;
            else if (available >= cost)
                text.color = affordableColor;
            else
                text.color = notAffordableColor;
        }
    }
}
