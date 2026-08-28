using Project.UI;
using UnityEngine;

public class TacticalPanelUI : MonoBehaviour
{
    public static TacticalPanelUI instance;
    [SerializeField] GameObject panelUIShip;
    [SerializeField] Transform content;


    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }

    public ProfileUIShip InstantiateUIPanel(HangarShip ship)
    {
        GameObject uiPanel = Instantiate(panelUIShip, content);
        ProfileUIShip profilUIShip = uiPanel.GetComponent<ProfileUIShip>();
        profilUIShip.ship = ship;
        return profilUIShip;
    }
}
