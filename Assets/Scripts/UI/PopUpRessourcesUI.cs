using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PopUpRessourcesUI : MonoBehaviour
{

    public List<DisplayRessourceUI> displayRessources;

    public Pool myPool;

    void Start()
    {
        DeathStar.instance.onResourceChanged.AddListener(PopUpRessources);
    }



    public void PopUpRessources(DeathStar.Ressources ressource, float valueAdd)
    {
        foreach (DisplayRessourceUI displayRessource in displayRessources)
        {
            if (displayRessource.myRessource == ressource)
            {
                GameObject popUp = myPool.GetPoolObject();
                popUp.transform.position = displayRessource.transform.position;

                //Si valeur positif
                if (valueAdd > 0)
                {
                    popUp.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.green;
                    popUp.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "+" + valueAdd.ToString();
                }
                //Si valeur négative
                else
                {
                    popUp.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.red;
                    popUp.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "-" + valueAdd.ToString();
                }
            }
        }
    }
}
