using UnityEngine;


/// <summary>
/// Config data-only : la liste des tiers d'unités (10 max). Clic droit dans
/// le Project > Create > Data > Unit Tier Set pour en créer un.
/// </summary>
[CreateAssetMenu(fileName = "NewUnitShip", menuName = "Data/Unit")]
public class UnitTier : ScriptableObject
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

    [Header("Propulsion")]
    [Tooltip("Force de poussée constante vers l'avant")]
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


    /// <summary>
    /// Crée une copie indépendante de ce tier (pour une instance runtime
    /// modifiable par les améliorations, sans toucher à l'asset de base).
    /// Tous les champs sont soit des types valeur, soit des références
    /// qu'on veut justement partager (prefab du vaisseau, sphereCenter...) :
    /// un MemberwiseClone suffit, pas besoin de copie profonde champ par champ.
    /// </summary>
    /// <summary>
    /// Copie toutes les valeurs de "source" dans cette instance, sans créer
    /// de nouveau ScriptableObject. Utile pour mettre à jour un "unit" runtime
    /// déjà existant (ex: après un changement de tier) sans casser les
    /// références qui pointent dessus.
    /// </summary>
    public void CopyFrom(UnitTier source)
    {
        tierName = source.tierName;
        ship = source.ship;
        maxUnits = source.maxUnits;
        cdSpawnUnits = source.cdSpawnUnits;

        // Stats Combats
        pv = source.pv;
        damage = source.damage;
        cdAttack = source.cdAttack;
        rangeAttack = source.rangeAttack;
        nbAttack = source.nbAttack;

        // Movement
        sphereCenter = source.sphereCenter;
        orbitRadiusMin = source.orbitRadiusMin;
        orbitRadiusMax = source.orbitRadiusMax;

        // Propulsion
        thrust = source.thrust;
        maxSpeed = source.maxSpeed;
        maxTurnRateDegPerSec = source.maxTurnRateDegPerSec;
        maxBankAngle = source.maxBankAngle;
        bankLerpSpeed = source.bankLerpSpeed;
        rotationFollowSpeed = source.rotationFollowSpeed;

        // Comportement de vol
        targetReachedDistance = source.targetReachedDistance;
        maxTimeOnTarget = source.maxTimeOnTarget;

        // Évitement — Planète
        planetLayerMask = source.planetLayerMask;
        planetDetectionDistance = source.planetDetectionDistance;
        planetCastRadius = source.planetCastRadius;

        // Évitement — Autres vaisseaux
        shipLayerMask = source.shipLayerMask;
        shipDetectionRadius = source.shipDetectionRadius;
        avoidanceForce = source.avoidanceForce;
    }

}

