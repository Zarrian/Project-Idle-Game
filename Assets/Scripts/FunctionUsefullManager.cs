using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FunctionUseful
{
    public static class FunctionUsefullManager
    {

        public static Transform FindTarget(Transform originPoint, LayerMask enemyLayer, float targetPriority)
        {
            Collider[] candidates = Physics.OverlapSphere(originPoint.position, 10000, enemyLayer);
            if (candidates.Length == 0)
                return null;

            //Verifie que l'objet a l'interface Idamageable
            List<Transform> EnemyActif = new List<Transform>();
            foreach (Collider unit in candidates)
            {
                IDamageable damageable = unit.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    EnemyActif.Add(unit.transform);
                }

                if (EnemyActif.Count > 100)
                {
                    break;
                }
            }

            if (EnemyActif == null || EnemyActif.Count == 0)
                return null;

            // Cas extrêmes
            if (targetPriority <= 0)
                return EnemyActif[EnemyActif.Count - 1];

            if (targetPriority >= 100)
                return EnemyActif[0];

            float t = targetPriority / 100f;

            // Plus t est grand, plus on favorise les petits indices.
            float random = Mathf.Pow(Random.value, Mathf.Lerp(3f, 0.35f, t));

            int index = Mathf.RoundToInt(random * (EnemyActif.Count - 1));

            return EnemyActif[index];
        }

        public static Transform FindTarget(Transform originPoint, LayerMask enemyLayer)
        {
            float targetPriority = Random.Range(0f, 100f); // Random priority between 0 and 100

            Collider[] candidates = Physics.OverlapSphere(originPoint.position, 10000, enemyLayer);
            if (candidates.Length == 0)
                return null;

            //Verifie que l'objet a l'interface Idamageable
            List<Transform> EnemyActif = new List<Transform>();
            foreach (Collider unit in candidates)
            {
                IDamageable damageable = unit.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    EnemyActif.Add(unit.transform);
                }

                if (EnemyActif.Count > 100)
                {
                    break;
                }
            }

            if (EnemyActif == null || EnemyActif.Count == 0)
                return null;

            // Cas extrêmes
            if (targetPriority <= 0)
                return EnemyActif[EnemyActif.Count - 1];

            if (targetPriority >= 100)
                return EnemyActif[0];

            float t = targetPriority / 100f;

            // Plus t est grand, plus on favorise les petits indices.
            float random = Mathf.Pow(Random.value, Mathf.Lerp(3f, 0.35f, t));

            int index = Mathf.RoundToInt(random * (EnemyActif.Count - 1));

            return EnemyActif[index];
        }
    }
}
