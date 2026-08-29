using TMPro;
using UnityEngine;

public class CheatCode : MonoBehaviour
{
    public Hangar[] weapons;

    public TextMeshProUGUI TierText;
    public int currentTier = 1;
    public int maxTier;

    private void Awake()
    {
        weapons = Object.FindObjectsByType<Hangar>(FindObjectsSortMode.None);

        foreach (Hangar item in weapons)
        {
            item.currentTier = currentTier;
            item.SetTier();
        }
        TierText.text = "Current Tier: " + currentTier;
    }

    public void IncreaseTier()
    {
        currentTier++;
        currentTier = Mathf.Clamp(currentTier, 0, maxTier);

        TierText.text = "Current Tier: " + currentTier;
        foreach (Hangar item in weapons)
        {
            item.currentTier = currentTier;
            item.SetTier();
        }
    }

    public void DecreaseTier()
    {
        currentTier--;
        currentTier = Mathf.Clamp(currentTier, 0, maxTier);
        TierText.text = "Current Tier: " + currentTier;
        foreach (Hangar item in weapons)
        {
            item.currentTier = currentTier;
            item.SetTier();
        }
    }

    public void AddMetal()
    {
        DeathStar.instance.ChangeRessources(DeathStar.Ressources.Metal, 10000f);
    }

    public void AddElectricity()
    {
        DeathStar.instance.ChangeRessources(DeathStar.Ressources.Electricity, 10000f);
    }

    public void AddUranium()
    {
        DeathStar.instance.ChangeRessources(DeathStar.Ressources.Uranium, 10000f);
    }
}
