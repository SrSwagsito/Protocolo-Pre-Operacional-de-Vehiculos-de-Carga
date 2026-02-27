using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance;

    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Iniciar con pantalla negra y aclarar
        InstantDark();
        StartCoroutine(FadeIn(fadeDuration));
    }

    // Oscurecer pantalla (alpha 0 → 1)
    public IEnumerator FadeOut(float duration)
    {
        float time = 0;
        Color color = fadeImage.color;
        float startAlpha = color.a;

        while (time < duration)
        {
            time += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, 1f, time / duration);
            fadeImage.color = color;
            yield return null;
        }

        // Asegurar que termine completamente negro
        color.a = 1f;
        fadeImage.color = color;
    }

    // Aclarar pantalla (alpha 1 → 0)
    public IEnumerator FadeIn(float duration)
    {
        // CORREGIDO: Usar el parámetro duration, no fadeDuration
        float time = 0;
        Color color = fadeImage.color;
        float startAlpha = color.a;

        while (time < duration)
        {
            time += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, 0f, time / duration);
            fadeImage.color = color;
            yield return null;
        }

        // Asegurar que termine completamente transparente
        color.a = 0f;
        fadeImage.color = color;
    }

    // Hacer pantalla negra instantáneamente
    public void InstantDark()
    {
        Color color = fadeImage.color;
        color.a = 1f;
        fadeImage.color = color;
    }

    // Hacer pantalla transparente instantáneamente
    public void InstantTransparent()
    {
        Color color = fadeImage.color;
        color.a = 0f;
        fadeImage.color = color;
    }
}