using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class DisplayRessourceUI : MonoBehaviour
{

    public DeathStar.Ressources myRessource;
    public TextMeshProUGUI textRessource;

    void Start()
    {
        DeathStar.instance.onResourceChanged.AddListener(DisplaytextRessource);
    }

    public void DisplaytextRessource(DeathStar.Ressources ressource, float valueAdd)
    {
        if (myRessource != ressource)
            return;

        //Lance une animation ou je sais pas quoi
        textRessource.text = DeathStar.instance.GetAmount(myRessource).ToString();
    }

}
