using System.Collections;
using UnityEngine;

public class FunctionUsefullManager : MonoBehaviour
{
    public static FunctionUsefullManager Instance;
    private void Awake()
    {
        Instance = this;

        Application.targetFrameRate = 144;
        QualitySettings.vSyncCount = 0;

        //Afficher les fps avec un text
        // À appeler au démarrage du jeu

        Resources.UnloadUnusedAssets();
        System.GC.Collect();

        StartCoroutine(CleanGarbage());

    }

    //Clean toute les minutes la mémoire
    public IEnumerator CleanGarbage()
    {
        yield return new WaitForSeconds(60f);
        System.GC.Collect();

        StartCoroutine(CleanGarbage());
    }

    public Transform TryFindNearestTarget(Transform originPoint, LayerMask enemyLayer)
    {
        Collider[] candidates = Physics.OverlapSphere(originPoint.position, 10000, enemyLayer);
        if (candidates.Length == 0)
            return null;

        Transform nearest = null;
        float nearestSqrDistance = float.MaxValue;

        foreach (Collider col in candidates)
        {
            float sqrDistance = (col.transform.position - originPoint.position).sqrMagnitude;
            if (sqrDistance < nearestSqrDistance && col.gameObject.activeSelf == true)
            {
                nearestSqrDistance = sqrDistance;
                nearest = col.transform;
            }
        }

        if (nearest == null)
            return null;

        return nearest;
    }
}
