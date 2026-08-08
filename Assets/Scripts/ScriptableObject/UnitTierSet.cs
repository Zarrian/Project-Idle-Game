using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Un seul tier de la configuration : un groupe d'unités avec ses propres
/// réglages de spawn/attaque.
/// </summary>
[System.Serializable]
public class UnitTier
{
    [Tooltip("Nom libre pour s'y retrouver dans l'Inspector, ex: 'Tier 1 - Basique'")]
    public string tierName = "Nouveau tier";

    //public List<GameObject> unitsList = new List<GameObject>();

    public GameObject ship;
    //public GameObject attack;
    public int maxUnits;
    public float cdSpawnUnits;


    [Header("Stats Combats")]
    public float pv;
    public float damage;
    public float cdAttack;
    public float rangeAttack;
    public int nbAttack = 1;

    [Header("Movement")]
    [Tooltip("Centre de la sphère autour de laquelle on tourne")]
    public Transform sphereCenter;

    [Tooltip("Distance min/max au centre à laquelle on choisit les points cibles")]
    public float orbitRadiusMin = 15f;
    public float orbitRadiusMax = 25f;

    [Header("Propulsion")]    [Tooltip("Force de poussée constante vers l'avant")]
    public float thrust = 20f;

    [Tooltip("Vitesse max (limitée via le drag du Rigidbody, voir Start)")]
    public float maxSpeed = 12f;


    [Tooltip("Vitesse à laquelle le CAP DE VOL (invisible, pas la rotation visuelle) se courbe vers la cible, en degrés/seconde. Plus c'est bas, plus les courbes sont larges et lentes.")]
    public float maxTurnRateDegPerSec = 25f;

    [Tooltip("Inclinaison en roulis dans les virages, purement visuel")]
    public float maxBankAngle = 45f;
    public float bankLerpSpeed = 2f;

    [Tooltip("Vitesse à laquelle la rotation visuelle rattrape la vélocité réelle. Haut = colle presque instantanément à la vélocité, bas = léger flottement/inertie visuelle.")]
    public float rotationFollowSpeed = 8f;

    [Header("Comportement de vol")]
    [Tooltip("Distance à laquelle on considère la cible atteinte et on en choisit une nouvelle")]
    public float targetReachedDistance = 4f;

    [Tooltip("Temps max avant de forcer un changement de cible même si pas atteinte (évite les boucles infinies)")]
    public float maxTimeOnTarget = 20f;

    [Header("Évitement — Planète")]
    [Tooltip("Layer(s) sur lesquels se trouve le collider de la planète")]
    public LayerMask planetLayerMask;

    [Tooltip("Distance de détection du SphereCast envoyé devant le vaisseau")]
    public float planetDetectionDistance = 15f;

    [Tooltip("Rayon du SphereCast, à peu près la taille du vaisseau + une marge")]
    public float planetCastRadius = 1.5f;

    [Header("Évitement — Autres vaisseaux")]
    [Tooltip("Layer(s) sur lesquels se trouvent les autres vaisseaux détectables")]
    public LayerMask shipLayerMask;

    [Tooltip("Rayon dans lequel on détecte les vaisseaux voisins")]
    public float shipDetectionRadius = 8f;

    [Tooltip("Intensité de la force d'esquive ajoutée par-dessus la poussée normale")]
    public float avoidanceForce = 60f;

}

/// <summary>
/// Config data-only : la liste des tiers d'unités (10 max). Clic droit dans
/// le Project > Create > Data > Unit Tier Set pour en créer un.
/// </summary>
[CreateAssetMenu(fileName = "NewUnitTierSet", menuName = "Data/Unit Tier Set")]
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
