using System.Collections;
using UnityEngine;

public class Scrap : MonoBehaviour
{
    public Pool myPool;
    public DeathStar.Ressources myRessource = DeathStar.Ressources.Metal;
    public int value;

    [SerializeField] GameObject metal;
    [SerializeField] GameObject Electricity;
    [SerializeField] GameObject Uranium;

    [Header("Auto-despawn")]
    [SerializeField] private float lifetime = 60f;
    [Range(0f, 1f)]
    [SerializeField] private float partialCollectPourcentage = 0.3f; // % récupéré si non ramassé

    private Coroutine lifetimeCoroutine;
    private bool isReturning;

    private void OnEnable()
    {
        isReturning = false;
        SetEsthetics();
        lifetimeCoroutine = StartCoroutine(CollectJustSome(partialCollectPourcentage));
    }

    private void OnDisable()
    {
        // Sécurité : si l'objet est désactivé autrement (ex: pool clear), on stoppe la coroutine
        if (lifetimeCoroutine != null)
        {
            StopCoroutine(lifetimeCoroutine);
            lifetimeCoroutine = null;
        }
    }

    public void SetEsthetics()
    {
        metal.SetActive(false);
        Electricity.SetActive(false);
        Uranium.SetActive(false);
        switch (myRessource)
        {
            case DeathStar.Ressources.Metal:
                metal.SetActive(true);
                break;
            case DeathStar.Ressources.Electricity:
                Electricity.SetActive(true);
                break;
            case DeathStar.Ressources.Uranium:
                Uranium.SetActive(true);
                break;
            default:
                break;
        }
    }

    public void Collect()
    {
        DeathStar.instance.AddRessources(myRessource, value);
        ReturnToPool();
    }

    public IEnumerator CollectJustSome(float pourcentage)
    {
        yield return new WaitForSeconds(lifetime);

        int partialValue = Mathf.RoundToInt(value * pourcentage);
        DeathStar.instance.AddRessources(myRessource, partialValue);
        ReturnToPool();
    }

    private void ReturnToPool()
    {
        if (isReturning) return;
        isReturning = true;

        if (lifetimeCoroutine != null)
        {
            StopCoroutine(lifetimeCoroutine);
            lifetimeCoroutine = null;
        }

        myPool.ReturnPool(gameObject);
    }
}