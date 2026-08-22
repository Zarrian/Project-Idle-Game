using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Positionne une icône pour qu'elle suive toujours le bord actuel d'une
/// Image en mode Filled (comme le curseur d'un Slider). Suppose : Fill
/// Method = Horizontal, Fill Origin = Left (comme dans ton Inspector).
///
/// Mise en place : l'icône doit être un ENFANT du même RectTransform que la
/// barre (ou d'un objet de MÊME largeur), avec son ancre X à gauche
/// (Anchor Min/Max X = 0) et son pivot X à 0.5 (centré sur le point qu'il
/// représente).
/// </summary>
public class FillBarEdgeIcon : MonoBehaviour
{
    public Image filledBar;
    public RectTransform icon;

    void LateUpdate()
    {
        UpdateIconPosition();
    }

    public void UpdateIconPosition()
    {
        if (filledBar == null || icon == null) return;

        float barWidth = filledBar.rectTransform.rect.width;
        float x = filledBar.fillAmount * barWidth;

        // Fill Origin = Left : fillAmount 0 -> bord gauche (x=0),
        // fillAmount 1 -> bord droit (x=barWidth).
        // Si un jour tu passes Fill Origin à Right, inverse avec :
        // x = barWidth - (filledBar.fillAmount * barWidth);

        Vector2 pos = icon.anchoredPosition;
        pos.x = x;
        icon.anchoredPosition = pos;
    }
}
