using TMPro;
using UnityEngine;

public class CheatCode : MonoBehaviour
{
    public Hangar[] weapons;

    public TextMeshProUGUI TierText;
    public TextMeshProUGUI TimeText;
    public int currentTier = 1;
    public int maxTier;

    public bool dontUpdatePlayerShip;

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
        if (dontUpdatePlayerShip == true)
        {
            currentTier++;
            currentTier = Mathf.Clamp(currentTier, 0, maxTier);

            TierText.text = "Current Tier: " + currentTier;
            foreach (Hangar item in weapons)
            {
                if (item.GetComponent<SpawnerEnemy>() == true)
                {
                    item.currentTier = currentTier;
                    item.SetTier();
                }
            }
        }

        else
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

    }

    public void DecreaseTier()
    {
        if (dontUpdatePlayerShip == true)
        {
            currentTier--;
            currentTier = Mathf.Clamp(currentTier, 0, maxTier);
            TierText.text = "Current Tier: " + currentTier;
            foreach (Hangar item in weapons)
            {
                if (item.GetComponent<SpawnerEnemy>() == true)
                {
                    item.currentTier = currentTier;
                    item.SetTier();
                }
            }
        }
        else
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

    }

    public void IncreaseTime()
    {
        Time.timeScale += 0.2f;
        Time.timeScale = Mathf.Clamp(Time.timeScale, 0f, 10f);
        TimeText.text = "Current Time: " + Time.timeScale.ToString("F1");
    }

    public void DecreaseTime()
    {
        Time.timeScale -= 0.2f;
        Time.timeScale = Mathf.Clamp(Time.timeScale, 0f, 10f);
        TimeText.text = "Current Time: " + Time.timeScale.ToString("F1");
    }

    public void AddMetal()
    {
        DeathStar.instance.ChangeRessources(DeathStar.Ressources.Metal, 1000000f);
    }

    public void AddElectricity()
    {
        DeathStar.instance.ChangeRessources(DeathStar.Ressources.Electricity, 1000000f);
    }

    public void AddUranium()
    {
        DeathStar.instance.ChangeRessources(DeathStar.Ressources.Uranium, 1000000f);
    }
}
