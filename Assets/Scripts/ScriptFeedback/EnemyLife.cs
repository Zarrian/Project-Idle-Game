using UnityEngine;

public class EnemyLife : MonoBehaviour
{
    public int pv = 5;

    public GameObject feedbackDeath;
    private float nbFeedback = 50;

    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    //Fonction Take Damage
    public void TakeDamage(int damage)
    {
        pv -= damage;

        //Lance une anim
        animator.SetTrigger("Hit");

        if (pv <= 0)
        {
            Death();
        }
    }

    //A la mort de l'object
    public void Death()
    {
        //Instantie 50 object de feedbackDeath
        for (int i = 0; i < nbFeedback; i++)
        {
            Instantiate(feedbackDeath, transform.position, Quaternion.identity);
        }

        SlowMotion.instance.StartSlowMotion(0.1f, 0.01f);
        Destroy(gameObject);
    }
}
