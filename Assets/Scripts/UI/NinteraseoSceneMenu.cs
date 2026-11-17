using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

/// <summary>
/// Add to an empty GameObject in each game scene. Creates a settings panel and
/// a top-right AJUSTES button at runtime, using the project's EPPStyleKit.
/// VOLVER AL MENU lives inside settings and uses ScreenFader's gradient wipe.
/// Shares the audio preferences used by InitialMenuStyleController.
/// </summary>
[DisallowMultipleComponent]
public sealed class NinteraseoSceneMenu : MonoBehaviour
{
    [Header("Escena del menu inicial")]
    [SerializeField] private string initialMenuScene = "Initial Menu game";

    [Header("Estilo del menu")]
    [SerializeField] private TMP_FontAsset menuFont;
    [SerializeField] private int canvasSortingOrder = 100;

    private const string VolumeKey = "Ninteraseo_MasterVolume";
    private const string MutedKey = "Ninteraseo_Muted";
    private static readonly Vector2 CardSize = new Vector2(900f, 620f);
    private static readonly Vector2 ToolbarSize = new Vector2(252f, 88f);
    private static readonly Color Navy = new Color32(11, 30, 58, 255);
    private static readonly Color Blue = new Color32(47, 127, 216, 255);
    private static readonly Color Lime = new Color32(120, 214, 60, 255);
    private static readonly Color LimeEdge = new Color32(176, 240, 110, 255);
    private static readonly Color SubText = new Color32(201, 214, 232, 255);

    private GameObject uiRoot;
    private RectTransform canvasRect;
    private RectTransform safeAreaRoot;
    private RectTransform toolbar;
    private RectTransform overlay;
    private RectTransform dimRect;
    private RectTransform card;
    private Slider volumeSlider;
    private TMP_Text volumeLabel;
    private TMP_Text muteLabel;
    private Button muteButton;
    private Button settingsButton;
    private Button returnButton;
    private Button closeButton;
    private GameObject previousSelection;
    private CursorLockMode previousCursorLock;
    private bool previousCursorVisible;
    private float volume;
    private bool muted;
    private bool audioDirty;
    private bool returningToMenu;
    private Rect lastSafeArea;
    private Vector2 lastCanvasSize;
    private Vector2 lastScreenSize;

    private void Awake()
    {
        if (menuFont == null) menuFont = TMP_Settings.defaultFontAsset;
        EnsureEventSystem();
        LoadAudioSettings();
        BuildMenu();
    }

    private void Start()
    {
        Canvas.ForceUpdateCanvases();
        UpdateLayout();
    }

    private void OnEnable()
    {
        if (uiRoot != null) uiRoot.SetActive(true);
    }

    private void LateUpdate() => UpdateLayout();

    // Public methods can also be called from your own buttons or scripts.
    public void ToggleSettings()
    {
        if (overlay == null || returningToMenu) return;
        if (overlay.gameObject.activeSelf) HideSettings();
        else ShowSettings();
    }

    public void ShowSettings()
    {
        if (overlay == null || !isActiveAndEnabled || returningToMenu || overlay.gameObject.activeSelf) return;
        EnsureEventSystem();
        previousSelection = EventSystem.current.currentSelectedGameObject;
        previousCursorLock = Cursor.lockState;
        previousCursorVisible = Cursor.visible;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        LoadAudioSettings();
        RefreshControls();
        overlay.gameObject.SetActive(true);
        toolbar.gameObject.SetActive(false);
        Canvas.ForceUpdateCanvases();
        UpdateLayout();
        volumeSlider.Select();
    }

    public void HideSettings()
    {
        SaveAudioSettings();
        if (overlay == null || !overlay.gameObject.activeSelf) return;
        overlay.gameObject.SetActive(false);
        toolbar.gameObject.SetActive(!returningToMenu);
        Cursor.lockState = previousCursorLock;
        Cursor.visible = previousCursorVisible;
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(
                previousSelection != null && previousSelection.activeInHierarchy ? previousSelection : null);
    }

