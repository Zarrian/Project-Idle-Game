using UnityEngine;

public class ManagerPanelUpgrade : MonoBehaviour
{
    public GameObject panelInfoUpgrade;
    public static ManagerPanelUpgrade instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }

    public PanelUpgrade GeneratePanelUpgrade()
    {
        GameObject newPanel = Instantiate(panelInfoUpgrade, transform);
        return newPanel.GetComponent<PanelUpgrade>();
    }
}
