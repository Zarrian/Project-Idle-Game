using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [SerializeField] int targetFrameRate = 144;
    private void Awake()
    {
        Instance = this;

        Application.targetFrameRate = targetFrameRate;
        QualitySettings.vSyncCount = 0;

        //Afficher les fps avec un text
        // À appeler au démarrage du jeu

        Resources.UnloadUnusedAssets();
        //System.GC.Collect();

        //StartCoroutine(CleanGarbage());

    }

    //Clean toute les minutes la mémoire
    public IEnumerator CleanGarbage()
    {
        yield return new WaitForSeconds(60f);
        System.GC.Collect();

        StartCoroutine(CleanGarbage());
    }
}
