using UnityEngine;

public class ImmerseCamera : MonoBehaviour
{
    public DeathStar manager;
    public Transform shipFollow;
    public Camera immersiveCamera;
    public Camera mainCamera;

    private Vector3 offset = new Vector3(0, 5, -500);
    private float followSmoothness = 5f;
    private float rotationSensitivity = 5f;
    private float zoomSensitivity = 200f;
    private float minZoom = -3000f;
    private float maxZoom = -200f;

    private float cameraRotationX = 0f;
    private float cameraRotationY = 0f;
    [SerializeField] bool isTransitioning = false;

    [Header("Transition Settings")]
    [SerializeField] private float transitionDuration = 1.5f; // durée fixe en secondes

    private float transitionTimer = 0f;
    private Vector3 transitionStartPos;
    private Quaternion transitionStartRot;

    private Transform transitionGoal; // repositionné chaque frame pendant la transition

    private Vector3 mainCameraHomePosition;
    private Quaternion mainCameraHomeRotation;

    public bool immersiveViewActive = false;

    private void Awake()
    {
        // Capture la position de repos de la caméra principale telle qu'elle est placée dans la scène
        mainCameraHomePosition = mainCamera.transform.position;
        mainCameraHomeRotation = mainCamera.transform.rotation;
    }

    private void OnDestroy()
    {
        if (transitionGoal != null)
        {
            Destroy(transitionGoal.gameObject);
        }
    }

    private void FixedUpdate()
    {
        // Si en mode immersive et le vaisseau est mort
        if (immersiveViewActive && (shipFollow == null || !shipFollow.gameObject.activeSelf))
        {
            // Prendre un nouveau vaisseau aléatoire avec transition
            SetShipFollow();
            if (shipFollow != null && shipFollow.gameObject.activeSelf)
            {
                StartCameraTransition();
            }
        }

        if (isTransitioning)
        {
            UpdateTransition();
        }
        else if (immersiveViewActive)
        {
            FollowShip();
            HandleCameraInput();
        }
    }

    public void FollowShip()
    {
        if (shipFollow != null && shipFollow.gameObject.activeSelf)
        {
            // Position cible derrière le vaisseau avec rotation
            Vector3 rotatedOffset = GetRotatedOffset();
            Vector3 targetPosition = shipFollow.position + shipFollow.TransformDirection(rotatedOffset);

            // Suivi fluide de la caméra
            immersiveCamera.transform.position = Vector3.Lerp(
                immersiveCamera.transform.position,
                targetPosition,
                Time.fixedDeltaTime * followSmoothness
            );

            // La caméra regarde le vaisseau
            immersiveCamera.transform.LookAt(shipFollow.position + Vector3.up * 2);
        }
        else if (shipFollow != null && !shipFollow.gameObject.activeSelf)
        {
            // Le vaisseau est mort, revenir à la caméra principale
            StartCameraTransition();
        }
    }

    private Vector3 GetRotatedOffset()
    {
        // Appliquer les rotations à l'offset
        Quaternion rotation = Quaternion.Euler(cameraRotationX, cameraRotationY, 0f);
        return rotation * offset;
    }

    private void HandleCameraInput()
    {
        // Clic droit maintenu pour tourner
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            cameraRotationY += mouseX * rotationSensitivity;
            cameraRotationX -= mouseY * rotationSensitivity;
            cameraRotationX = Mathf.Clamp(cameraRotationX, -85f, 85f);
        }

        // Molette pour zoomer/dézoomer
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            offset.z += scroll * zoomSensitivity;
            offset.z = Mathf.Clamp(offset.z, minZoom, maxZoom);
        }
    }

    public void SetShipFollow()
    {
        // Sinon, prendre un vaisseau aléatoire de la liste allShipPlayer
        if (manager != null && manager.allShipPlayer != null && manager.allShipPlayer.Count > 0)
        {
            Transform randomShip = manager.allShipPlayer[Random.Range(0, manager.allShipPlayer.Count)].transform;

            if (shipFollow != randomShip)
            {
                shipFollow = randomShip;
                StartCameraTransition();
            }
        }
    }

    public void SetCamera()
    {
        if (shipFollow != null && shipFollow.gameObject.activeSelf)
        {
            if (!immersiveCamera.enabled)
            {
                StartCameraTransition();
            }
        }
        else if (shipFollow == null || !shipFollow.gameObject.activeSelf)
        {
            if (immersiveCamera.enabled)
            {
                StartCameraTransition();
            }
        }
    }

    private void EnsureGoal()
    {
        if (transitionGoal == null)
        {
            GameObject goalObj = new GameObject("CameraTransitionGoal");
            goalObj.hideFlags = HideFlags.HideInHierarchy;
            transitionGoal = goalObj.transform;
        }
    }

    private void StartCameraTransition()
    {
        isTransitioning = true;
        EnsureGoal();
        transitionTimer = 0f;

        Camera activeCam = immersiveViewActive ? immersiveCamera : mainCamera;
        transitionStartPos = activeCam.transform.position;
        transitionStartRot = activeCam.transform.rotation;
    }

    private void UpdateTransition()
    {
        Camera activeCam = immersiveViewActive ? immersiveCamera : mainCamera;
        Camera inactiveCam = immersiveViewActive ? mainCamera : immersiveCamera;

        // 1. Repositionne le goal CHAQUE FRAME sur la cible actuelle
        if (immersiveViewActive && shipFollow != null && shipFollow.gameObject.activeSelf)
        {
            Vector3 rotatedOffset = GetRotatedOffset();
            transitionGoal.position = shipFollow.position + shipFollow.TransformDirection(rotatedOffset) + offset;

            Vector3 lookTarget = shipFollow.position + Vector3.up * 2f;
            transitionGoal.rotation = Quaternion.LookRotation((lookTarget - transitionGoal.position).normalized);
        }
        else
        {
            // Cible fixe de repos pour la caméra principale
            transitionGoal.position = mainCameraHomePosition;
            transitionGoal.rotation = mainCameraHomeRotation;
        }

        // 2. Avance le timer et interpole entre le départ figé et le goal (mouvant)
        transitionTimer += Time.fixedDeltaTime;
        float t = Mathf.Clamp01(transitionTimer / transitionDuration);

        activeCam.transform.position = Vector3.Lerp(transitionStartPos, transitionGoal.position, t);
        activeCam.transform.rotation = Quaternion.Slerp(transitionStartRot, transitionGoal.rotation, t);

        activeCam.enabled = true;
        inactiveCam.enabled = false;

        // 3. Fin garantie après transitionDuration secondes
        if (t >= 1f)
        {
            activeCam.transform.position = transitionGoal.position;
            activeCam.transform.rotation = transitionGoal.rotation;
            isTransitioning = false;
        }
    }

    public void StartandStopImmersiveView()
    {
        if (immersiveViewActive)
        {
            // Save ancienne position caméra
            mainCameraHomePosition = mainCamera.transform.position;
            mainCameraHomeRotation = mainCamera.transform.rotation;

            // Désactiver la vue immersive
            immersiveViewActive = false;
            StartCameraTransition();
        }
        else
        {
            // Activer la vue immersive
            immersiveViewActive = true;
            SetShipFollow(); // Prendre un vaisseau aléatoire
            StartCameraTransition();
        }
    }
}