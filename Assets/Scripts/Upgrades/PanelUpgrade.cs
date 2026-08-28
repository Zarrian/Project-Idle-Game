using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PanelUpgrade : MonoBehaviour
{
    public UnitTier unit;

    public Image icone;
    public TextMeshProUGUI nameShip;

    public TextMeshProUGUI texthp;
    public TextMeshProUGUI textdamage;
    public TextMeshProUGUI textCDAttack;
    public TextMeshProUGUI textAttack;

    public TextMeshProUGUI nextValuehp;
    public TextMeshProUGUI nextValuedamage;
    public TextMeshProUGUI nextValueCDAttack;
    public TextMeshProUGUI nextValueAttack;


    public void SetInfo()
    {
        icone.sprite = unit.uiSprite;
        nameShip.text = unit.tierName;

        texthp.text = unit.pv.ToString();
        textdamage.text = unit.damage.ToString();
        textCDAttack.text = unit.cdAttack.ToString();
        textAttack.text = unit.nbAttack.ToString();
    }

    public void SetInfoCost()
    {

    }

    public void OpenPanel()
    {
        gameObject.SetActive(true);
        SetInfo();
        SetInfoCost();
    }

    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }

    public void InteractPanel()
    {
        if (gameObject.activeSelf)
            ClosePanel();
        else
            OpenPanel();
    }

}
