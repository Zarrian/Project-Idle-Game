using UnityEngine;
using TMPro;

public class FPSCount : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI fpsText;
    //[SerializeField] private TextMeshProUGUI averageFpsText;

    private float currentFPS;
    private float totalFrames = 0f;
    //private float totalTime = 0f;
    private float updateInterval = 1f; // Mise à jour tous les 0.5 secondes
    private float timeSinceLastUpdate = 0f;

    private void Update()
    {
        totalFrames++;
        //totalTime += Time.deltaTime;
        timeSinceLastUpdate += Time.deltaTime;

        // Mettre à jour les FPS actuels
        if (timeSinceLastUpdate >= updateInterval)
        {
            currentFPS = totalFrames / timeSinceLastUpdate;

            if (fpsText != null)
                fpsText.text = $"FPS: {currentFPS:F0}";

            timeSinceLastUpdate = 0f;
            totalFrames = 0f;
        }

        //// Mettre à jour les FPS moyens
        //if (averageFpsText != null && totalTime > 0f)
        //{
        //    float averageFPS = totalFrames / totalTime;
        //    averageFpsText.text = $"Average FPS: {averageFPS:F1}";
        //}
    }
}
