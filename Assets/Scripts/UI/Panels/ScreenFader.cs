using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Persistent gradient overlay. Keeps the original FadeIn / FadeOut API and
/// adds a left-to-right sweep that covers the scene before loading the next one.
/// </summary>
[DefaultExecutionOrder(-1000)]
public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance;

    [SerializeField] private Image fadeImage;
    [SerializeField, Min(0f)] private float fadeDuration = 1f;

    [Header("Degradado")]
    [SerializeField] private Color topColor = new Color32(11, 30, 58, 255);
    [SerializeField] private Color bottomColor = new Color32(21, 87, 126, 255);

    [Header("Barrido al cambiar de escena")]
    [SerializeField, Min(0f)] private float wipeDuration = 0.45f;

    public bool IsTransitioning { get; private set; }

    private GameObject overlayRoot;
    private Texture2D gradientTexture;
    private Sprite gradientSprite;
    private int animationVersion;
    private bool hasVisualOperation;

    public static ScreenFader GetOrCreate()
    {
        if (Instance == null)
            return new GameObject("ScreenFader").AddComponent<ScreenFader>();

        if (!Instance.gameObject.activeSelf) Instance.gameObject.SetActive(true);
        Instance.enabled = true;
        return Instance;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        if (transform.parent != null) transform.SetParent(null, true);
        DontDestroyOnLoad(gameObject);
        BuildOverlay();
        UseFullImage();
        SetAlpha(0f);
    }

    private void Start()
    {
        // A menu can request a sweep on the same frame this component is created.
        // In that case, do not start another animation over the sweep.
        if (hasVisualOperation || IsTransitioning) return;
        InstantDark();
        StartCoroutine(FadeIn(fadeDuration));
    }

    private void BuildOverlay()
    {
        // A separate, persistent overlay always covers the full viewport.
        overlayRoot = new GameObject("__ScreenFaderCanvas", typeof(RectTransform), typeof(Canvas),
            typeof(GraphicRaycaster));
        DontDestroyOnLoad(overlayRoot);
        Canvas canvas = overlayRoot.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32760;

        if (fadeImage == null)
            fadeImage = new GameObject("Gradient", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image))
                .GetComponent<Image>();
        fadeImage.transform.SetParent(overlayRoot.transform, false);
        fadeImage.gameObject.SetActive(true);
        fadeImage.enabled = true;

        RectTransform rect = fadeImage.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;

        const int height = 128;
        gradientTexture = new Texture2D(2, height, TextureFormat.RGBA32, false)
        {
            name = "NinteraseoFadeGradient",
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };
        Color[] pixels = new Color[2 * height];
        Color top = topColor;
        Color bottom = bottomColor;
        top.a = bottom.a = 1f;
        for (int y = 0; y < height; y++)
        {
            Color color = Color.Lerp(bottom, top, y / (height - 1f));
            pixels[y * 2] = pixels[y * 2 + 1] = color;
        }
        gradientTexture.SetPixels(pixels);
        gradientTexture.Apply(false, true);
        gradientSprite = Sprite.Create(gradientTexture, new Rect(0f, 0f, 2f, height),
            new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
        fadeImage.sprite = gradientSprite;
        fadeImage.overrideSprite = null;
        fadeImage.material = null;
        fadeImage.maskable = false;
        fadeImage.preserveAspect = false;
        fadeImage.alphaHitTestMinimumThreshold = 0f;
        fadeImage.canvasRenderer.SetAlpha(1f);
    }

    // Original API: opacity fades, now using the gradient and unscaled time.
    public IEnumerator FadeOut(float duration) => FadeAlpha(1f, duration);
    public IEnumerator FadeIn(float duration) => FadeAlpha(0f, duration);

    private IEnumerator FadeAlpha(float target, float duration)
    {
        // Scene startup scripts must not replace a sweep already in progress.
        if (IsTransitioning) yield break;
        hasVisualOperation = true;
        int version = ++animationVersion;
        UseFullImage();
        float startAlpha = fadeImage.color.a;
        float elapsed = 0f;
        while (elapsed < duration && version == animationVersion)
        {
            elapsed += Time.unscaledDeltaTime;
            SetAlpha(Mathf.Lerp(startAlpha, target, Mathf.Clamp01(elapsed / duration)));
            yield return null;
        }
        if (version == animationVersion) SetAlpha(target);
    }

    public void InstantDark()
    {
        if (IsTransitioning) return;
        hasVisualOperation = true;
        animationVersion++;
        UseFullImage();
        SetAlpha(1f);
    }

    public void InstantTransparent()
    {
        if (IsTransitioning) return;
        hasVisualOperation = true;
        animationVersion++;
        UseFullImage();
        SetAlpha(0f);
    }

    public bool WipeToScene(string sceneName, System.Action onFailure = null)
    {
        if (IsTransitioning || !isActiveAndEnabled) return false;
        if (string.IsNullOrWhiteSpace(sceneName) || !Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError("No se puede cargar la escena '" + sceneName + "'. Revisa la lista de escenas de compilacion.", this);
            return false;
        }

        IsTransitioning = true;
        hasVisualOperation = true;
        animationVersion++;
        StartCoroutine(WipeSceneRoutine(sceneName, onFailure));
        return true;
    }

    private IEnumerator WipeSceneRoutine(string sceneName, System.Action onFailure)
    {
        fadeImage.type = Image.Type.Filled;
        fadeImage.fillMethod = Image.FillMethod.Horizontal;
        fadeImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        fadeImage.fillAmount = 0f;
        SetAlpha(1f);
        yield return AnimateFill(0f, 1f, wipeDuration);

        // Load only after the gradient completely covers the current scene.
        AsyncOperation loading = null;
        try
        {
            loading = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        }
        catch (System.Exception exception)
        {
            Debug.LogException(exception, this);
        }
        if (loading != null)
        {
            yield return loading;
            yield return null;
            Canvas.ForceUpdateCanvases();
        }

        // Shrinking from the right reveals the menu from left to right too.
        fadeImage.fillOrigin = (int)Image.OriginHorizontal.Right;
        yield return AnimateFill(1f, 0f, wipeDuration);
        IsTransitioning = false;
        UseFullImage();
        SetAlpha(0f);
        if (loading == null) onFailure?.Invoke();
    }

    private IEnumerator AnimateFill(float from, float to, float duration)
    {
        float elapsed = 0f;
        fadeImage.fillAmount = from;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            fadeImage.fillAmount = Mathf.Lerp(from, to, progress);
            yield return null;
        }
        fadeImage.fillAmount = to;
    }

    private void UseFullImage()
    {
        fadeImage.type = Image.Type.Simple;
        fadeImage.fillAmount = 1f;
    }

    private void SetAlpha(float alpha)
    {
        fadeImage.color = new Color(1f, 1f, 1f, Mathf.Clamp01(alpha));
        // An invisible fade must never block the menu's buttons or slider.
        fadeImage.raycastTarget = IsTransitioning || alpha > 0.001f;
    }

    private void OnDisable()
    {
        if (Instance != this || fadeImage == null) return;
        StopAllCoroutines();
        animationVersion++;
        IsTransitioning = false;
        UseFullImage();
        SetAlpha(0f);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        if (overlayRoot != null) Destroy(overlayRoot);
        if (gradientSprite != null) Destroy(gradientSprite);
        if (gradientTexture != null) Destroy(gradientTexture);
    }
}
