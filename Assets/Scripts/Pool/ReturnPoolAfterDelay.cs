using System.Collections;
using UnityEngine;

public class ReturnPoolAfterDelay : MonoBehaviour
{
    public Pool myPool;

    [SerializeField] float delay = 3;
    private Coroutine returnPoolCoroutine;

    void OnEnable()
    {
        if (returnPoolCoroutine != null)
        {
            StopCoroutine(returnPoolCoroutine);
        }
        returnPoolCoroutine = StartCoroutine(ReturnPool());
    }

    void OnDisable()
    {
        if (returnPoolCoroutine != null)
        {
            StopCoroutine(returnPoolCoroutine);
            returnPoolCoroutine = null;
        }
    }

    IEnumerator ReturnPool()
    {
        yield return new WaitForSeconds(delay);

        myPool.ReturnPool(gameObject);
    }
}