    public void ReturnToInitialMenu()
    {
        if (returningToMenu) return;
        if (string.IsNullOrWhiteSpace(initialMenuScene) || !Application.CanStreamedLevelBeLoaded(initialMenuScene))
        {
            Debug.LogError("No se puede cargar el menu inicial. Agrega la escena '" + initialMenuScene +
                "' a la lista de escenas de la compilacion y comprueba su nombre en el Inspector.", this);
            return;
        }

        ScreenFader fader = ScreenFader.GetOrCreate();
        if (fader.IsTransitioning) return;
        returningToMenu = true;
        HideSettings();
        returnButton.interactable = false;
        settingsButton.interactable = false;
        // The initial menu must also work if the current game was paused.
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        // The persistent fader owns the transition, so unloading this scene
        // cannot interrupt the sweep that reveals the initial menu.
        if (!fader.WipeToScene(initialMenuScene, RestoreReturnControls)) RestoreReturnControls();
    }

    private void RestoreReturnControls()
    {
        if (this == null || uiRoot == null) return;
        returningToMenu = false;
        returnButton.interactable = true;
        settingsButton.interactable = true;
        toolbar.gameObject.SetActive(true);
    }

    private void BuildMenu()
    {
        if (uiRoot != null) return;
        uiRoot = new GameObject("NinteraseoSceneMenuUI", typeof(RectTransform), typeof(Canvas),
            typeof(CanvasScaler), typeof(GraphicRaycaster));
        if (uiRoot.scene != gameObject.scene) SceneManager.MoveGameObjectToScene(uiRoot, gameObject.scene);
        canvasRect = uiRoot.GetComponent<RectTransform>();
        Canvas canvas = uiRoot.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = canvasSortingOrder;
        CanvasScaler scaler = uiRoot.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        safeAreaRoot = EPPStyleKit.NewRect(canvasRect, "SafeArea", Vector2.zero, Vector2.zero);
        Stretch(safeAreaRoot);
        BuildSettingsPanel();
        BuildToolbar();
        RefreshControls();
    }

    private void BuildToolbar()
    {
        toolbar = EPPStyleKit.NewRect(safeAreaRoot, "TopRightMenu", Vector2.zero, ToolbarSize);
        toolbar.anchorMin = toolbar.anchorMax = Vector2.one;
        toolbar.pivot = Vector2.one;
        toolbar.sizeDelta = ToolbarSize;
        toolbar.anchoredPosition = new Vector2(-40f, -40f);

        settingsButton = MakeButton(toolbar, "SettingsButton", "AJUSTES", Vector2.zero,
            ToolbarSize, Navy, Color.white, Blue, 30f);
        settingsButton.onClick.AddListener(ToggleSettings);

        Sprite gear = Resources.Load<Sprite>("MainMenu/MainMenu_IconGear");
        if (gear != null)
        {
            Image icon = EPPStyleKit.NewImage(settingsButton.transform, "Icon", new Vector2(-82f, 0f),
                new Vector2(44f, 44f), gear, Color.white, false, false);
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            TMP_Text caption = settingsButton.GetComponentInChildren<TMP_Text>(true);
            caption.rectTransform.anchoredPosition = new Vector2(30f, 0f);
            caption.rectTransform.sizeDelta = new Vector2(144f, 60f);
            caption.fontSizeMax = 30f;
            caption.fontSizeMin = 26f;
        }

        toolbar.SetAsLastSibling();
    }

