using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Pool : MonoBehaviour
{
    public UnityEvent OnObjectPool;
    public UnityEvent OnReturnPool;

    // Pile des objets DISPONIBLES (inactifs). Get() dépile, Return() empile.
    // O(1) dans les deux cas, contrairement à un scan de list à chaque appel.
    private readonly Stack<GameObject> availableObjects = new Stack<GameObject>();

    // L'enfant 0 sert de modèle pour Instantiate (garde son Inspector tel
    // quel) et n'est jamais lui-même remis dans la pile de disponibles.
    public GameObject template;

    void OnEnable()
    {
        if (template == null)
            template = transform.GetChild(0).gameObject;

        // Récupère les enfants déjà présents dans la scène (à partir de 1,
        // le child 0 restant réservé comme modèle) comme pool de départ.
        for (int i = 1; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;
            if (!child.activeSelf)
            {
                availableObjects.Push(child);
            }
        }
    }

    public GameObject GetPoolObject()
    {
        GameObject objectReturned = availableObjects.Count > 0
            ? availableObjects.Pop()
            : InstantiateObjectPool();

        objectReturned.SetActive(true);
        OnObjectPool?.Invoke();
        return objectReturned;
    }

    public void ReturnPool(GameObject objectPool)
    {
        objectPool.SetActive(false);
        availableObjects.Push(objectPool);
        OnReturnPool?.Invoke();
    }

    private GameObject InstantiateObjectPool()
    {
        GameObject newObject = Instantiate(template, transform);
        return newObject;
    }

    /// <summary>
    /// Pré-instancie des objets et les place directement dans la pile des
    /// disponibles, sans déclencher OnObjectPool/OnReturnPool (ce ne sont
    /// pas de vraies prises/retours côté gameplay, juste un préchauffage).
    /// Idempotent : si la pool a déjà assez d'objets dispo, ne fait rien.
    /// </summary>
    public void WarmUp(int targetAvailableCount)
    {
        if (template == null)
            template = transform.GetChild(0).gameObject;

        int toCreate = targetAvailableCount - availableObjects.Count;
        for (int i = 0; i < toCreate; i++)
        {
            GameObject newObject = InstantiateObjectPool();
            newObject.SetActive(false);
            availableObjects.Push(newObject);
        }
    }
}