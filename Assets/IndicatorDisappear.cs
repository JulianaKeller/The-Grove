using System.Collections;
using UnityEngine;

public class IndicatorDisappear : MonoBehaviour
{
    [Header("Fade Durations")]
    public float fadeInDuration = 0.5f;
    public float fadeOutDuration = 0.5f;
    public float fadeOutDelay = 1.5f;

    private ParticleSystem[] particleSystems;
    private Color[] originalStartColors;

    private bool isDisappearing = false;
    private bool isAppearing = false;

    void Awake()
    {
        particleSystems = GetComponentsInChildren<ParticleSystem>();

        originalStartColors = new Color[particleSystems.Length];

        // Save original start colors and initialize alpha = 0
        for (int i = 0; i < particleSystems.Length; i++)
        {
            var main = particleSystems[i].main;
            originalStartColors[i] = main.startColor.color;

            Color c = originalStartColors[i];
            c.a = 0f;
            main.startColor = c;
        }

        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / fadeInDuration;
            float alpha = Mathf.SmoothStep(0f, 1f, t);

            for (int i = 0; i < particleSystems.Length; i++)
                SetStartColorAlpha(particleSystems[i], originalStartColors[i], alpha);

            yield return null;
        }
    }

    /// <summary>
    /// Call this from InteractionManager when the player confirms the spawn.
    /// </summary>
    public void Disappear()
    {
        if (isDisappearing)
            return;

        isDisappearing = true;
        StopEmissions();

        // Start delayed destroy
        StartCoroutine(FadeOutAndDestroy());
    }

    void StopEmissions()
    {
        foreach (ParticleSystem ps in particleSystems)
        {
            var emission = ps.emission;
            emission.enabled = false;
        }
    }

    IEnumerator FadeOutAndDestroy()
    {
        yield return new WaitForSeconds(fadeOutDelay);

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / fadeOutDuration;
            float alpha = Mathf.SmoothStep(1f, 0f, t);

            for (int i = 0; i < particleSystems.Length; i++)
                SetStartColorAlpha(particleSystems[i], originalStartColors[i], alpha);

            yield return null;
        }

        Destroy(gameObject);
    }

    void SetStartColorAlpha(ParticleSystem ps, Color baseColor, float alpha)
    {
        var main = ps.main;
        Color c = baseColor;
        c.a = alpha;
        main.startColor = c;
    }
}
