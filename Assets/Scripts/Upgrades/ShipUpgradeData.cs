using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Un seul palier d'amélioration pour une stat (ex: HP niveau 3 = 18,
/// coûte 120 Métal / 5 Électricité / 0 Uranium).
/// Correspond à une ligne du CSV exporté depuis le Google Sheet.
/// </summary>
[System.Serializable]
public class StatUpgradeStep
{
    public int level;
    public float value;

    public int costMetal;
    public int costElectricity;
    public int costUranium;
}

/// <summary>
/// Une catégorie de stat améliorable (ex: "HP", "Damage", "CD Attack",
/// "Attack") avec tous ses paliers, du niveau 1 au niveau max.
/// </summary>
[System.Serializable]
public class StatUpgradeTrack
{
    public string statName;
    public List<StatUpgradeStep> steps = new List<StatUpgradeStep>();

    /// <summary>Palier pour un niveau donné, ou null si absent.</summary>
    public StatUpgradeStep GetStep(int level)
    {
        foreach (var step in steps)
        {
            if (step.level == level)
                return step;
        }
        return null;
    }

    /// <summary>Niveau max disponible pour cette stat.</summary>
    public int GetMaxLevel()
    {
        int max = 0;
        foreach (var step in steps)
        {
            if (step.level > max)
                max = step.level;
        }
        return max;
    }
}

/// <summary>
/// Toutes les améliorations d'un vaisseau, organisées par catégorie de stat
/// (une "Track" par stat, chacune avec ses propres paliers). Généré
/// automatiquement depuis un CSV via l'outil Tools/Upgrades/Import CSV,
/// un ScriptableObject par vaisseau (ex: Charlemagne, Vagabond, Drone...).
/// </summary>
[CreateAssetMenu(fileName = "NewShipUpgradeData", menuName = "Data/Ship Upgrade Data")]
public class ShipUpgradeData : ScriptableObject
{
    [Tooltip("Nom du vaisseau, rempli automatiquement depuis le nom du fichier CSV")]
    public string shipName;

    [Tooltip("Une catégorie par stat améliorable (HP, Damage, CD Attack, Attack...), chacune avec ses paliers")]
    public List<StatUpgradeTrack> stats = new List<StatUpgradeTrack>();

    /// <summary>Récupère la track d'une stat par son nom, ou null si absente.</summary>
    public StatUpgradeTrack GetTrack(string statName)
    {
        foreach (var track in stats)
        {
            if (track.statName == statName)
                return track;
        }
        return null;
    }

    /// <summary>Liste des noms de stats disponibles sur ce vaisseau (ex: "HP", "Damage"...).</summary>
    public IEnumerable<string> GetStatNames()
    {
        foreach (var track in stats)
            yield return track.statName;
    }

    /// <summary>Récupère le palier d'une stat à un niveau donné, ou null si absent.</summary>
    public StatUpgradeStep GetStep(string statName, int level)
    {
        return GetTrack(statName)?.GetStep(level);
    }

    /// <summary>Niveau max disponible pour une stat donnée.</summary>
    public int GetMaxLevel(string statName)
    {
        return GetTrack(statName)?.GetMaxLevel() ?? 0;
    }
}
