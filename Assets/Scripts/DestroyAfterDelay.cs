using UnityEngine;

public class DestroyAfterDelay : MonoBehaviour
{
    [SerializeField] float delay = 3;
    void Start()
    {
        Destroy(gameObject, delay);
    }

}
