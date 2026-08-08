using UnityEngine;

public class ExplosionForce : MonoBehaviour
{
    public float explosionForce = 50f;

    public float desagregationSpeed;

    //Créer un effet d'explosion pour que les balles partent a des endroits différents
    private void Start()
    {
        GetComponent<Rigidbody>().AddForce(new Vector3(Random.Range(-1f, 1f), Random.Range(2f, 3f), Random.Range(-1f, 1f)) * explosionForce, ForceMode.Impulse);
    }

    //Réduit progressivement leur taille, quand atteinds proche de 0, détruit les objets
    private void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, Time.deltaTime * desagregationSpeed);

        if(transform.localScale.magnitude < 0.1f)
        {
            Destroy(gameObject);
        }
    }
}