    private void BuildSettingsPanel()
    {
        overlay = EPPStyleKit.NewRect(safeAreaRoot, "SettingsOverlay", Vector2.zero, Vector2.zero);
        Stretch(overlay);
        overlay.gameObject.SetActive(false);

        // Dim covers the entire screen, including the area outside the safe area.
        Image dim = EPPStyleKit.NewImage(overlay, "Dim", Vector2.zero, Vector2.zero, null,
            new Color32(2, 10, 25, 220), false, false);
        dimRect = dim.rectTransform;
        Stretch(dimRect);
        Button dimButton = dim.gameObject.AddComponent<Button>();
        dimButton.targetGraphic = dim;
        dimButton.transition = Selectable.Transition.None;
        Navigation noNavigation = new Navigation { mode = Navigation.Mode.None };
        dimButton.navigation = noNavigation;
        dimButton.onClick.AddListener(HideSettings);
        EnableControlRaycasts(dimButton);

        Image frame = EPPStyleKit.NewImage(overlay, "SettingsCard", Vector2.zero, CardSize,
            EPPStyleKit.Rounded(34), Blue, true, false);
        card = frame.rectTransform;
        card.anchorMin = card.anchorMax = new Vector2(0.5f, 0.5f);
        card.pivot = new Vector2(0.5f, 0.5f);
        frame.raycastTarget = true;
        Image surface = EPPStyleKit.NewImage(card, "Surface", Vector2.zero, CardSize - new Vector2(6f, 6f),
            EPPStyleKit.Rounded(32), Navy, true, false);
        surface.raycastTarget = false;

        Label(card, "Title", "AJUSTES", new Vector2(0f, 230f), new Vector2(740f, 72f), 54f, Color.white);
        Label(card, "Subtitle", "Controla el sonido del juego", new Vector2(0f, 172f),
            new Vector2(740f, 40f), 28f, SubText, false);
        Image rule = EPPStyleKit.NewImage(card, "Rule", new Vector2(0f, 136f), new Vector2(772f, 4f),
            EPPStyleKit.Rounded(2), new Color32(46, 190, 90, 255), true, false);
        rule.raycastTarget = false;

        Label(card, "VolumeTitle", "VOLUMEN GENERAL", new Vector2(-114f, 86f), new Vector2(544f, 48f),
            32f, SubText).alignment = TextAlignmentOptions.Left;
        volumeLabel = Label(card, "VolumeValue", "100%", new Vector2(326f, 86f), new Vector2(120f, 48f),
            32f, LimeEdge);
        volumeLabel.alignment = TextAlignmentOptions.Right;
        BuildSlider();

        Label(card, "SoundTitle", "SONIDO", new Vector2(-226f, -90f), new Vector2(320f, 64f),
            32f, SubText).alignment = TextAlignmentOptions.Left;
        muteButton = MakeButton(card, "MuteButton", "ACTIVADO", new Vector2(254f, -90f),
            new Vector2(264f, 64f), Lime, Navy, LimeEdge, 30f);
        muteLabel = muteButton.GetComponentInChildren<TMP_Text>(true);
        muteButton.onClick.AddListener(ToggleMute);

        closeButton = MakeButton(card, "CloseSettings", "CONTINUAR", new Vector2(-190f, -226f),
            new Vector2(352f, 80f), Lime, Navy, LimeEdge, 30f);
        closeButton.onClick.AddListener(HideSettings);

        returnButton = MakeButton(card, "ReturnToInitialMenu", "VOLVER AL MEN\u00da", new Vector2(190f, -226f),
            new Vector2(352f, 80f), Navy, Color.white, Blue, 28f);
        returnButton.onClick.AddListener(ReturnToInitialMenu);

        NavigateVertically(volumeSlider, returnButton, muteButton);
        NavigateVertically(muteButton, volumeSlider, closeButton);
        NavigateVertically(closeButton, muteButton, volumeSlider, returnButton, returnButton);
        NavigateVertically(returnButton, muteButton, volumeSlider, closeButton, closeButton);
    }

