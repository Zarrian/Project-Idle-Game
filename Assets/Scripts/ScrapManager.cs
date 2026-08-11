using UnityEngine;

public class ScrapManager : MonoBehaviour
{
    public Pool poolScrap;

    public void InstantiateScrap(Transform ship)
    {
        GameObject newScrap = poolScrap.GetPoolObject();
        newScrap.transform.position = ship.transform.position;
        newScrap.transform.rotation = ship.transform.rotation;

        //varié la taille du scrap en fonction de sa value ?
    }
}
