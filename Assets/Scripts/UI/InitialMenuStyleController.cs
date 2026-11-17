using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

/// <summary>
/// Restyles the "Initial Menu game" scene (start screen + Game selection) to the
/// "Mi consola Ninteraseo" reference: dark space backdrop, gradient title, big green JUGAR
/// button and navy secondary buttons. Authored buttons are reused so their persistent
/// listeners (Game selection SetActive, MenuButtons.LoadScene) keep working.
/// </summary>
public sealed class InitialMenuStyleController : MonoBehaviour
{
    private const string TargetScene = "Initial Menu game";
    private const string RootMarker = "__MenuStyle";
    private const string ArtFolder = "MainMenu/";

    private static readonly Color Navy = new Color32(11, 30, 58, 255);
    private static readonly Color NavyPanel = new Color32(14, 35, 64, 240);
    private static readonly Color BlueEdge = new Color32(47, 127, 216, 255);
    private static readonly Color Lime = new Color32(120, 214, 60, 255);
    private static readonly Color LimeEdge = new Color32(176, 240, 110, 255);
    private static readonly Color LimeGlow = new Color32(120, 230, 70, 22);
    private static readonly Color GreenLine = new Color32(46, 190, 90, 255);
    private static readonly Color SubText = new Color32(201, 214, 232, 255);
    private static readonly Color TitleBottom = new Color32(150, 196, 255, 255);
    private static readonly Color TitleOutline = new Color32(28, 79, 184, 255);

    private Transform canvas;
    private TMP_FontAsset font;
    private Sprite background, leaf, iconBook, iconTrophy, iconGear;
    private const string MasterVolumeKey = "Ninteraseo_MasterVolume";
    private const string MutedKey = "Ninteraseo_Muted";
    private bool isMuted;
    private float savedVolume = 1f;
    private TMP_Text volumeValueLabel;
    private TMP_Text muteButtonLabel;
    private RectTransform settingsOverlay;
    private RectTransform settingsCard;
    private Slider masterVolumeSlider;
    private Button muteButton;
    private Button closeSettingsButton;
    private GameObject previousSelection;
    private bool audioSettingsDirty;
    private static readonly Vector2 SettingsCardSize = new Vector2(900f, 600f);

    // ------------------------------------------------------------------ install

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Register()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        TryInstall(SceneManager.GetActiveScene());
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => TryInstall(scene);

    private static void TryInstall(Scene scene)
    {
        if (!scene.IsValid() || !scene.isLoaded || scene.name != TargetScene) return;

        // Resolve the canvas from this scene's own roots (never a global search; the
        // DontDestroyOnLoad fader canvas would otherwise be picked up).
        Transform canvasTransform = null;
        foreach (GameObject rootObject in scene.GetRootGameObjects())
        {
            if (rootObject.name != "UI") continue;
            canvasTransform = rootObject.transform.Find("Canvas");
            break;
        }

        if (canvasTransform == null || canvasTransform.Find(RootMarker) != null) return;

        InitialMenuStyleController controller = canvasTransform.gameObject.AddComponent<InitialMenuStyleController>();
        controller.canvas = canvasTransform;
        controller.Initialize();
    }

    private void Initialize()
    {
        GameObject marker = new GameObject(RootMarker, typeof(RectTransform));
        marker.transform.SetParent(canvas, false);
        marker.SetActive(false);

        background = Resources.Load<Sprite>(ArtFolder + "MainMenu_Background");
        leaf = Resources.Load<Sprite>(ArtFolder + "MainMenu_Leaf");
        iconBook = Resources.Load<Sprite>(ArtFolder + "MainMenu_IconBook");
        iconTrophy = Resources.Load<Sprite>(ArtFolder + "MainMenu_IconTrophy");
        iconGear = Resources.Load<Sprite>(ArtFolder + "MainMenu_IconGear");

        Transform title = canvas.Find("InitialPanel/ProyectTitle");
        TMP_Text titleText = title != null ? title.GetComponent<TMP_Text>() : null;
        font = titleText != null ? titleText.font : TMP_Settings.defaultFontAsset;

        Transform initialPanel = canvas.Find("InitialPanel");
        Transform gameSelection = canvas.Find("Game selection");

        EnsureUIInput();
        ApplySavedAudioSettings();
        if (initialPanel != null) StyleInitialPanel(initialPanel, gameSelection);
        if (gameSelection != null) StyleGameSelection(gameSelection);
    }

