using System.Collections;
using UnityEngine;

public class FeedbackCanon : MonoBehaviour
{
    public GameObject VFXFireCanon;
    public ParticleSystem FXLaser;
    public ParticleSystem.EmissionModule emission;

    private void OnEnable()
    {
        emission = FXLaser.emission;
    }
    public void PlayFXFireCanon()
    {
        VFXFireCanon.SetActive(true);
        FXLaser.gameObject.SetActive(true);
        StartCoroutine(StopFXFireCanon(1.5f));
    }

    public IEnumerator StopFXFireCanon(float delay)
    {
        yield return new WaitForSeconds(0.1f);
        emission.enabled = false;

        yield return new WaitForSeconds(delay - 0.1f);
        VFXFireCanon.SetActive(false);
        FXLaser.gameObject.SetActive(false);
        emission.enabled = true;
    }


}
