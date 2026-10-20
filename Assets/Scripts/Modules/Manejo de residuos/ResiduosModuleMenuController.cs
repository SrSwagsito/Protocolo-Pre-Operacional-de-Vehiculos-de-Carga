using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Restyles the "Manejo de residuos" module pickers to match the reference design: a title
/// ribbon over two module cards (preview art + colour band + play button) and a single Salir
/// action that returns to the initial menu. The original module buttons keep their SceneButton
/// wiring and stay on top of each card as the click surface.
/// </summary>
public sealed class ResiduosModuleMenuController : MonoBehaviour
{
    private const string InitialMenuScene = "Initial Menu game";

    private static readonly Color RibbonFill = new Color32(138, 90, 52, 255);
    private static readonly Color RibbonEdge = new Color32(107, 66, 38, 255);
    private static readonly Color BandGreen = new Color32(31, 138, 59, 255);
    private static readonly Color BandBlue = new Color32(29, 95, 168, 255);
    private static readonly Color PlayFill = new Color32(245, 176, 30, 255);
    private static readonly Color PlayEdge = new Color32(217, 143, 18, 255);
    private static readonly Color ExitFill = new Color32(193, 53, 43, 255);
    private static readonly Color ExitEdge = new Color32(158, 42, 34, 255);

    private static readonly Vector2 CardSize = new Vector2(218f, 272f);
    private const float CardOffsetX = 120f;
    private const float CardOffsetY = 6f;

    /// <summary>Per-scene configuration: which authored buttons become the two cards.</summary>
    private sealed class SceneSetup
    {
        public string SceneName;
        public string RootName;
        public string Ribbon;
        public string[] Buttons;
        public string[] Titles;   // null entry = reuse the text authored on the original label
        public Color[] Bands;
    }

    private static readonly SceneSetup[] Setups =
    {
        new SceneSetup
        {
            SceneName = "selection residuos memorias",
            RootName = "__MemoryMenu",
            Ribbon = "Selecciona un m\u00f3dulo",
            Buttons = new[] { "pgir nacional", "pgir aeropuerto" },
            Titles = new string[] { null, null },
            Bands = new[] { BandGreen, BandBlue },
        },
        new SceneSetup
        {
            SceneName = "selection residuos",
            RootName = "__ResiduosMenu",
            Ribbon = "Selecciona un m\u00f3dulo",
            Buttons = new[] { "pgir nacional", "pgir aeropuerto" },
            Titles = new[] { "PGIRS Nacional", "PGIRS Aeropuerto" },
            Bands = new[] { BandGreen, BandBlue },
        },
    };

    private SceneSetup setup;
    private Transform root;

