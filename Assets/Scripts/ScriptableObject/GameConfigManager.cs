using UnityEngine;

/// <summary>
/// Détient la copie "runtime" de la config (celle qu'on modifie via les
/// améliorations) et garde l'asset de base intact.
///
/// Placer ce script sur un GameObject unique de ta scène de démarrage
/// (ou en DontDestroyOnLoad si tu changes de scène en cours de partie).
/// </summary>
public class GameConfigManager : MonoBehaviour
{
    public static GameConfigManager Instance { get; private set; }

    [Tooltip("L'asset de base (design), JAMAIS modifié directement")]
    public UnitTierSet baseConfig;

    [Tooltip("La copie runtime, modifiée par les améliorations")]
    public UnitTierSet RuntimeConfig { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        CreateRuntimeConfig();
    }

    /// <summary>
    /// Crée (ou recrée) la copie runtime à partir de l'asset de base.
    /// Utile aussi pour un mécanisme de "prestige" / reset de partie :
    /// rappeler cette méthode efface toutes les améliorations d'un coup.
    /// </summary>
    public void CreateRuntimeConfig()
    {
        // Si on avait déjà une instance runtime (ex: reset de partie),
        // on la détruit proprement avant d'en recréer une.
        if (RuntimeConfig != null)
            Destroy(RuntimeConfig);

        // La commande clé : Instantiate() clone l'asset au lieu de le modifier.
        RuntimeConfig = Instantiate(baseConfig);
    }

    /// <summary>Raccourci pratique pour récupérer un tier par index.</summary>
    public UnitTier GetTier(int index)
    {
        return RuntimeConfig != null ? RuntimeConfig.GetTier(index) : null;
    }

    // --- Exemples d'améliorations ---------------------------------------

    /// <summary>Augmente les dégâts d'un tier d'une valeur donnée.</summary>
    public void UpgradeDamage(int tierIndex, float amount)
    {
        UnitTier tier = GetTier(tierIndex);
        if (tier == null) return;

        tier.damage += amount;
    }

    /// <summary>Augmente le nombre max d'unités d'un tier.</summary>
    public void UpgradeMaxUnits(int tierIndex, int amount)
    {
        UnitTier tier = GetTier(tierIndex);
        if (tier == null) return;

        tier.maxUnits += amount;
    }

    /// <summary>Réduit le cooldown de spawn d'un tier (avec un plancher).</summary>
    public void UpgradeSpawnSpeed(int tierIndex, float reduction, float minCd = 0.1f)
    {
        UnitTier tier = GetTier(tierIndex);
        if (tier == null) return;

        tier.cdSpawnUnits = Mathf.Max(minCd, tier.cdSpawnUnits - reduction);
    }

    private void OnDestroy()
    {
        if (RuntimeConfig != null)
            Destroy(RuntimeConfig);
    }
}
