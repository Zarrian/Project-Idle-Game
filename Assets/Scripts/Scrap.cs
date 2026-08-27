using UnityEngine;

public class Scrap : MonoBehaviour
{

    public Pool myPool;

    public DeathStar.Ressources myRessource = DeathStar.Ressources.Metal;
    public int value;
    [SerializeField] GameObject metal;
    [SerializeField] GameObject Electricity;
    [SerializeField] GameObject Uranium;

    private void OnEnable()
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
        myPool.ReturnPool(gameObject);
    }
}
