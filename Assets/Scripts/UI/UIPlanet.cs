using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPlanet : MonoBehaviour
{
    [SerializeField] Camera targetCamera;
    [SerializeField] PlanetStats myPlanet;

    [SerializeField] Image barFilled;
    [SerializeField] TextMeshProUGUI textHP;

    private void OnEnable()
    {
        myPlanet.OnTakeDamage += UpdateUI;
        myPlanet.OnRegenPV += UpdateUI;
    }

    private void OnDisable()
    {
        // Sans ce d�sabonnement, chaque r�activation empile un handler de
        // plus sur les Action de PlanetStats et cette instance ne peut plus
        // jamais �tre garbage collect�e (PlanetStats la r�f�rence � vie).
        myPlanet.OnTakeDamage -= UpdateUI;
        myPlanet.OnRegenPV -= UpdateUI;
    }

    private void UpdateUI(Vector3 vector, float arg2)
    {
        UpdateUI();
    }

    void LateUpdate()
    {
        transform.rotation = targetCamera.transform.rotation;

        //A passer en abonnement pour que se soit moins lourd
    }

    public void UpdateUI()
    {
        barFilled.fillAmount = myPlanet.hp / myPlanet.hpMax;
        textHP.text = myPlanet.hp.ToString();
    }
}
