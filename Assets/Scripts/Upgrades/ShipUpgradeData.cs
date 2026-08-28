using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Un seul niveau d'amélioration pour une stat donnée (ex: HP niveau 3 = 18,
/// coûte 120 Métal / 5 Électricité / 0 Uranium).
/// Correspond à une ligne du CSV exporté depuis le Google Sheet.
/// </summary>
[System.Serializable]
public class StatUpgradeLevel
{
    public int level;
    public string statName;
    public float value;

    public int costMetal;
    public int costElectricity;
    public int costUranium;
}

/// <summary>
/// Toutes les améliorations d'un vaisseau (une stat -> la liste de ses
/// niveaux). Généré automatiquement depuis un CSV via l'outil
/// Tools/Upgrades/Import CSV, un ScriptableObject par vaisseau
/// (ex: Charlemagne, Vagabond, Drone...).
/// </summary>
[CreateAssetMenu(fileName = "NewShipUpgradeData", menuName = "Data/Ship Upgrade Data")]
public class ShipUpgradeData : ScriptableObject
{
    [Tooltip("Nom du vaisseau, rempli automatiquement depuis le nom du fichier CSV")]
    public string shipName;

    [Tooltip("Toutes les lignes d'amélioration importées du CSV, triées par stat puis niveau")]
    public List<StatUpgradeLevel> levels = new List<StatUpgradeLevel>();

    /// <summary>Liste des noms de stats distincts présents dans ce set (ex: "HP", "Damage"...).</summary>
    public IEnumerable<string> GetStatNames()
    {
        HashSet<string> seen = new HashSet<string>();
        foreach (var lvl in levels)
        {
            if (seen.Add(lvl.statName))
                yield return lvl.statName;
        }
    }

    /// <summary>Récupère la ligne d'amélioration pour une stat à un niveau donné, ou null si absente.</summary>
    public StatUpgradeLevel GetLevel(string statName, int level)
    {
        foreach (var lvl in levels)
        {
            if (lvl.level == level && lvl.statName == statName)
                return lvl;
        }
        return null;
    }

    /// <summary>Niveau max disponible pour une stat donnée.</summary>
    public int GetMaxLevel(string statName)
    {
        int max = 0;
        foreach (var lvl in levels)
        {
            if (lvl.statName == statName && lvl.level > max)
                max = lvl.level;
        }
        return max;
    }
}
