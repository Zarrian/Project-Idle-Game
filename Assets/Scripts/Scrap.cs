using System.Collections;
using UnityEngine;

public class Scrap : MonoBehaviour
{
    public DeathStar.Ressources myRessource = DeathStar.Ressources.Metal;
    public int value;

    [SerializeField] Pool metal;
    [SerializeField] Pool Electricity;
    [SerializeField] Pool Uranium;

    public Pool myPoolVisuel;
    public Pool myPool;
    public GameObject visuel;

    [Header("Auto-despawn")]
    [SerializeField] private float lifetime = 60f;
    [Range(0f, 1f)]
    [SerializeField] private float partialCollectPourcentage = 0.3f; // % récupéré si non ramassé

    private Coroutine lifetimeCoroutine;
    private bool isReturning;

    private void OnEnable()
    {
        isReturning = false;
        //SetEsthetics();
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
        switch (myRessource)
        {
            case DeathStar.Ressources.Metal:
                myPoolVisuel = metal;
                break;
            case DeathStar.Ressources.Electricity:
                myPoolVisuel = Electricity;
                break;
            case DeathStar.Ressources.Uranium:
                myPoolVisuel = Uranium;
                break;
            default:
                break;
        }

        visuel = myPoolVisuel.GetPoolObject();
        visuel.transform.position = transform.position;
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

        myPoolVisuel.ReturnPool(visuel);
        myPool.ReturnPool(gameObject);
    }
}