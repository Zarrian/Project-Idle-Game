using System.Collections.Generic;
using UnityEngine;



public class UnitTierSet : ScriptableObject
{
    public const int MaxTierCount = 10;

    public List<UnitTier> tiers = new List<UnitTier>();

    /// <summary>Récupère un tier par index, ou null si hors limites.</summary>
    public UnitTier GetTier(int index)
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
            tiers.Add(new UnitTier());
        }
    }
}
