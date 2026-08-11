using UnityEngine;

public class Scrap : MonoBehaviour
{

    public Pool myPool;

    public DeathStar.Ressources myRessource = DeathStar.Ressources.Metal;
    public int value;

    public void Collect()
    {
        DeathStar.instance.AddRessources(myRessource, value);
        myPool.ReturnPool(gameObject);
    }
}
