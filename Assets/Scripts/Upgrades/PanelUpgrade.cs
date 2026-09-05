using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PanelUpgrade : MonoBehaviour
{
    public UnitTier unit;
    public ShipUpgradeData shipUpgradeData;
    public UpgradeShip upgradeShip;

    public Image icone;
    public TextMeshProUGUI nameShip;

    public GameObject lineStats;
    public Transform content;

    /// <summary>Stocke le niveau actuel d'amélioration pour chaque stat</summary>
    private Dictionary<string, int> currentUpgradeLevels = new Dictionary<string, int>();

    /// <summary>Garde une référence des lignes instantiées pour pouvoir les mettre à jour</summary>
    private List<LineUpgrade> spawnedLines = new List<LineUpgrade>();


    public void SetInfo()
    {
        icone.sprite = unit.uiSprite;
        nameShip.text = unit.tierName;
    }

    /// <summary>
    /// Instantie autant de LineUpgrade que de stats améliorables présentes
    /// dans le ShipUpgradeData, et remplit chaque ligne avec les infos
    /// correspondantes (niveau actuel, valeur actuelle/suivante, coûts).
    /// </summary>
    public void SetStatsLine()
    {
        // Nettoie les lignes précédentes avant d'en recréer
        ClearStatsLines();

        foreach (var statName in shipUpgradeData.GetStatNames())
        {
            // Récupère (ou initialise) le niveau actuel pour cette stat
            if (!currentUpgradeLevels.TryGetValue(statName, out int currentLevel))
            {
                currentLevel = 1;
                currentUpgradeLevels[statName] = currentLevel;
            }

            GameObject lineGO = Instantiate(lineStats, content);
            lineGO.SetActive(true);

            LineUpgrade line = lineGO.GetComponent<LineUpgrade>();
            line.upgradeShip = upgradeShip;
            line.shipUpgradeData = shipUpgradeData;
            line.statId = statName;

            line.SetLineInfo();
            spawnedLines.Add(line);
        }
    }

    /// <summary>Détruit toutes les lignes précédemment instantiées.</summary>
    private void ClearStatsLines()
    {
        foreach (var line in spawnedLines)
        {
            if (line != null)
                Destroy(line.gameObject);
        }
        spawnedLines.Clear();
    }

    public void OpenPanel()
    {
        gameObject.SetActive(true);
        SetInfo();
        SetStatsLine();
    }

    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }

    public void InteractPanel()
    {
        if (gameObject.activeSelf)
            ClosePanel();
        else
            OpenPanel();
    }

}
