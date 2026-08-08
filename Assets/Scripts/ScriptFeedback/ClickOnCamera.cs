using UnityEngine;

public class ClickOnCamera : MonoBehaviour
{

    void Update()
    {
        //Quand je click gauche avec ma souris
        if (Input.GetMouseButtonDown(0))
        {
            //Crée un raycast
            //Si jamais vous voulez en apprendre plus : https://www.youtube.com/watch?v=cUf7FnNqv7U
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 100.0f))
            {
                //Si l'object touché a le script, appelle la fonction TakeDamage
                if(hit.collider.gameObject.GetComponent<EnemyLife>())
                {
                    hit.collider.gameObject.GetComponent<EnemyLife>().TakeDamage(1);
                }
            }
        }
    }
}