    private void BuildSlider()
    {
        RectTransform sliderRect = EPPStyleKit.NewRect(card, "MasterVolumeSlider", new Vector2(0f, 8f),
            new Vector2(772f, 76f));
        Image hitArea = sliderRect.gameObject.AddComponent<Image>();
        hitArea.color = Color.clear;
        EPPStyleKit.NewImage(sliderRect, "Track", Vector2.zero, new Vector2(772f, 24f),
            EPPStyleKit.Rounded(12), new Color32(5, 17, 38, 255), true, false);

        RectTransform fillArea = EPPStyleKit.NewRect(sliderRect, "FillArea", Vector2.zero, Vector2.zero);
        fillArea.anchorMin = new Vector2(0f, 0.5f);
        fillArea.anchorMax = new Vector2(1f, 0.5f);
        fillArea.offsetMin = new Vector2(24f, -8f);
        fillArea.offsetMax = new Vector2(-24f, 8f);
        Image fill = EPPStyleKit.NewImage(fillArea, "Fill", Vector2.zero, Vector2.zero,
            EPPStyleKit.Rounded(8), Lime, true, false);
        Stretch(fill.rectTransform);

        RectTransform handleArea = EPPStyleKit.NewRect(sliderRect, "HandleArea", Vector2.zero, Vector2.zero);
        handleArea.anchorMin = new Vector2(0f, 0.5f);
        handleArea.anchorMax = new Vector2(1f, 0.5f);
        handleArea.offsetMin = new Vector2(24f, -22f);
        handleArea.offsetMax = new Vector2(-24f, 22f);
        Image handle = EPPStyleKit.NewImage(handleArea, "Handle", Vector2.zero, new Vector2(44f, 0f),
            EPPStyleKit.Rounded(22), LimeEdge, true, false);
        handle.rectTransform.sizeDelta = new Vector2(44f, 0f);
        handle.rectTransform.anchoredPosition = Vector2.zero;

        volumeSlider = sliderRect.gameObject.AddComponent<Slider>();
        volumeSlider.fillRect = fill.rectTransform;
        volumeSlider.handleRect = handle.rectTransform;
        volumeSlider.targetGraphic = handle;
        volumeSlider.direction = Slider.Direction.LeftToRight;
        volumeSlider.minValue = 0f;
        volumeSlider.maxValue = 1f;
        volumeSlider.wholeNumbers = false;
        volumeSlider.SetValueWithoutNotify(volume);
        EnableControlRaycasts(volumeSlider);
        volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    private void LoadAudioSettings()
    {
        volume = Mathf.Clamp01(PlayerPrefs.GetFloat(VolumeKey, 1f));
        muted = PlayerPrefs.GetInt(MutedKey, 0) == 1;
        ApplyAudio();
    }

    private void ApplyAudio() => AudioListener.volume = muted ? 0f : volume;

    private void SetVolume(float value)
    {
        volume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(VolumeKey, volume);
        audioDirty = true;
        ApplyAudio();
        RefreshControls();
    }

    private void ToggleMute()
    {
        muted = !muted;
        PlayerPrefs.SetInt(MutedKey, muted ? 1 : 0);
        audioDirty = true;
        ApplyAudio();
        RefreshControls();
    }

    private void RefreshControls()
    {
        if (volumeSlider != null) volumeSlider.SetValueWithoutNotify(volume);
        if (volumeLabel != null) volumeLabel.text = Mathf.RoundToInt(volume * 100f) + "%";
        if (muteButton != null) muteButton.GetComponent<Image>().color = muted ? Blue : Lime;
        if (muteLabel != null)
        {
            muteLabel.text = muted ? "SILENCIADO" : "ACTIVADO";
            muteLabel.color = muted ? Color.white : Navy;
        }
    }

    private void SaveAudioSettings()
    {
        if (!audioDirty) return;
        PlayerPrefs.Save();
        audioDirty = false;
    }

    private void UpdateLayout()
    {
        if (uiRoot == null || Screen.width <= 0 || Screen.height <= 0) return;
        Rect safe = Screen.safeArea;
        Vector2 size = canvasRect.rect.size;
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
        if (safe == lastSafeArea && size == lastCanvasSize && screenSize == lastScreenSize) return;
        safeAreaRoot.anchorMin = new Vector2(safe.xMin / Screen.width, safe.yMin / Screen.height);
        safeAreaRoot.anchorMax = new Vector2(safe.xMax / Screen.width, safe.yMax / Screen.height);
        safeAreaRoot.offsetMin = safeAreaRoot.offsetMax = Vector2.zero;

        // Extend the dim layer beyond the safe area without shifting the card.
        dimRect.offsetMin = new Vector2(-safe.xMin / Screen.width * size.x, -safe.yMin / Screen.height * size.y);
        dimRect.offsetMax = new Vector2((Screen.width - safe.xMax) / Screen.width * size.x,
            (Screen.height - safe.yMax) / Screen.height * size.y);

        Vector2 available = safeAreaRoot.rect.size - new Vector2(96f, 96f);
        if (available.x <= 0f || available.y <= 0f) return;
        float scale = Mathf.Min(1f, Mathf.Min(available.x / CardSize.x, available.y / CardSize.y));
        card.localScale = Vector3.one * scale;
        card.anchoredPosition = Vector2.zero;
        float toolbarScale = Mathf.Min(1f, (safeAreaRoot.rect.width - 80f) / ToolbarSize.x);
        toolbar.localScale = Vector3.one * Mathf.Max(0.01f, toolbarScale);
        lastSafeArea = safe;
        lastCanvasSize = size;
        lastScreenSize = screenSize;
    }

    private void OnDisable()
    {
        HideSettings();
        if (uiRoot != null) uiRoot.SetActive(false);
    }

    private void OnDestroy()
    {
        SaveAudioSettings();
        if (uiRoot != null) Destroy(uiRoot);
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused) SaveAudioSettings();
    }

