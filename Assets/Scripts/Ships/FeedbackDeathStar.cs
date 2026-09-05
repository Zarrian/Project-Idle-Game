using UnityEngine;

public class FeedbackDeathStar : MonoBehaviour
{
    [SerializeField] PlanetStats star;
    public Pool poolExplosionDamage;
    public float ratioExplosion = 1f;
    private void Start()
    {
        offsetDistance = GetComponent<SphereCollider>().radius + 10;

        star.OnTakeDamage += ExplosionDamage;
    }
    float offsetDistance = 10;
    void ExplosionDamage(Vector3 pos, float damage)
    {
        Vector3 direction = (pos - transform.position).normalized;

        GameObject explosion = poolExplosionDamage.GetPoolObject();

        // Racine carrée au lieu d'une simple multiplication : la taille
        // augmente vite au début (1 -> 10 se voit clairement) puis de moins
        // en moins vite ensuite (10 -> 100 ne fait pas 10x plus gros).
        float scaleValue = Mathf.Sqrt(damage) * ratioExplosion;
        Vector3 explosionScale = Vector3.one * scaleValue;

        explosion.transform.localScale = explosionScale;
        for (int i = 0; i < explosion.transform.childCount; i++)
        {
            explosion.transform.GetChild(i).localScale = explosionScale;
        }

        explosion.transform.position = transform.position + direction * offsetDistance;
        explosion.transform.rotation = transform.rotation;

    }

    void ExplosionDeath()
    {
        //GameObject explosion = poolExplosionDeath.GetPoolObject();
        //explosion.transform.position = transform.position;
        //explosion.transform.rotation = transform.rotation;
        //
        ////Modifie scale explosion
        //explosion.transform.localScale = Vector3.one * scaleExplosionDeath;
        //for (int i = 0; i < explosion.transform.childCount; i++)
        //{
        //    explosion.transform.GetChild(i).localScale = Vector3.one * scaleExplosionDeath;
        //}
    }
}
