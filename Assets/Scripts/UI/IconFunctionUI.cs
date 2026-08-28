using UnityEngine;

public class IconFunctionUI : MonoBehaviour
{
    public GameObject panel;
    public Transform canvas;

    private void Start()
    {
        panel.SetActive(false);
    }

    public void PanelInteract()
    {
        if (panel.activeSelf == true)
            ClosePanel();
        else
            OpenPanel();
    }

    public void ClosePanel()
    {
        panel.SetActive(false);
    }

    public void OpenPanel()
    {
        panel.SetActive(true);
    }

}
