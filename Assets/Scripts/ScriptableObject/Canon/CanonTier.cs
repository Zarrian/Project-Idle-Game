using UnityEngine;

[CreateAssetMenu(fileName = "CanonUnit", menuName = "Data/Canon")]
public class CanonTier : ScriptableObject
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

    /// <summary>
    /// Copie toutes les valeurs de "source" dans cette instance, sans créer
    /// de nouveau ScriptableObject. Utile pour mettre à jour un "canon" runtime
    /// déjà existant (ex: après un changement de tier) sans casser les
    /// références qui pointent dessus.
    /// </summary>
    public void CopyFrom(CanonTier source)
    {
        tierName = source.tierName;
        canon = source.canon;
        maxUnits = source.maxUnits;
        cdSpawnUnits = source.cdSpawnUnits;

        // Stats Combats
        pv = source.pv;
        damage = source.damage;
        cdAttack = source.cdAttack;
        rangeAttack = source.rangeAttack;
        nbAttack = source.nbAttack;

        planetLayerMask = source.planetLayerMask;
        shipLayerMask = source.shipLayerMask;
    }

}

