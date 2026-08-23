using UnityEngine;

public class Interectable : MonoBehaviour
{
    public Outline myOutline;

    private Color originalOutlineColor;
    private Coroutine flashRoutine;

    [Header("Flash couleur au clic")]
    public Color flashColor = Color.red;
    public float flashDuration = 0.1f;

    private void Awake()
    {
        if(myOutline == null)
            myOutline = GetComponent<Outline>();

        originalOutlineColor = myOutline.OutlineColor; // adapte le nom si ta propriété diffère
    }
    public virtual void Interact()
    {
        FlashOutline();
    }

    private void FlashOutline()
    {
        if (myOutline == null) return;

        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(FlashOutlineRoutine());
    }

    private System.Collections.IEnumerator FlashOutlineRoutine()
    {

        myOutline.OutlineColor = flashColor;

        yield return new WaitForSeconds(flashDuration);

        myOutline.OutlineColor = originalOutlineColor;
        flashRoutine = null;
    }

    public virtual void OnMouseOn()
    {
        myOutline.enabled = true;
    }

    public virtual void OnMouseOff()
    {
        myOutline.enabled = false;
    }
}
