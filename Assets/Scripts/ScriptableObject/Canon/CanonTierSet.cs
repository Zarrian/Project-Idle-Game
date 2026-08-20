using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Un seul tier de la configuration : un groupe d'unités avec ses propres
/// réglages de spawn/attaque.
/// </summary>
[System.Serializable]
public class CanonTier
{
    [Tooltip("Nom libre pour s'y retrouver dans l'Inspector, ex: 'Tier 1 - Basique'")]
    public string tierName = "Nouveau tier";

    public GameObject canon;
    //public GameObject attack;
    public int maxUnits;
    public float cdSpawnUnits;

    [Header("Stats Combats")]
    public float pv;
    public float damage;
    public float cdAttack;
    public float rangeAttack;
    public int nbAttack = 1;

    public LayerMask planetLayerMask;
    public LayerMask shipLayerMask;

}

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