    // ------------------------------------------------------------ start screen

    private void StyleInitialPanel(Transform panel, Transform gameSelection)
    {
        // Backdrop replaces the scrolling star field.
        Transform stars = panel.Find("Panel");
        if (stars != null) stars.gameObject.SetActive(false);
        AddBackdrop(panel, Vector2.zero);

        // Logos keep their art, placed in the top corners like the reference.
        Place(panel.Find("Image (1)"), new Vector2(-725f, 372f), new Vector2(300f, 236f));
        Place(panel.Find("Image"), new Vector2(745f, 372f), new Vector2(210f, 242f));

        // Title.
        Transform title = panel.Find("ProyectTitle");
        if (title != null)
        {
            RectTransform rect = Place(title, new Vector2(0f, 205f), new Vector2(1300f, 290f));
            TMP_Text text = title.GetComponent<TMP_Text>();
            // The authored per-character effect recolours glyphs and fights the gradient.
            foreach (MonoBehaviour behaviour in title.GetComponents<MonoBehaviour>())
                if (behaviour.GetType().Name == "TextEffect") behaviour.enabled = false;

            if (text.font != null && text.font.material != null) text.fontSharedMaterial = text.font.material;
            text.text = "MI CONSOLA\nNINTERASEO";
            text.fontSize = 128f;
            text.enableAutoSizing = false;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.lineSpacing = -38f;
            text.color = Color.white;
            text.enableVertexGradient = true;
            text.colorGradient = new VertexGradient(Color.white, Color.white, TitleBottom, TitleBottom);
            text.outlineWidth = 0.14f;
            text.outlineColor = TitleOutline;
            rect.SetAsLastSibling();
        }

        // Subtitle with green rules on both sides.
        RectTransform subtitle = EPPStyleKit.NewRect(panel, "Subtitle", new Vector2(0f, 22f), new Vector2(900f, 60f));
        TextMeshProUGUI sub = Label(subtitle, "Text", "Aprende, juega y cuida el planeta", Vector2.zero,
            new Vector2(700f, 60f), 36f, SubText, FontStyles.Normal);
        sub.characterSpacing = 1f;
        EPPStyleKit.NewImage(subtitle, "RuleLeft", new Vector2(-385f, 0f), new Vector2(80f, 4f),
            EPPStyleKit.Rounded(2), GreenLine, true, false);
        EPPStyleKit.NewImage(subtitle, "RuleRight", new Vector2(385f, 0f), new Vector2(80f, 4f),
            EPPStyleKit.Rounded(2), GreenLine, true, false);

        StylePlayButton(panel);
        BuildSecondaryRow(panel, gameSelection);
    }

    private void StylePlayButton(Transform panel)
    {
        Transform start = panel.Find("ButtonStart");
        if (start == null) return;

        Vector2 size = new Vector2(700f, 168f);
        Vector2 position = new Vector2(-0f, -140f);
        RectTransform rect = Place(start, position, size);

        // Glow sits behind the button as its own sibling so it does not pulse with the label.
        Image glow = EPPStyleKit.NewImage(panel, "PlayGlow", position, size + new Vector2(34f, 28f),
            EPPStyleKit.Rounded(48), LimeGlow, true, false);
        glow.transform.SetSiblingIndex(rect.GetSiblingIndex());

        Image image = start.GetComponent<Image>();
        image.sprite = EPPStyleKit.Rounded(48);
        image.type = Image.Type.Sliced;
        image.color = Lime;

        // Top highlight + light rim for the glossy look.
        EPPStyleKit.NewImage(rect, "Highlight", new Vector2(0f, 44f), new Vector2(size.x - 60f, 46f),
            EPPStyleKit.Rounded(22), new Color(1f, 1f, 1f, 0.10f), true, false);
        EPPStyleKit.NewImage(rect, "Rim", Vector2.zero, size + new Vector2(6f, 6f),
            EPPStyleKit.Rounded(48, 4), LimeEdge, true, false);

        Transform label = start.Find("Text (TMP)");
        if (label != null)
        {
            TMP_Text text = label.GetComponent<TMP_Text>();
            text.text = "JUGAR";
            text.font = font;
            text.enableAutoSizing = false;
            text.fontSize = 92f;
            text.fontStyle = FontStyles.Bold;
            text.color = Navy;
            text.alignment = TextAlignmentOptions.Center;
            RectTransform labelRect = (RectTransform)label;
            labelRect.offsetMin = new Vector2(80f, 0f);
            labelRect.offsetMax = Vector2.zero;
            label.SetAsLastSibling();
        }

        EPPStyleKit.NewImage(rect, "PlayIcon", new Vector2(-170f, 0f), new Vector2(84f, 96f),
            EPPStyleKit.Triangle(), Navy, false, false);

        // Leaves tucked into the button corners and spark dashes on the sides.
        Leaf(rect, new Vector2(-292f, -44f), 58f, 200f, 0.85f);
        Leaf(rect, new Vector2(292f, -44f), 58f, -20f, 0.85f);
        Spark(panel, position + new Vector2(-395f, 26f), 30f);
        Spark(panel, position + new Vector2(-395f, -26f), -30f);
        Spark(panel, position + new Vector2(395f, 26f), -30f);
        Spark(panel, position + new Vector2(395f, -26f), 30f);
        Button playButton = start.GetComponent<Button>();
        if (playButton != null) MakeControlInteractive(playButton);
    }

