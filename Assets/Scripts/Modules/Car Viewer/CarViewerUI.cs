using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

/// <summary>
/// Construye en tiempo de ejecución la interfaz del visualizador:
/// lista de partes, botón de vista general y panel con la información de la parte enfocada.
/// </summary>
public class CarViewerUI : MonoBehaviour
{
    public CarViewerController viewer;
    public string title = "Visualizador 3D del vehículo";

    private static readonly Color PanelColor = new Color(0.06f, 0.09f, 0.14f, 0.88f);
    private static readonly Color ButtonColor = new Color(0.14f, 0.19f, 0.27f, 1f);
    private static readonly Color ButtonSelectedColor = new Color(0.05f, 0.6f, 0.85f, 1f);
    private static readonly Color AccentColor = new Color(0.1f, 0.85f, 1f, 1f);

    private readonly List<KeyValuePair<CarPart, Image>> partButtons = new List<KeyValuePair<CarPart, Image>>();
    private TMP_FontAsset font;
    private GameObject infoPanel;
    private TextMeshProUGUI infoName, infoIndex, infoDescription;
    private Button overviewButton;

    private void Start()
    {
        if (viewer == null) viewer = FindAnyObjectByType<CarViewerController>();
        if (viewer == null) { Debug.LogError("[CarViewerUI] No se encontró CarViewerController."); return; }

        font = TMP_Settings.defaultFontAsset;
        EnsureEventSystem();
        Build();
        viewer.SelectionChanged += OnSelectionChanged;
        OnSelectionChanged(viewer.Selected);
    }

    private void OnDestroy()
    {
        if (viewer != null) viewer.SelectionChanged -= OnSelectionChanged;
    }

    private void OnSelectionChanged(CarPart part)
    {
        foreach (var pair in partButtons)
            pair.Value.color = pair.Key == part ? ButtonSelectedColor : ButtonColor;

        infoPanel.SetActive(part != null);
        overviewButton.interactable = part != null;
        if (part == null) return;

        int index = 0;
        for (int i = 0; i < viewer.Parts.Count; i++) if (viewer.Parts[i] == part) index = i;
        infoName.text = part.partName;
        infoIndex.text = $"Parte {index + 1} de {viewer.Parts.Count}";
        infoDescription.text = part.description;
    }

    // ---------- Construcción ----------

    private void Build()
    {
        GameObject canvasGO = new GameObject("CarViewerCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGO.transform.SetParent(transform, false);
        Canvas canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        RectTransform root = (RectTransform)canvasGO.transform;

        // Título e instrucciones
        RectTransform header = Rect("Header", root, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f));
        header.offsetMin = new Vector2(400f, -130f);
        header.offsetMax = new Vector2(-320f, -20f);
        TextMeshProUGUI titleText = Label("Title", header, title, 46f, Color.white, FontStyles.Bold, TextAlignmentOptions.Center);
        Stretch(titleText.rectTransform, 0f, 0.45f, 1f, 1f);
        TextMeshProUGUI hint = Label("Hint", header,
            "Arrastra para girar  •  Rueda o pellizco para acercar  •  Clic en una parte para enfocarla  •  Esc para volver",
            22f, new Color(0.8f, 0.88f, 0.95f, 1f), FontStyles.Normal, TextAlignmentOptions.Center);
        Stretch(hint.rectTransform, 0f, 0f, 1f, 0.45f);

        // Botón vista general
        RectTransform overviewRect = Rect("OverviewButton", root, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f));
        overviewRect.anchoredPosition = new Vector2(-24f, -24f);
        overviewRect.sizeDelta = new Vector2(270f, 64f);
        overviewButton = MakeButton(overviewRect, "Vista general", 26f, TextAlignmentOptions.Center, out _);
        overviewButton.onClick.AddListener(viewer.ShowOverview);

