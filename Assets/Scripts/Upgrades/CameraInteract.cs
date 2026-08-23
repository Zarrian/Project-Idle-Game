using UnityEngine;

public class CameraInteract : MonoBehaviour
{
    [Header("Raycast")]
    public Camera mainCamera;
    public float interactDistance = 10f;
    public LayerMask interactableLayer = ~0; // Tous les layers par défaut

    private Interectable currentTarget;


    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    private void Update()
    {
        HandleHover();

        if (Input.GetMouseButtonDown(0) && currentTarget != null)
        {
            currentTarget.Interact();
        }
    }

    private void HandleHover()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactableLayer))
        {
            Interectable hitInteractable = hit.collider.GetComponent<Interectable>();

            if (hitInteractable != null)
            {
                // Nouvel objet survolé
                if (hitInteractable != currentTarget)
                {
                    // On désactive l'ancien
                    if (currentTarget != null)
                        currentTarget.OnMouseOff();

                    currentTarget = hitInteractable;
                    currentTarget.OnMouseOn();
                }
                return;
            }
        }

        // Rien sous la souris (ou objet non interactable) -> on désactive si besoin
        if (currentTarget != null)
        {
            currentTarget.OnMouseOff();
            currentTarget = null;
        }
    }
}