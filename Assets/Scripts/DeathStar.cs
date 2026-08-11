using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DeathStar : MonoBehaviour
{
    public static DeathStar instance;

    public List<List<GameObject>> listWeapons;

    public enum Ressources { Metal, Electricity, Uranium }

    /// <summary>Plafond de stockage par ressource, visible et réglable dans l'Inspector.</summary>
    [System.Serializable]
    public class ResourceCap
    {
        public Ressources type;
        public float maxAmount = 1000f;
    }

    [Tooltip("Un plafond par type de ressource. Le montant actuel, lui, n'est pas visible dans l'Inspector (voir GetAmount).")]
    public List<ResourceCap> resourceCaps;


    public UnityEvent<Ressources, float> onResourceChanged;

    private Dictionary<Ressources, float> currentAmounts;
    private Dictionary<Ressources, float> maxAmounts;

    private void Awake()
    {
        instance = this;
        BuildDictionaries();
    }

    /// <summary>
    /// Construit les deux Dictionary à partir de l'enum (valeurs à 0) et de
    /// la List resourceCaps assignée dans l'Inspector (pour les plafonds).
    /// </summary>
    private void BuildDictionaries()
    {
        currentAmounts = new Dictionary<Ressources, float>();
        maxAmounts = new Dictionary<Ressources, float>();

        foreach (Ressources type in System.Enum.GetValues(typeof(Ressources)))
        {
            currentAmounts[type] = 0f;
            maxAmounts[type] = float.MaxValue; // pas de plafond tant qu'aucun ResourceCap n'est défini pour ce type
        }

        foreach (ResourceCap cap in resourceCaps)
        {
            maxAmounts[cap.type] = cap.maxAmount;
        }
    }

    /// <summary>
    /// Ajoute (ou retire, avec un montant négatif) une quantité à une
    /// ressource, en respectant le plafond et le minimum de 0.
    /// </summary>
    public void AddRessources(Ressources type, float amount)
    {
        float newAmount = Mathf.Clamp(currentAmounts[type] + amount, 0f, maxAmounts[type]);
        currentAmounts[type] = newAmount;
        onResourceChanged?.Invoke(type, amount);
    }

    public void RemoveRessources(Ressources type, float amount)
    {
        float newAmount = Mathf.Clamp(currentAmounts[type] + amount, 0f, maxAmounts[type]);
        currentAmounts[type] = newAmount;
        onResourceChanged?.Invoke(type, amount);
    }

    public float GetAmount(Ressources type)
    {
        return currentAmounts[type];
    }

    public float GetMaxAmount(Ressources type)
    {
        return maxAmounts[type];
    }
}