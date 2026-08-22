using System.Collections;
using UnityEngine;

public class ReturnPoolAfterDelay : MonoBehaviour
{
    public Pool myPool;

    [SerializeField] float delay = 3;
    void OnEnable()
    {
        StartCoroutine(ReturnPool());
    }

    IEnumerator ReturnPool()
    {
        yield return new WaitForSeconds(delay);

        myPool.ReturnPool(gameObject);
    }

}
