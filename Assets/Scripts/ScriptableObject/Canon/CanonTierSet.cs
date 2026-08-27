using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Config data-only : la liste des tiers d'unités (10 max). Clic droit dans
/// le Project > Create > Data > Unit Tier Set pour en créer un.
/// </summary>
[CreateAssetMenu(fileName = "NewCanonTierSet", menuName = "Data/Canon Tier Set")]
public class CanonTierSet : ScriptableObject
{
    public const int MaxTierCount = 10;

    public List<CanonTier> tiers = new List<CanonTier>();
    /// <summary>Récupère un tier par index, ou null si hors limites.</summary>
    public CanonTier GetTier(int index)
    {
        if (index < 0 || index >= tiers.Count) return null;
        return tiers[index];
    }

    // Contraint la liste entre 1 et 10 tiers depuis l'Inspector.
    void OnValidate()
    {
        if (tiers.Count > MaxTierCount)
        {
            tiers.RemoveRange(MaxTierCount, tiers.Count - MaxTierCount);
        }
        else if (tiers.Count == 0)
        {
            tiers.Add(new CanonTier());
        }
    }
}
