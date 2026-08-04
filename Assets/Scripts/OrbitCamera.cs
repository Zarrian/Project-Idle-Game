using UnityEngine;

/// <summary>
/// Caméra qui orbite autour d'une cible (typiquement le centre d'une sphère).
/// - Molette : zoom / dézoom
/// - Clic molette maintenu + déplacement souris : rotation autour de la cible
/// Utilise l'ancien Input Manager (Input.GetAxis / Input.GetMouseButton).
/// </summary>
public class OrbitCamera : MonoBehaviour
{
    [Header("Cible")]
    [Tooltip("Point autour duquel la caméra orbite (centre de la sphère)")]
    public Transform target;

    [Header("Distance / Zoom")]
    public float distance = 20f;
    public float minDistance = 5f;
    public float maxDistance = 50f;
    public float zoomSpeed = 10f;

    [Header("Rotation")]
    [Tooltip("Sensibilité de la rotation au déplacement souris")]
    public float rotationSpeed = 150f;

    [Tooltip("Limite l'angle vertical pour éviter que la caméra passe au-dessus/en dessous et se retourne")]
    public float minVerticalAngle = -80f;
    public float maxVerticalAngle = 80f;

    [Header("Lissage")]
    [Tooltip("Mets à 0 pour une caméra instantanée, sans lissage")]
    public float smoothTime = 0.1f;

    private float yaw;
    private float pitch;
    private float currentDistance;
    private float distanceVelocity;
    private Vector3 rotationVelocity;
    private Vector3 currentAngles;

    void Start()
    {
        if (target == null)
        {
            Debug.LogWarning("OrbitCamera: aucune target assignée.");
        }

        // Initialise yaw/pitch à partir de la position actuelle de la caméra
        // pour éviter un saut au premier frame.
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;
        currentAngles = new Vector3(pitch, yaw, 0f);
        currentDistance = distance;
    }

    void LateUpdate()
    {
        if (target == null) return;

        HandleZoom();
        HandleRotation();
        ApplyTransform();
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.0001f)
        {
            distance -= scroll * zoomSpeed;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }
    }

    void HandleRotation()
    {
        // Bouton 2 = clic molette
        if (Input.GetMouseButton(2))
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            yaw += mouseX * rotationSpeed * Time.deltaTime;
            pitch -= mouseY * rotationSpeed * Time.deltaTime;
            pitch = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);
        }
    }

    void ApplyTransform()
    {
        // Lissage de la distance (zoom) et de la rotation.
        currentDistance = Mathf.SmoothDamp(currentDistance, distance, ref distanceVelocity, smoothTime);

        Vector3 targetAngles = new Vector3(pitch, yaw, 0f);
        currentAngles.x = Mathf.SmoothDampAngle(currentAngles.x, targetAngles.x, ref rotationVelocity.x, smoothTime);
        currentAngles.y = Mathf.SmoothDampAngle(currentAngles.y, targetAngles.y, ref rotationVelocity.y, smoothTime);

        Quaternion rotation = Quaternion.Euler(currentAngles.x, currentAngles.y, 0f);
        Vector3 position = target.position - (rotation * Vector3.forward * currentDistance);

        transform.rotation = rotation;
        transform.position = position;
    }
}
