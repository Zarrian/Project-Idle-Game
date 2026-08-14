using System.Collections;
using UnityEngine;

public class FeedbackCanon : MonoBehaviour
{
    public GameObject VFXFireCanon;


    public void PlayFXFireCanon()
    {
        VFXFireCanon.SetActive(true);
        StartCoroutine(StopFXFireCanon(1.5f));
    }

    public IEnumerator StopFXFireCanon(float delay)
    {
        yield return new WaitForSeconds(delay);
        VFXFireCanon.SetActive(false);
    }
}
