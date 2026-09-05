using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FunctionUseful
{
    public static class FunctionUsefullManager
    {
        // === OPTIMISATION MAJEURE ===
        // FindTarget() est appel�e tr�s souvent (ex: chaque FixedUpdate pour
        // chaque missile sans cible valide, ou � chaque attaque de Hangar).
        // L'ancienne version faisait un Physics.OverlapSphere(rayon 10000)
        // qui ALLOUE un nouveau tableau � chaque appel, puis une
        // List<Transform> �galement allou�e � chaque appel : avec beaucoup
        // de missiles/vaisseaux actifs, �a g�n�re une quantit� de garbage
        // �norme en continu -> cause majeure des freezes de GC. Buffers
        // statiques r�utilis�s = plus aucune allocation par appel.
        private const int MAX_CANDIDATES = 512;
        private const int MAX_ENEMY_ACTIVE = 100;
        private static readonly Collider[] candidateBuffer = new Collider[MAX_CANDIDATES];
        private static readonly List<Transform> enemyActiveBuffer = new List<Transform>(MAX_ENEMY_ACTIVE);

        public static Transform FindTarget(Transform originPoint, LayerMask enemyLayer, float targetPriority)
        {
            return FindTargetInternal(originPoint, enemyLayer, targetPriority);
        }

        public static Transform FindTarget(Transform originPoint, LayerMask enemyLayer)
        {
            float targetPriority = Random.Range(0f, 100f); // Random priority between 0 and 100
            return FindTargetInternal(originPoint, enemyLayer, targetPriority);
        }

        private static Transform FindTargetInternal(Transform originPoint, LayerMask enemyLayer, float targetPriority)
        {
            int candidateCount = Physics.OverlapSphereNonAlloc(originPoint.position, 10000f, candidateBuffer, enemyLayer);
            if (candidateCount == 0)
                return null;

            //Verifie que l'objet a l'interface Idamageable
            enemyActiveBuffer.Clear();
            for (int i = 0; i < candidateCount; i++)
            {
                if (candidateBuffer[i].TryGetComponent(out IDamageable damageable))
                {
                    enemyActiveBuffer.Add(candidateBuffer[i].transform);

                    if (enemyActiveBuffer.Count >= MAX_ENEMY_ACTIVE)
                    {
                        break;
                    }
                }
            }

            if (enemyActiveBuffer.Count == 0)
                return null;

            // Cas extr�mes
            if (targetPriority <= 0)
                return enemyActiveBuffer[enemyActiveBuffer.Count - 1];

            if (targetPriority >= 100)
                return enemyActiveBuffer[0];

            float t = targetPriority / 100f;

            // Plus t est grand, plus on favorise les petits indices.
            float random = Mathf.Pow(Random.value, Mathf.Lerp(3f, 0.35f, t));

            int index = Mathf.RoundToInt(random * (enemyActiveBuffer.Count - 1));

            return enemyActiveBuffer[index];
        }
    }
}
