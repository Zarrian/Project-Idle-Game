using UnityEngine;

public class FeedbackShip : MonoBehaviour
{
    public Ship ship;

    public Pool poolExplosionDamage;
    public Pool poolExplosionDeath;

    public float scaleExplosion = 50;

    private void Awake()
    {
        if (ship == null)
            ship = GetComponent<Ship>();

        ship.OnTakeDamage.AddListener(ExplosionDamage);
        ship.OnDeath.AddListener(ExplosionDeath);
    }

    public float offsetDistance = 10;
    void ExplosionDamage(Vector3 pos)
    {
        Vector3 direction = (pos - transform.position).normalized;

        if (ship.GetComponent<EnemyShip>())
        {
            print("explosionBlue");
        }

        GameObject explosion = poolExplosionDamage.GetPoolObject();
        explosion.transform.position = transform.position + direction * offsetDistance;
        explosion.transform.rotation = transform.rotation;

    }

    void ExplosionDeath()
    {
        GameObject explosion = poolExplosionDeath.GetPoolObject();
        explosion.transform.position = transform.position;
        explosion.transform.rotation = transform.rotation;

        //Modifie scale explosion
        explosion.transform.localScale = Vector3.one * scaleExplosion;
        for (int i = 0; i < explosion.transform.childCount; i++)
        {
            explosion.transform.GetChild(i).localScale = Vector3.one * scaleExplosion;
        }
    }

}
