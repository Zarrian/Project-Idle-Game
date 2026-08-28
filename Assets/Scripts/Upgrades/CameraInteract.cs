using UnityEngine;
using UnityEngine.EventSystems;

public class CameraInteract : MonoBehaviour
{
    [Header("Raycast")]
    public Camera mainCamera;
    public float interactDistance = 10f;
    public LayerMask interactableLayer = ~0;
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
        // Si la souris est au-dessus d'un élément UI, on ignore complètement le 3D
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            if (currentTarget != null)
            {
                currentTarget.OnMouseOff();
                currentTarget = null;
            }
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactableLayer))
        {
            Interectable hitInteractable = hit.collider.GetComponent<Interectable>();
            if (hitInteractable != null)
            {
                if (hitInteractable != currentTarget)
                {
                    if (currentTarget != null)
                        currentTarget.OnMouseOff();
                    currentTarget = hitInteractable;
                    currentTarget.OnMouseOn();
                }
                return;
            }
        }

        if (currentTarget != null)
        {
            currentTarget.OnMouseOff();
            currentTarget = null;
        }
    }
}