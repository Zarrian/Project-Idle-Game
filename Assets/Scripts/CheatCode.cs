using TMPro;
using UnityEngine;

public class CheatCode : MonoBehaviour
{
    public Weapon[] weapons;

    public TextMeshProUGUI TierText;
    public int currentTier = 1;
    public int maxTier;

    private void Awake()
    {
        weapons = Object.FindObjectsByType<Weapon>(FindObjectsSortMode.None);

        foreach (Weapon item in weapons)
        {
            item.currentTier = currentTier;
        }
        TierText.text = "Current Tier: " + currentTier;
    }

    public void IncreaseTier()
    {
        currentTier++;
        currentTier = Mathf.Clamp(currentTier, 0, maxTier);

        TierText.text = "Current Tier: " + currentTier;
        foreach (Weapon item in weapons)
        {
            item.currentTier = currentTier;
        }
    }

    public void DecreaseTier()
    {
        currentTier--;
        currentTier = Mathf.Clamp(currentTier, 0, maxTier);
        TierText.text = "Current Tier: " + currentTier;
        foreach (Weapon item in weapons)
        {
            item.currentTier = currentTier;
        }
    }

    public void AddMetal()
    {
        DeathStar.instance.AddRessources(DeathStar.Ressources.Metal, 100f);
    }

    public void AddElectricity()
    {
        DeathStar.instance.AddRessources(DeathStar.Ressources.Electricity, 100f);
    }

    public void AddUranium()
    {
        DeathStar.instance.AddRessources(DeathStar.Ressources.Uranium, 100f);
    }
}
