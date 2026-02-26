using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance;
    [SerializeField] Image fadeImage;
    [SerializeField] float fadeDuration;

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
        StartCoroutine(FadeIn(0f));
    }

    public IEnumerator FadeOut(float duration)
    {
        float time = 0;
        Color color = fadeImage.color;

        while (time < duration)
        {
            time += Time.deltaTime;
            color.a = Mathf.Lerp(0, 1, time / duration);
            fadeImage.color = color;
            yield return null;
        }

        color.a = 1;
        fadeImage.color = color;
    }

    public IEnumerator FadeIn(float _)
    {
        float duration = fadeDuration;
        float elapsedTime = 0f;
        Color color = fadeImage.color;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(1, 0, elapsedTime / duration);
            fadeImage.color = color;
            yield return null;
        }
    }

    public void InstantDark()
    {
        Color color = fadeImage.color;
        color.a = 1;
        fadeImage.color = color;
    }

    public void InstantTransparent()
    {
        Color color = fadeImage.color;
        color.a = 0;
        fadeImage.color = color;
    }
}