        // Panel de partes (izquierda)
        RectTransform listPanel = Rect("PartsPanel", root, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f));
        listPanel.offsetMin = new Vector2(24f, 24f);
        listPanel.offsetMax = new Vector2(384f, -24f);
        listPanel.gameObject.AddComponent<Image>().color = PanelColor;

        TextMeshProUGUI listTitle = Label("PartsTitle", listPanel, "Partes del vehículo", 28f, AccentColor, FontStyles.Bold, TextAlignmentOptions.Center);
        RectTransform ltr = listTitle.rectTransform;
        ltr.anchorMin = new Vector2(0f, 1f); ltr.anchorMax = new Vector2(1f, 1f); ltr.pivot = new Vector2(0.5f, 1f);
        ltr.offsetMin = new Vector2(12f, -70f); ltr.offsetMax = new Vector2(-12f, -12f);

        RectTransform viewport = Rect("Viewport", listPanel, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        viewport.offsetMin = new Vector2(12f, 12f);
        viewport.offsetMax = new Vector2(-12f, -78f);
        viewport.gameObject.AddComponent<RectMask2D>();
        Image vpImage = viewport.gameObject.AddComponent<Image>();
        vpImage.color = new Color(0f, 0f, 0f, 0.001f);

        RectTransform content = Rect("Content", viewport, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f));
        content.offsetMin = Vector2.zero; content.offsetMax = Vector2.zero;
        VerticalLayoutGroup layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 6f;
        layout.childControlHeight = true; layout.childControlWidth = true;
        layout.childForceExpandHeight = false; layout.childForceExpandWidth = true;
        content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ScrollRect scroll = listPanel.gameObject.AddComponent<ScrollRect>();
        scroll.viewport = viewport; scroll.content = content;
        scroll.horizontal = false; scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 30f;

        foreach (CarPart part in viewer.Parts)
        {
            RectTransform br = Rect(part.partName, content, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            br.gameObject.AddComponent<LayoutElement>().preferredHeight = 48f;
            Button b = MakeButton(br, part.partName, 22f, TextAlignmentOptions.MidlineLeft, out Image bg);
            CarPart captured = part;
            b.onClick.AddListener(() => viewer.Select(captured));
            partButtons.Add(new KeyValuePair<CarPart, Image>(part, bg));
        }

        // Panel de información (abajo derecha)
        RectTransform info = Rect("InfoPanel", root, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f));
        info.anchoredPosition = new Vector2(-24f, 24f);
        info.sizeDelta = new Vector2(660f, 290f);
        info.gameObject.AddComponent<Image>().color = PanelColor;
        infoPanel = info.gameObject;

        RectTransform accentBar = Rect("Accent", info, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f));
        accentBar.sizeDelta = new Vector2(8f, 0f);
        accentBar.gameObject.AddComponent<Image>().color = AccentColor;

        infoIndex = Label("Index", info, "", 20f, AccentColor, FontStyles.Normal, TextAlignmentOptions.TopLeft);
        SetBox(infoIndex.rectTransform, 30f, -18f, -24f, 30f);
        infoName = Label("Name", info, "", 36f, Color.white, FontStyles.Bold, TextAlignmentOptions.TopLeft);
        SetBox(infoName.rectTransform, 30f, -46f, -24f, 50f);
        infoDescription = Label("Description", info, "", 22f, new Color(0.85f, 0.9f, 0.95f, 1f), FontStyles.Normal, TextAlignmentOptions.TopLeft);
        RectTransform dr = infoDescription.rectTransform;
        dr.anchorMin = Vector2.zero; dr.anchorMax = Vector2.one;
        dr.offsetMin = new Vector2(30f, 18f); dr.offsetMax = new Vector2(-24f, -104f);
        infoDescription.textWrappingMode = TextWrappingModes.Normal;
    }

    private void EnsureEventSystem()
    {
        if (FindAnyObjectByType<EventSystem>() != null) return;
        GameObject es = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        es.transform.SetParent(transform, false);
    }

    // ---------- Helpers ----------

    private static RectTransform Rect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot)
    {
        RectTransform rt = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.pivot = pivot;
        return rt;
    }

    private static void Stretch(RectTransform rt, float minX, float minY, float maxX, float maxY)
    {
        rt.anchorMin = new Vector2(minX, minY); rt.anchorMax = new Vector2(maxX, maxY);
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    private static void SetBox(RectTransform rt, float left, float top, float right, float height)
    {
        rt.anchorMin = new Vector2(0f, 1f); rt.anchorMax = new Vector2(1f, 1f); rt.pivot = new Vector2(0.5f, 1f);
        rt.offsetMin = new Vector2(left, top - height); rt.offsetMax = new Vector2(right, top);
    }

    private TextMeshProUGUI Label(string name, Transform parent, string text, float size, Color color, FontStyles style, TextAlignmentOptions align)
    {
        TextMeshProUGUI t = new GameObject(name, typeof(RectTransform)).AddComponent<TextMeshProUGUI>();
        t.transform.SetParent(parent, false);
        if (font != null) t.font = font;
        t.text = text; t.fontSize = size; t.color = color; t.fontStyle = style; t.alignment = align;
        t.textWrappingMode = TextWrappingModes.NoWrap;
        t.raycastTarget = false;
        return t;
    }

    private Button MakeButton(RectTransform rt, string text, float size, TextAlignmentOptions align, out Image bg)
    {
        bg = rt.gameObject.AddComponent<Image>();
        bg.color = ButtonColor;
        Button b = rt.gameObject.AddComponent<Button>();
        ColorBlock cb = b.colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = new Color(1.25f, 1.25f, 1.25f, 1f);
        cb.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        cb.disabledColor = new Color(0.6f, 0.6f, 0.6f, 0.6f);
        b.colors = cb;
        b.targetGraphic = bg;

        TextMeshProUGUI label = Label("Label", rt, text, size, Color.white, FontStyles.Bold, align);
        Stretch(label.rectTransform, 0f, 0f, 1f, 1f);
        label.margin = new Vector4(18f, 0f, 12f, 0f);
        return b;
    }
}
