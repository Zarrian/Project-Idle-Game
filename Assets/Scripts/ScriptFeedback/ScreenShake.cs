using Unity.VisualScripting;
using UnityEngine;

public class ScreenShake : MonoBehaviour
{
    public float shakeAmount = 0.05f;
    public float shakeDuration = 0.5f;

    public Transform myCamera;
    private Vector3 initialPosition;
    private float shakeTimer;

    void Start()
    {
        initialPosition = myCamera.position;
    }

    void Update()
    {
        //Quand appuie sur espace, apelle la fonction Shake
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Shake();
        }

        //Si il reste du temps dans "ShakerTimer"
        //Déplace la caméra aléatoirement chaque frame
        if (shakeTimer > 0)
        {
            myCamera.position = initialPosition + (Vector3)Random.insideUnitSphere * shakeAmount;
            shakeTimer -= Time.deltaTime;
        }

        //Quand finis, retourne a sa position initial
        else
        {
            myCamera.position = initialPosition;
        }
    }

    public void Shake()
    {
        shakeTimer = shakeDuration;
    }

}