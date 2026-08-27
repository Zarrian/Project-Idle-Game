using System.Collections.Generic;
using UnityEngine;

public class LootDroper : MonoBehaviour
{
    [System.Serializable]
    public class LootEntry
    {
        public DeathStar.Ressources ressource;
        [Tooltip("Poids relatif : plus il est haut, plus la ressource a de chance de sortir")]
        public float weight = 1f;
        [Tooltip("Valeur min/max de la ressource droppée")]
        public Vector2Int valueRange = new Vector2Int(1, 5);
    }

    [Header("Loot Table")]
    [SerializeField] private List<LootEntry> entries = new List<LootEntry>();

    [Range(0f, 1f)]
    [Tooltip("Chance globale qu'un drop ait lieu (1 = toujours un drop)")]
    [SerializeField] private float dropChance = 1f;

    [Header("Pool")]
    [SerializeField] private Pool scrapPool;
    [SerializeField] private Ship ship;

    private void Awake()
    {
        if (ship == null)
            ship = GetComponent<Ship>();

        ship.OnDeath.AddListener(DropLoot);
    }

    public void DropLoot()
    {
        if (scrapPool == null || entries.Count == 0) return;
        if (Random.value > dropChance) return; // pas de drop cette fois-ci

        if (!TryGetRandomDrop(out DeathStar.Ressources ressource, out int value))
            return;

        GameObject scrapObj = scrapPool.GetPoolObject(); // adapte au nom réel dans ton Pool.cs
        if (scrapObj == null) return;

        scrapObj.transform.position = transform.position;

        Scrap scrap = scrapObj.GetComponent<Scrap>();
        scrap.myPool = scrapPool;
        scrap.myRessource = ressource;
        scrap.value = value;

        scrap.SetEsthetics(); // déclenche OnEnable() qui gère déjà l'affichage visuel

        //Faire que la taille dépends du nombre ?
    }

    private bool TryGetRandomDrop(out DeathStar.Ressources ressource, out int value)
    {
        ressource = default;
        value = 0;

        float totalWeight = 0f;
        foreach (var entry in entries)
            totalWeight += entry.weight;

        if (totalWeight <= 0f) return false;

        float roll = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        foreach (var entry in entries)
        {
            cumulative += entry.weight;
            if (roll <= cumulative)
            {
                ressource = entry.ressource;
                value = Random.Range(entry.valueRange.x, entry.valueRange.y + 1); // +1 car max exclusif
                return true;
            }
        }

        return false;
    }
}