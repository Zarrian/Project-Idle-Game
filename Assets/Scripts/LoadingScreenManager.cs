using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gère l'écran de chargement DANS la scène de jeu elle-même (pas de
/// changement de scène). Préchauffe les Pool listées, puis lance une
/// transition : fondu de sortie de l'écran de chargement (Animator) +
/// fondu d'entrée des CanvasGroup de gameplay, en simultané.
/// </summary>
public class LoadingScreenManager : MonoBehaviour
{
    [System.Serializable]
    public class PoolWarmupEntry
    {
        public Pool pool;
        [Tooltip("Nombre d'objets disponibles souhaité pour cette pool (dimensionne sur le tier max).")]
        public int targetAvailableCount = 20;
    }

    [Header("Pools à préchauffer")]
    [SerializeField] private List<PoolWarmupEntry> poolsToWarmup = new List<PoolWarmupEntry>();

    [Header("Barre de progression")]
    [SerializeField] private Slider progressBar;
    [SerializeField] private Text progressLabel;

    [Header("Écran de chargement")]
    [SerializeField] private Animator loadingScreenGroup;
    [Tooltip("Durée de l'animation \"FadeOut\" jouée par l'Animator ci-dessus.")]
    [SerializeField] private float loadingFadeOutDuration = 0.5f;

    [Header("UI de jeu à révéler")]
    [Tooltip("CanvasGroup désactivés/alpha 0 au départ, qui apparaissent une fois le jeu prêt.")]
    [SerializeField] private List<CanvasGroup> gameplayUIGroups = new List<CanvasGroup>();
    [SerializeField] private float gameplayFadeInDuration = 0.6f;

    [Header("Petit confort visuel")]
    [SerializeField] private float holdAtFullBarDuration = 0.15f;

    private void Start()
    {
        loadingScreenGroup.gameObject.SetActive(true);
        // S'assure que l'UI de jeu est bien invisible tant que le
        // chargement n'est pas terminé.
        foreach (CanvasGroup group in gameplayUIGroups)
        {
            group.alpha = 0f;
        }

        StartCoroutine(LoadRoutine());
    }

    private IEnumerator LoadRoutine()
    {
        yield return StartCoroutine(WarmupPoolsRoutine());

        UpdateUI(1f);
        yield return new WaitForSeconds(holdAtFullBarDuration);

        yield return StartCoroutine(TransitionToGameplay());
    }

    private IEnumerator WarmupPoolsRoutine()
    {
        int total = poolsToWarmup.Count;
        for (int i = 0; i < total; i++)
        {
            PoolWarmupEntry entry = poolsToWarmup[i];
            if (entry.pool != null)
            {
                entry.pool.WarmUp(entry.targetAvailableCount);
            }

            UpdateUI((i + 1) / (float)Mathf.Max(total, 1));
            yield return null; // une frame entre chaque pool pour lisser le pic de charge
        }
    }

    /// <summary>
    /// Le moment clé de la transition : on déclenche la sortie du loading
    /// screen et l'apparition du HUD en même temps, puis on attend que
    /// la plus longue des deux transitions soit terminée.
    /// </summary>
    private IEnumerator TransitionToGameplay()
    {
        // 1) Sortie du loading screen : gérée par l'Animator, on ne fait
        //    que déclencher le trigger, l'anim tourne en parallèle.
        loadingScreenGroup.SetTrigger("FadeOut");

        yield return new WaitForSeconds(loadingFadeOutDuration);

        // 2) Apparition du HUD de jeu : gérée ici en alpha, pendant que
        //    l'anim ci-dessus se joue de son côté.
        yield return StartCoroutine(FadeInGameplayUI());

        // 3) Filet de sécurité : si l'anim du loading screen est plus
        //    longue que le fade-in du HUD, on lui laisse le temps de finir
        //    avant de considérer la transition terminée.
        float remainingLoadingScreenTime = loadingFadeOutDuration - gameplayFadeInDuration;
        if (remainingLoadingScreenTime > 0f)
        {
            yield return new WaitForSeconds(remainingLoadingScreenTime);
        }
    }

    private IEnumerator FadeInGameplayUI()
    {
        float elapsed = 0f;
        while (elapsed < gameplayFadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / gameplayFadeInDuration);

            foreach (CanvasGroup group in gameplayUIGroups)
            {
                group.alpha = t;
            }

            yield return null;
        }

        foreach (CanvasGroup group in gameplayUIGroups)
        {
            group.alpha = 1f;
        }
    }

    private void UpdateUI(float progress01)
    {
        if (progressBar != null)
            progressBar.value = progress01;

        if (progressLabel != null)
            progressLabel.text = $"Chargement... {Mathf.RoundToInt(progress01 * 100f)}%";
    }
}