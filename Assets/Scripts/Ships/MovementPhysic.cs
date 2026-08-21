using UnityEngine;

public class MovementPhysic : MonoBehaviour
{
    [Header("Sphère de référence")]
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

    [Header("Maniabilité (l'essentiel de l'effet WWI)")]
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

    [Header("Évitement — Force")]
    [Tooltip("Intensité de la force d'esquive ajoutée par-dessus la poussée normale")]
    public float avoidanceForce = 60f;

    public virtual void MovementPhysicUpdate()
    {

    }
}
