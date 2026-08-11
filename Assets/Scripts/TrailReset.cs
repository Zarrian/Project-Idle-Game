using System.Collections;
using UnityEngine;

public class PooledTrail : MonoBehaviour
{
    public TrailRenderer trail;

    private void Start()
    {
        if (trail == null)
            trail = GetComponent<TrailRenderer>();
    }

    private void OnEnable()
    {
        trail.Clear();
        StartCoroutine(ClearNextFrame());
    }

    private void OnDisable()
    {
        trail.Clear();
        // Si la coroutine ci-dessous était en cours, Unity l'arrête toute
        // seule ici — sans conséquence puisqu'elle ne fait QUE rappeler
        // Clear(), rien qui puisse rester dans un état à moitié fait.
    }

    /// <summary>
    /// Un Clear() seul ne suffit pas toujours de façon fiable sur un
    /// TrailRenderer réactivé depuis un pool (bug connu de Unity). Le
    /// correctif classique : un second Clear() une frame plus tard, une
    /// fois que le buffer interne a eu le temps de vraiment se vider.
    /// Contrairement à la version précédente, on ne touche à AUCUNE
    /// propriété persistante (emitting, time) — donc rien ne peut rester
    /// bloqué dans un état cassé si l'objet est redésactivé entre-temps.
    /// </summary>
    private IEnumerator ClearNextFrame()
    {
        yield return null;
        trail.Clear();
    }
}