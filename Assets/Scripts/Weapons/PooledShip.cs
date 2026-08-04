using UnityEngine;

/// <summary>
/// Ajouté automatiquement par WeaponShip sur chaque ship instancié.
/// Permet au ship de se rendre au pool depuis n'importe où (ex: son propre
/// script de vie/mort), sans connaître les détails du WeaponShip qui l'a créé.
/// </summary>
public class PooledShip : MonoBehaviour
{
    public WeaponShip OwnerWeapon { get; set; }

    /// <summary>À appeler à la place de Destroy(gameObject).</summary>
    public void ReturnToPool()
    {
        //OwnerWeapon?.ReturnShip(gameObject);
    }
}
