using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Caméra libre pour piloter un vaisseau/point de vue de type "Star Destroyer".
/// - ZQSD/WASD : déplacement horizontal relatif à l'orientation de la caméra
/// - E / A : monter / descendre
/// - Clic droit maintenu + déplacement souris : rotation (yaw/pitch)
/// - Molette : avance/recule la caméra (zoom)
/// - Déplacement limité à une zone sphérique autour d'un centre pour ne pas s'éloigner trop.
/// Utilise l'ancien Input Manager (Input.GetAxis / Input.GetKey / Input.GetMouseButton).
/// </summary>
public class FreeCamera : MonoBehaviour
{
    [Header("Zone de jeu")]
    [Tooltip("Centre de la zone autorisée (typiquement le centre de la scène/mission)")]
    public Transform boundsCenter;

    [Tooltip("Rayon maximum autorisé autour du centre")]
    public float boundsRadius = 100f;

    [Header("Déplacement")]
    public float moveSpeed = 20f;
    [Tooltip("Multiplicateur de vitesse quand Shift est maintenu")]
    public float fastMoveMultiplier = 3f;
    public float verticalSpeed = 15f;

    [Header("Zoom (avance/recul)")]
    public float zoomSpeed = 20f;

    [Header("Rotation")]
    [Tooltip("Sensibilité de la rotation au déplacement souris")]
    public float rotationSpeed = 150f;

    [Tooltip("Limite l'angle vertical pour éviter que la caméra se retourne")]
    public float minVerticalAngle = -80f;
    public float maxVerticalAngle = 80f;

    [Header("Lissage")]
    [Tooltip("Mets à 0 pour une caméra instantanée, sans lissage")]
    public float smoothTime = 0.1f;

    private float yaw;
    private float pitch;
    private Vector3 currentAngles;
    private Vector3 rotationVelocity;
    private Vector3 moveVelocity;
    private Vector3 targetPosition;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;
        currentAngles = new Vector3(pitch, yaw, 0f);
        targetPosition = transform.position;
    }

    void LateUpdate()
    {
        HandleRotation();
        HandleMovement();
        HandleZoom();
        ApplyBounds();
        ApplyTransform();
    }

    void HandleRotation()
    {
        // Bouton 1 = clic droit
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            yaw += mouseX * rotationSpeed * Time.deltaTime;
            pitch -= mouseY * rotationSpeed * Time.deltaTime;
            pitch = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);
        }
    }

    void HandleMovement()
    {
        // Ignore les déplacements clavier si un champ de texte UI est en cours d'édition, par ex.
        float horizontal = Input.GetAxis("Horizontal"); // A/D ou Q/D selon layout clavier
        float vertical = Input.GetAxis("Vertical");     // W/S ou Z/S selon layout clavier

        float speed = moveSpeed;
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            speed *= fastMoveMultiplier;
        }

        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        // Déplacement horizontal (on ignore la composante Y pour rester "à niveau" par défaut)
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = (forward * vertical + right * horizontal);
        targetPosition += moveDirection * speed * Time.deltaTime;

        // Montée / descente (E monte, A descend)
        if (Input.GetKey(KeyCode.E))
        {
            targetPosition += Vector3.up * verticalSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.A))
        {
            targetPosition -= Vector3.up * verticalSpeed * Time.deltaTime;
        }
    }

    void HandleZoom()
    {
        // Ignore le zoom si la souris est au-dessus d'un élément d'UI.
        if (IsPointerOverUI())
        {
            return;
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.0001f)
        {
            targetPosition += transform.forward * scroll * zoomSpeed;
        }
    }

    void ApplyBounds()
    {
        if (boundsCenter == null)
        {
            return;
        }

        Vector3 offset = targetPosition - boundsCenter.position;
        if (offset.magnitude > boundsRadius)
        {
            offset = offset.normalized * boundsRadius;
            targetPosition = boundsCenter.position + offset;
        }
    }

    void ApplyTransform()
    {
        Vector3 targetAngles = new Vector3(pitch, yaw, 0f);
        currentAngles.x = Mathf.SmoothDampAngle(currentAngles.x, targetAngles.x, ref rotationVelocity.x, smoothTime);
        currentAngles.y = Mathf.SmoothDampAngle(currentAngles.y, targetAngles.y, ref rotationVelocity.y, smoothTime);

        Quaternion rotation = Quaternion.Euler(currentAngles.x, currentAngles.y, 0f);
        Vector3 position = Vector3.SmoothDamp(transform.position, targetPosition, ref moveVelocity, smoothTime);

        transform.rotation = rotation;
        transform.position = position;
    }

    /// <summary>
    /// Vérifie si le pointeur de la souris est actuellement au-dessus d'un élément d'UI
    /// (nécessite un EventSystem présent dans la scène).
    /// </summary>
    bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (boundsCenter == null) return;
        Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
        Gizmos.DrawWireSphere(boundsCenter.position, boundsRadius);
    }
#endif
}