    private void BuildSecondaryRow(Transform panel, Transform gameSelection)
    {
        const float y = -352f;

        Button modules = SecondaryButton(panel, "ModulosButton", new Vector2(-245f, y), new Vector2(316f, 104f),
            iconBook, "M\u00d3DULOS");
        if (gameSelection != null)
            modules.onClick.AddListener(() => gameSelection.gameObject.SetActive(true));

        SecondaryButton(panel, "LogrosButton", new Vector2(98f, y), new Vector2(306f, 104f), iconTrophy, "LOGROS");
        Button settings = SecondaryButton(panel, "AjustesButton", new Vector2(342f, y), new Vector2(122f, 104f), iconGear, null);
        settings.onClick.AddListener(ToggleSettingsPanel);
    }

    // --------------------------------------------------------------- settings

    private void ApplySavedAudioSettings()
    {
        savedVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(MasterVolumeKey, 1f));
        isMuted = PlayerPrefs.GetInt(MutedKey, 0) == 1;
        ApplyMasterAudio();
    }

    // AudioListener.volume is Unity's master output level. It affects AudioSources
    // routed through AudioMixerGroups too, including AudioManager's music and SFX.
    private void ApplyMasterAudio()
    {
        AudioListener.volume = isMuted ? 0f : Mathf.Clamp01(savedVolume);
    }

    private void EnsureUIInput()
    {
        GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
        if (raycaster == null) raycaster = canvas.gameObject.AddComponent<GraphicRaycaster>();
        raycaster.enabled = true;

        EventSystem system = EventSystem.current;
        if (system == null) system = new GameObject("EventSystem", typeof(EventSystem)).GetComponent<EventSystem>();

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        // A legacy module cannot read input when the project enables only the new system.
        foreach (StandaloneInputModule legacy in system.GetComponents<StandaloneInputModule>())
            legacy.enabled = false;
#endif
        // Keep an existing, enabled UI input module and its authored bindings.
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

    private void ToggleSettingsPanel()
    {
        if (settingsOverlay == null) BuildSettingsPanel();
        // Show the newly built panel on the very first click too.
        SetSettingsVisible(!settingsOverlay.gameObject.activeSelf);
    }

    private void SetSettingsVisible(bool visible)
    {
        if (settingsOverlay == null) return;

        if (visible)
        {
            EnsureUIInput();
            previousSelection = EventSystem.current.currentSelectedGameObject;
            ApplySavedAudioSettings();
            RefreshAudioControls();
            settingsOverlay.SetAsLastSibling();
            settingsOverlay.gameObject.SetActive(true);
            Canvas.ForceUpdateCanvases();
            FitSettingsPanel();
            masterVolumeSlider.Select();
        }
        else
        {
            SaveAudioSettings();
            settingsOverlay.gameObject.SetActive(false);
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(
                    previousSelection != null && previousSelection.activeInHierarchy ? previousSelection : null);
        }
    }

    private void BuildSettingsPanel()
    {
        // Parent to the Canvas itself: InitialPanel can be offset or scaled.
        settingsOverlay = EPPStyleKit.NewRect(canvas, "SettingsOverlay", Vector2.zero, Vector2.zero);
        settingsOverlay.gameObject.SetActive(false);
        Stretch(settingsOverlay);
        settingsOverlay.localScale = Vector3.one;
        settingsOverlay.localRotation = Quaternion.identity;

        Canvas menuCanvas = canvas.GetComponent<Canvas>();
        Canvas modalCanvas = settingsOverlay.gameObject.AddComponent<Canvas>();
        modalCanvas.overrideSorting = true;
        modalCanvas.sortingLayerID = menuCanvas.sortingLayerID;
        modalCanvas.sortingOrder = menuCanvas.sortingOrder + 1;
        modalCanvas.additionalShaderChannels = menuCanvas.additionalShaderChannels;
        settingsOverlay.gameObject.AddComponent<GraphicRaycaster>();
        CanvasGroup modalGroup = settingsOverlay.gameObject.AddComponent<CanvasGroup>();
        modalGroup.interactable = true;
        modalGroup.blocksRaycasts = true;
        modalGroup.ignoreParentGroups = true;

        Image dim = EPPStyleKit.NewImage(settingsOverlay, "Dim", Vector2.zero, Vector2.zero, null,
            new Color32(2, 10, 25, 220), false, false);
        Stretch(dim.rectTransform);
        Button dimButton = dim.gameObject.AddComponent<Button>();
        dimButton.targetGraphic = dim;
        dimButton.transition = Selectable.Transition.None;
        Navigation noNavigation = dimButton.navigation;
        noNavigation.mode = Navigation.Mode.None;
        dimButton.navigation = noNavigation;
        dimButton.onClick.AddListener(() => SetSettingsVisible(false));
        MakeControlInteractive(dimButton);

        Image frame = EPPStyleKit.NewImage(settingsOverlay, "SettingsCard", Vector2.zero, SettingsCardSize,
            EPPStyleKit.Rounded(34), BlueEdge, true, false);
        settingsCard = Place(frame.transform, Vector2.zero, SettingsCardSize);
        // The frame catches clicks in empty card space so they cannot reach Dim.
        frame.raycastTarget = true;
        Image surface = EPPStyleKit.NewImage(settingsCard, "Surface", Vector2.zero,
            SettingsCardSize - new Vector2(6f, 6f), EPPStyleKit.Rounded(32), new Color32(11, 30, 58, 255), true, false);
        surface.raycastTarget = false;

        Label(settingsCard, "SettingsTitle", "AJUSTES", new Vector2(0f, 220f), new Vector2(740f, 72f),
            54f, Color.white, FontStyles.Bold);
        Label(settingsCard, "SettingsSubtitle", "Controla el sonido del juego", new Vector2(0f, 162f),
            new Vector2(740f, 40f), 28f, SubText, FontStyles.Normal);
        Image rule = EPPStyleKit.NewImage(settingsCard, "TitleRule", new Vector2(0f, 126f), new Vector2(772f, 4f),
            EPPStyleKit.Rounded(2), GreenLine, true, false);
        rule.raycastTarget = false;

        Label(settingsCard, "VolumeLabel", "VOLUMEN GENERAL", new Vector2(-114f, 76f),
            new Vector2(544f, 48f), 32f, SubText, FontStyles.Bold).alignment = TextAlignmentOptions.Left;
        volumeValueLabel = Label(settingsCard, "VolumeValue", "100%", new Vector2(326f, 76f),
            new Vector2(120f, 48f), 32f, LimeEdge, FontStyles.Bold);
        volumeValueLabel.alignment = TextAlignmentOptions.Right;

        BuildVolumeSlider(settingsCard);
        Label(settingsCard, "MuteLabel", "SONIDO", new Vector2(-226f, -90f), new Vector2(320f, 64f),
            32f, SubText, FontStyles.Bold).alignment = TextAlignmentOptions.Left;
        BuildMuteButton(settingsCard);

        closeSettingsButton = BuildPanelButton(settingsCard, "CloseSettings", "VOLVER", new Vector2(0f, -220f),
            new Vector2(300f, 76f), Lime, Navy);
        closeSettingsButton.onClick.AddListener(() => SetSettingsVisible(false));

        // Keep keyboard/controller focus inside the modal as well.
        SetVerticalNavigation(masterVolumeSlider, closeSettingsButton, muteButton);
        SetVerticalNavigation(muteButton, masterVolumeSlider, closeSettingsButton);
        SetVerticalNavigation(closeSettingsButton, muteButton, masterVolumeSlider);
        RefreshAudioControls();
    }

    private void BuildVolumeSlider(Transform parent)
    {
        RectTransform sliderRect = EPPStyleKit.NewRect(parent, "MasterVolumeSlider", new Vector2(0f, 8f),
            new Vector2(772f, 76f));
        // A full-width hit area makes both track clicks and dragging reliable.
        Image hitArea = sliderRect.gameObject.AddComponent<Image>();
        hitArea.color = Color.clear;

        EPPStyleKit.NewImage(sliderRect, "Track", Vector2.zero, new Vector2(772f, 24f),
            EPPStyleKit.Rounded(12), new Color32(5, 17, 38, 255), true, false);

        RectTransform fillArea = EPPStyleKit.NewRect(sliderRect, "Fill Area", Vector2.zero, Vector2.zero);
        fillArea.anchorMin = new Vector2(0f, 0.5f);
        fillArea.anchorMax = new Vector2(1f, 0.5f);
        fillArea.offsetMin = new Vector2(24f, -8f);
        fillArea.offsetMax = new Vector2(-24f, 8f);
        Image fill = EPPStyleKit.NewImage(fillArea, "Fill", Vector2.zero, Vector2.zero,
            EPPStyleKit.Rounded(8), Lime, true, false);
        fill.rectTransform.anchorMin = Vector2.zero;
        fill.rectTransform.anchorMax = Vector2.one;
        fill.rectTransform.offsetMin = fill.rectTransform.offsetMax = Vector2.zero;

        RectTransform handleArea = EPPStyleKit.NewRect(sliderRect, "Handle Slide Area", Vector2.zero, Vector2.zero);
        handleArea.anchorMin = new Vector2(0f, 0.5f);
        handleArea.anchorMax = new Vector2(1f, 0.5f);
        handleArea.offsetMin = new Vector2(24f, -22f);
        handleArea.offsetMax = new Vector2(-24f, 22f);
        Image handle = EPPStyleKit.NewImage(handleArea, "Handle", Vector2.zero, new Vector2(44f, 0f),
            EPPStyleKit.Rounded(22), LimeEdge, true, false);
        // Slider drives the anchors. Zero height delta + 44-high parent = a round handle.
        handle.rectTransform.sizeDelta = new Vector2(44f, 0f);
        handle.rectTransform.anchoredPosition = Vector2.zero;

        Slider slider = sliderRect.gameObject.AddComponent<Slider>();
        masterVolumeSlider = slider;
        slider.fillRect = fill.rectTransform;
        slider.handleRect = handle.rectTransform;
        slider.targetGraphic = handle;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
        slider.SetValueWithoutNotify(savedVolume);
        MakeControlInteractive(slider);
        slider.onValueChanged.AddListener(SetMasterVolume);
    }

    private void BuildMuteButton(Transform parent)
    {
        muteButton = BuildPanelButton(parent, "MuteButton", "ACTIVADO",
            new Vector2(254f, -90f), new Vector2(264f, 64f), Lime, Navy);
        muteButtonLabel = muteButton.GetComponentInChildren<TMP_Text>();
        muteButton.onClick.AddListener(() =>
        {
            isMuted = !isMuted;
            ApplyMasterAudio();
            PlayerPrefs.SetInt(MutedKey, isMuted ? 1 : 0);
            audioSettingsDirty = true;
            RefreshAudioControls();
        });
    }

    private void SetMasterVolume(float value)
    {
        savedVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(MasterVolumeKey, savedVolume);
        audioSettingsDirty = true;
        ApplyMasterAudio();
        RefreshAudioControls();
    }

    private void RefreshAudioControls()
    {
        if (masterVolumeSlider != null) masterVolumeSlider.SetValueWithoutNotify(savedVolume);
        if (volumeValueLabel != null) volumeValueLabel.text = Mathf.RoundToInt(savedVolume * 100f) + "%";
        if (muteButton != null) muteButton.GetComponent<Image>().color = isMuted ? BlueEdge : Lime;
        if (muteButtonLabel != null)
        {
            muteButtonLabel.text = isMuted ? "SILENCIADO" : "ACTIVADO";
            muteButtonLabel.color = isMuted ? Color.white : Navy;
        }
    }

    private void SaveAudioSettings()
    {
        if (!audioSettingsDirty) return;
        PlayerPrefs.Save();
        audioSettingsDirty = false;
    }

    private void OnDisable()
    {
        if (settingsOverlay != null) settingsOverlay.gameObject.SetActive(false);
        SaveAudioSettings();
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused) SaveAudioSettings();
    }

    private void OnApplicationQuit() => SaveAudioSettings();

    private void LateUpdate()
    {
        if (settingsOverlay != null && settingsOverlay.gameObject.activeInHierarchy) FitSettingsPanel();
    }

    private void FitSettingsPanel()
    {
        // Uniform scaling keeps the typography, margins and handle proportional.
        Vector2 available = settingsOverlay.rect.size - new Vector2(96f, 96f);
        if (available.x <= 0f || available.y <= 0f) return;
        float scale = Mathf.Min(1f, available.x / SettingsCardSize.x, available.y / SettingsCardSize.y);
        settingsCard.localScale = Vector3.one * scale;
        settingsCard.anchoredPosition = Vector2.zero;
    }

    private Button BuildPanelButton(Transform parent, string name, string caption, Vector2 position, Vector2 size,
        Color fillColor, Color textColor)
    {
        Image image = EPPStyleKit.NewImage(parent, name, position, size, EPPStyleKit.Rounded(22), fillColor, true,
            false);
        EPPStyleKit.NewImage(image.transform, "Rim", Vector2.zero, size + new Vector2(5f, 5f),
            EPPStyleKit.Rounded(24, 3), LimeEdge, true, false);
        TMP_Text captionText = Label(image.transform, "Text", caption, Vector2.zero,
            size - new Vector2(28f, 8f), 30f, textColor, FontStyles.Bold);
        captionText.transform.SetAsLastSibling();
        Button button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.86f, 0.96f, 1f, 1f);
        colors.pressedColor = new Color(0.72f, 0.88f, 0.98f, 1f);
        colors.fadeDuration = 0.08f;
        button.colors = colors;
        MakeControlInteractive(button);
        return button;
    }

    private Button SecondaryButton(Transform parent, string name, Vector2 position, Vector2 size, Sprite icon,
        string label)
    {
        Image fill = EPPStyleKit.Card(parent, name, position, size, 24, NavyPanel, BlueEdge, true, true);
        Transform rim = parent.Find(name + "_Border");
        if (rim != null)
        {
            ((RectTransform)rim).sizeDelta = size + new Vector2(6f, 6f);
            foreach (Graphic graphic in rim.GetComponentsInChildren<Graphic>(true))
                graphic.raycastTarget = false;
        }

        Button button = fill.gameObject.AddComponent<Button>();
        button.targetGraphic = fill;
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.85f, 0.95f, 1f, 1f);
        colors.pressedColor = new Color(0.75f, 0.85f, 0.95f, 1f);
        colors.fadeDuration = 0.08f;
        button.colors = colors;

        if (label == null)
        {
            EPPStyleKit.NewImage(fill.transform, "Icon", Vector2.zero, new Vector2(62f, 62f), icon, Color.white,
                false, false).preserveAspect = true;
        }
        else
        {
            float iconX = -size.x * 0.5f + 62f;
            EPPStyleKit.NewImage(fill.transform, "Icon", new Vector2(iconX, 0f), new Vector2(62f, 62f), icon,
                Color.white, false, false).preserveAspect = true;
            Label(fill.transform, "Label", label, new Vector2(34f, 0f), new Vector2(size.x - 120f, 60f), 34f,
                Color.white, FontStyles.Bold);
        }

        MakeControlInteractive(button);
        return button;
    }

    // ---------------------------------------------------------- game selection

    private void StyleGameSelection(Transform selection)
    {
        Image panelImage = selection.GetComponent<Image>();
        if (panelImage != null) panelImage.color = new Color(0f, 0f, 0f, 0f);

        RectTransform selectionRect = (RectTransform)selection;
        AddBackdrop(selection, -selectionRect.anchoredPosition, new Vector2(1920f, 1080f));

        Transform content = selection.Find("ContentPage");
        if (content != null)
        {
            Image contentImage = content.GetComponent<Image>();
            if (contentImage != null) contentImage.color = new Color(0f, 0f, 0f, 0f);
            content.SetAsLastSibling();
        }

        TextMeshProUGUI heading = Label(selection, "Heading", "SELECCIONA UN JUEGO", new Vector2(0f, 400f),
            new Vector2(1200f, 110f), 76f, Color.white, FontStyles.Bold);
        heading.enableVertexGradient = true;
        heading.colorGradient = new VertexGradient(Color.white, Color.white, TitleBottom, TitleBottom);
        // The panel starts inactive, so the text has no material instance yet: assign an outlined copy.
        if (font != null && font.material != null)
        {
            Material outlined = new Material(font.material) { name = "HeadingOutline" };
            outlined.EnableKeyword("OUTLINE_ON");
            outlined.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.14f);
            outlined.SetColor(ShaderUtilities.ID_OutlineColor, TitleOutline);
            heading.fontSharedMaterial = outlined;
        }

        if (content != null)
        {
            StyleGameCard(content.Find("Conductores Game"), "Conductores");
            StyleGameCard(content.Find("Barrido Game"), "Recolecci\u00f3n de residuos");
            StyleGameCard(content.Find("Alturas Game"), "Alturas");
        }

        Button back = SecondaryButton(selection, "VolverButton", new Vector2(-720f, -430f), new Vector2(240f, 84f),
            null, null);
        Transform backIcon = back.transform.Find("Icon");
        if (backIcon != null) Destroy(backIcon.gameObject);
        Label(back.transform, "Label", "VOLVER", Vector2.zero, new Vector2(200f, 60f), 32f, Color.white,
            FontStyles.Bold);
        back.onClick.AddListener(() => selection.gameObject.SetActive(false));
    }

    private void StyleGameCard(Transform card, string title)
    {
        if (card == null) return;

        RectTransform rect = (RectTransform)card;
        Vector2 size = new Vector2(425f, 525f);
        rect.sizeDelta = size;

        // The authored button image becomes the card frame (keeps the LoadScene listener).
        Image frame = card.GetComponent<Image>();
        frame.sprite = EPPStyleKit.Rounded(30);
        frame.type = Image.Type.Sliced;
        frame.color = NavyPanel;
        frame.raycastTarget = true;

        Button button = card.GetComponent<Button>();
        if (button != null)
        {
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.85f, 0.95f, 1f, 1f);
            colors.pressedColor = new Color(0.75f, 0.85f, 0.95f, 1f);
            button.colors = colors;
        }

        // Hide the authored hover labels; the card gets its own title.
        foreach (TMP_Text text in card.GetComponentsInChildren<TMP_Text>(true))
            text.gameObject.SetActive(false);

        // Preview art clipped to a rounded window.
        Image mask = EPPStyleKit.NewImage(card, "PreviewMask", new Vector2(0f, 62f), new Vector2(389f, 365f),
            EPPStyleKit.Rounded(22), Color.white, true, false);
        mask.gameObject.AddComponent<Mask>().showMaskGraphic = false;

        Transform art = card.Find("Panel");
        if (art != null)
        {
            RectTransform artRect = (RectTransform)art;
            artRect.SetParent(mask.transform, false);
            artRect.anchorMin = artRect.anchorMax = new Vector2(0.5f, 0.5f);
            artRect.pivot = new Vector2(0.5f, 0.5f);
            artRect.anchoredPosition = Vector2.zero;
            artRect.localScale = Vector3.one;

            Image artImage = art.GetComponent<Image>();
            artImage.raycastTarget = false;
            artImage.preserveAspect = false;
            float aspect = artImage.sprite != null ? artImage.sprite.rect.width / artImage.sprite.rect.height : 1f;
            AspectRatioFitter fitter = art.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fitter.aspectRatio = aspect;
        }

        TextMeshProUGUI cardTitle = Label(card, "Title", title, new Vector2(0f, -160f), new Vector2(395f, 56f), 38f, Color.white,
            FontStyles.Bold);
        cardTitle.enableAutoSizing = true;
        cardTitle.fontSizeMin = 24f;
        cardTitle.fontSizeMax = 38f;

        Image pill = EPPStyleKit.NewImage(card, "Play", new Vector2(0f, -222f), new Vector2(200f, 56f),
            EPPStyleKit.Rounded(26), Lime, true, false);
        EPPStyleKit.NewImage(pill.transform, "Rim", Vector2.zero, new Vector2(204f, 60f),
            EPPStyleKit.Rounded(28, 3), LimeEdge, true, false);
        EPPStyleKit.NewImage(pill.transform, "Icon", new Vector2(-50f, 0f), new Vector2(28f, 32f),
            EPPStyleKit.Triangle(), Navy, false, false);
        Label(pill.transform, "Label", "JUGAR", new Vector2(16f, 0f), new Vector2(120f, 50f), 30f, Navy,
            FontStyles.Bold);

        EPPStyleKit.NewImage(card, "Rim", Vector2.zero, size + new Vector2(8f, 8f), EPPStyleKit.Rounded(32, 4),
            BlueEdge, true, false);
        Leaf((RectTransform)card, new Vector2(-196f, -246f), 64f, 200f, 0.9f);
        Leaf((RectTransform)card, new Vector2(196f, -246f), 64f, -20f, 0.9f);
        if (button != null) MakeControlInteractive(button);
    }

    // ------------------------------------------------------------------ helpers

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    private static void MakeControlInteractive(Selectable control)
    {
        // EPPStyleKit also creates decorative graphics. Only the control's own
        // graphic should be hit; a sibling border or child label must not intercept it.
        foreach (Graphic graphic in control.GetComponentsInChildren<Graphic>(true))
            graphic.raycastTarget = false;
        Graphic target = control.GetComponent<Graphic>();
        if (target != null) target.raycastTarget = true;
        control.interactable = true;
    }

    private static void SetVerticalNavigation(Selectable control, Selectable up, Selectable down)
    {
        Navigation navigation = new Navigation { mode = Navigation.Mode.Explicit };
        navigation.selectOnUp = up;
        navigation.selectOnDown = down;
        control.navigation = navigation;
    }

    private void AddBackdrop(Transform parent, Vector2 position, Vector2? size = null)
    {
        if (background == null) return;

        RectTransform frame = EPPStyleKit.NewRect(parent, "Backdrop", position, Vector2.zero);
        if (size.HasValue)
        {
            frame.sizeDelta = size.Value;
        }
        else
        {
            frame.anchorMin = Vector2.zero;
            frame.anchorMax = Vector2.one;
            frame.offsetMin = frame.offsetMax = Vector2.zero;
        }

        frame.gameObject.AddComponent<RectMask2D>();
        frame.SetAsFirstSibling();

        Image image = EPPStyleKit.NewImage(frame, "Art", Vector2.zero, Vector2.zero, background, Color.white, false,
            true);
        image.raycastTarget = false;
        AspectRatioFitter fitter = image.gameObject.AddComponent<AspectRatioFitter>();
        fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        fitter.aspectRatio = background.rect.width / background.rect.height;
    }

    private static RectTransform Place(Transform target, Vector2 position, Vector2 size)
    {
        if (target == null) return null;
        RectTransform rect = (RectTransform)target;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        Image image = target.GetComponent<Image>();
        if (image != null && image.sprite != null && image.type == Image.Type.Simple) image.preserveAspect = true;
        return rect;
    }

    private void Leaf(RectTransform parent, Vector2 position, float size, float rotation, float alpha)
    {
        if (leaf == null) return;
        Image image = EPPStyleKit.NewImage(parent, "Leaf", position, new Vector2(size, size), leaf,
            new Color(1f, 1f, 1f, alpha), false, false);
        image.preserveAspect = true;
        image.rectTransform.localRotation = Quaternion.Euler(0f, 0f, rotation);
    }

    private static void Spark(Transform parent, Vector2 position, float rotation)
    {
        Image dash = EPPStyleKit.NewImage(parent, "Spark", position, new Vector2(26f, 7f), EPPStyleKit.Rounded(3),
            LimeEdge, true, false);
        dash.rectTransform.localRotation = Quaternion.Euler(0f, 0f, rotation);
    }

    private TextMeshProUGUI Label(Transform parent, string name, string value, Vector2 position, Vector2 size,
        float fontSize, Color color, FontStyles style)
    {
        TextMeshProUGUI text = EPPStyleKit.Label(parent, name, value, position, size, fontSize, color, false,
            TextAlignmentOptions.Center);
        if (font != null) text.font = font;
        text.fontStyle = style;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.raycastTarget = false;
        return text;
    }
}
