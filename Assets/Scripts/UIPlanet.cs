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