    // ------------------------------------------------------------------ install

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Register()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        TryInstall(SceneManager.GetActiveScene());
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryInstall(scene);
    }

    private static SceneSetup FindSetup(string sceneName)
    {
        for (int i = 0; i < Setups.Length; i++)
            if (Setups[i].SceneName == sceneName) return Setups[i];

        return null;
    }

    private static void TryInstall(Scene scene)
    {
        if (!scene.IsValid() || !scene.isLoaded) return;

        SceneSetup found = FindSetup(scene.name);
        if (found == null) return;

        Canvas canvas = FindCanvas(scene);
        if (canvas == null) return;
        if (canvas.transform.Find(found.RootName) != null) return;

        ResiduosModuleMenuController controller =
            canvas.gameObject.AddComponent<ResiduosModuleMenuController>();
        controller.setup = found;
        controller.Initialize(canvas);
    }

    /// <summary>
    /// Resolves a canvas that belongs to <paramref name="scene"/> itself. Searching every loaded
    /// canvas would also match the DontDestroyOnLoad overlay (AudioManager/ScreenFader), and a
    /// menu built there would survive scene changes and stay stuck on screen.
    /// </summary>
    private static Canvas FindCanvas(Scene scene)
    {
        Canvas fallback = null;

        foreach (GameObject rootObject in scene.GetRootGameObjects())
        {
            foreach (Canvas candidate in rootObject.GetComponentsInChildren<Canvas>(true))
            {
                if (!candidate.isActiveAndEnabled) continue;
                if (candidate.renderMode == RenderMode.ScreenSpaceOverlay) return candidate;
                if (fallback == null) fallback = candidate;
            }
        }

        return fallback;
    }

    private void Initialize(Canvas canvas)
    {
        root = EPPStyleKit.NewRect(canvas.transform, setup.RootName, Vector2.zero, Vector2.zero);
        RectTransform rootRect = (RectTransform)root;
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        BuildTitle();
        BuildCards(canvas.transform);
        BuildExit();
    }

    // -------------------------------------------------------------------- title

    private void BuildTitle()
    {
        EPPStyleKit.Card(root, "TitleRibbon", new Vector2(0f, 186f), new Vector2(400f, 64f), 26,
            RibbonFill, RibbonEdge, false, true);

        Transform ribbon = root.Find("TitleRibbon");
        if (ribbon != null)
        {
            EPPStyleKit.Label(ribbon, "TitleLabel", setup.Ribbon, new Vector2(0f, 2f),
                new Vector2(360f, 44f), 26f, EPPStyleKit.PureWhite, true, TextAlignmentOptions.Center);
            ribbon.SetAsLastSibling();
        }
    }

    // -------------------------------------------------------------------- cards

    private void BuildCards(Transform canvasTransform)
    {
        for (int i = 0; i < setup.Buttons.Length; i++)
        {
            float x = i == 0 ? -CardOffsetX : CardOffsetX;
            string title = setup.Titles != null && i < setup.Titles.Length ? setup.Titles[i] : null;
            Color band = setup.Bands != null && i < setup.Bands.Length ? setup.Bands[i] : BandGreen;
            BuildCard(canvasTransform, setup.Buttons[i], new Vector2(x, CardOffsetY), title, band);
        }
    }

    private void BuildCard(Transform canvasTransform, string buttonName, Vector2 position, string title,
        Color bandColor)
    {
        Transform original = FindDeep(canvasTransform, buttonName);
        if (original == null) return;

        TMP_Text originalLabel = original.GetComponentInChildren<TMP_Text>(true);
        if (string.IsNullOrWhiteSpace(title) && originalLabel != null &&
            !string.IsNullOrWhiteSpace(originalLabel.text))
            title = originalLabel.text.Trim();

        Image originalImage = original.GetComponent<Image>();
        Sprite preview = originalImage != null ? originalImage.sprite : null;

        RectTransform card = EPPStyleKit.NewRect(root, "Card_" + buttonName, position, CardSize);
        EPPStyleKit.Card(card, "Frame", Vector2.zero, CardSize, 22, EPPStyleKit.CardWhite,
            EPPStyleKit.BorderSoft, false, true);

        // Preview art reuses the sprite already assigned to the module button.
        Image previewImage = EPPStyleKit.NewImage(card, "Preview", new Vector2(0f, 43f),
            new Vector2(200f, 168f), preview, EPPStyleKit.PureWhite, false, false);
        previewImage.preserveAspect = false;

        EPPStyleKit.Card(card, "Band", new Vector2(0f, -84f), new Vector2(200f, 86f), 18, bandColor, null,
            false, false);

        EPPStyleKit.Card(card, "Play", new Vector2(0f, -96f), new Vector2(62f, 34f), 16, PlayFill, PlayEdge,
            false, false);

        Transform play = card.Find("Play");
        if (play != null)
        {
            EPPStyleKit.NewImage(play, "PlayIcon", new Vector2(1f, 0f), new Vector2(14f, 16f),
                EPPStyleKit.Triangle(), EPPStyleKit.PureWhite, false, false);
        }

        EPPStyleKit.Label(card, "Title", title ?? buttonName, new Vector2(0f, -62f), new Vector2(186f, 24f), 17f,
            EPPStyleKit.PureWhite, true, TextAlignmentOptions.Center);

        // The authored button becomes an invisible full-card hit area, keeping its SceneButton target.
        RectTransform buttonRect = (RectTransform)original;
        buttonRect.SetParent(card, false);
        buttonRect.anchorMin = buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = Vector2.zero;
        buttonRect.sizeDelta = CardSize;
        buttonRect.localScale = Vector3.one;

        if (originalImage != null)
        {
            Color transparent = originalImage.color;
            transparent.a = 0f;
            originalImage.color = transparent;
            originalImage.raycastTarget = true;
        }

        if (originalLabel != null) originalLabel.gameObject.SetActive(false);

        buttonRect.SetAsLastSibling();
    }

    // --------------------------------------------------------------------- exit

    private void BuildExit()
    {
        Button exit = EPPStyleKit.NewButton(root, "ExitButton", new Vector2(296f, -184f), new Vector2(150f, 46f),
            23, ExitFill, ExitEdge, EPPStyleKit.PureWhite, "Salir", 17f);
        exit.onClick.AddListener(ExitToInitialMenu);
        exit.transform.SetAsLastSibling();
    }

    private static void ExitToInitialMenu()
    {
        // Prefer the project loader (fade + button audio) but never depend on its singletons.
        if (SceneLoader.Instance != null && AudioManager.Instance != null)
            SceneLoader.Load(InitialMenuScene);
        else
            SceneManager.LoadScene(InitialMenuScene);
    }

    // -------------------------------------------------------------------- utils

    private static Transform FindDeep(Transform parent, string name)
    {
        if (parent == null) return null;
        if (parent.name == name) return parent;

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform found = FindDeep(parent.GetChild(i), name);
            if (found != null) return found;
        }

        return null;
    }
}