    private void OnApplicationQuit() => SaveAudioSettings();

    private Button MakeButton(Transform parent, string name, string caption, Vector2 position, Vector2 size,
        Color fillColor, Color textColor, Color edgeColor, float fontSize)
    {
        Image fill = EPPStyleKit.NewImage(parent, name, position, size, EPPStyleKit.Rounded(22), fillColor, true, false);
        fill.rectTransform.anchorMin = fill.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        fill.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        fill.rectTransform.sizeDelta = size;
        fill.rectTransform.anchoredPosition = position;
        EPPStyleKit.NewImage(fill.transform, "Rim", Vector2.zero, size + new Vector2(5f, 5f),
            EPPStyleKit.Rounded(24, 3), edgeColor, true, false);
        TMP_Text text = Label(fill.transform, "Text", caption, Vector2.zero, size - new Vector2(40f, 12f),
            fontSize, textColor);
        text.enableAutoSizing = true;
        text.fontSizeMin = Mathf.Max(22f, fontSize - 4f);
        text.fontSizeMax = fontSize;
        Button button = fill.gameObject.AddComponent<Button>();
        button.targetGraphic = fill;
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.86f, 0.96f, 1f, 1f);
        colors.pressedColor = new Color(0.72f, 0.88f, 0.98f, 1f);
        colors.fadeDuration = 0.08f;
        button.colors = colors;
        EnableControlRaycasts(button);
        return button;
    }

    private TextMeshProUGUI Label(Transform parent, string name, string caption, Vector2 position, Vector2 size,
        float fontSize, Color color, bool bold = true)
    {
        TextMeshProUGUI text = EPPStyleKit.Label(parent, name, caption, position, size, fontSize, color, false,
            TextAlignmentOptions.Center);
        if (menuFont != null) text.font = menuFont;
        text.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.raycastTarget = false;
        return text;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    private static void EnableControlRaycasts(Selectable control)
    {
        foreach (Graphic graphic in control.GetComponentsInChildren<Graphic>(true)) graphic.raycastTarget = false;
        Graphic target = control.GetComponent<Graphic>();
        if (target != null) target.raycastTarget = true;
        control.interactable = true;
    }

    private static void NavigateVertically(Selectable control, Selectable up, Selectable down,
        Selectable left = null, Selectable right = null)
    {
        Navigation navigation = new Navigation { mode = Navigation.Mode.Explicit };
        navigation.selectOnUp = up;
        navigation.selectOnDown = down;
        navigation.selectOnLeft = left;
        navigation.selectOnRight = right;
        control.navigation = navigation;
    }

    private static void EnsureEventSystem()
    {
        EventSystem system = EventSystem.current;
        if (system == null) system = new GameObject("EventSystem", typeof(EventSystem)).GetComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        foreach (StandaloneInputModule legacy in system.GetComponents<StandaloneInputModule>()) legacy.enabled = false;
#endif
        foreach (BaseInputModule existing in system.GetComponents<BaseInputModule>())
            if (existing.isActiveAndEnabled) return;
#if ENABLE_INPUT_SYSTEM
        InputSystemUIInputModule module = system.GetComponent<InputSystemUIInputModule>();
        if (module == null) module = system.gameObject.AddComponent<InputSystemUIInputModule>();
        module.enabled = true;
        if (module.actionsAsset == null) module.AssignDefaultActions();
#else
        StandaloneInputModule module = system.GetComponent<StandaloneInputModule>();
        if (module == null) module = system.gameObject.AddComponent<StandaloneInputModule>();
        module.enabled = true;
#endif
        system.UpdateModules();
    }
}
