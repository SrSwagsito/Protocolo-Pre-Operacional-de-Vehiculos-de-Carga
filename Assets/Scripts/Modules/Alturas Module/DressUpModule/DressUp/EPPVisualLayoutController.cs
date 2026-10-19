using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Builds a 2D EPP-selection layout at runtime and gives each draggable item a
/// real inventory parent, so returning a rejected drag restores it inside the panel.
/// </summary>
public sealed class EPPVisualLayoutController : MonoBehaviour
{
    private const string RootName = "__EPPVisualLayout";
    private static readonly Color Navy = new Color32(18, 54, 103, 255);
    private static readonly Color Blue = new Color32(36, 117, 196, 255);
    private static readonly Color Green = new Color32(20, 153, 82, 255);
    private static readonly Color PaleGreen = new Color32(224, 246, 225, 255);
    private static readonly Color PanelWhite = new Color32(250, 252, 255, 250);
    private static readonly Color CardGrey = new Color32(235, 241, 247, 255);
    private static readonly Color TextBlue = new Color32(19, 57, 105, 255);

    private Transform gameUI, layoutRoot, backplates, controls, catalogueContent;
    private readonly List<DraggableCloth> clothes = new List<DraggableCloth>();
    private readonly List<ClothSlot> slots = new List<ClothSlot>();
    private readonly Dictionary<string, Button> categoryButtons = new Dictionary<string, Button>();
    private readonly List<Text> overviewTexts = new List<Text>();
    private string selectedCategory = "Cabeza";
    private float nextRefresh;
    private bool initialized;

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
        if (!scene.IsValid() || scene.name != "Module1EPPS") return;
        GameObject ui = GameObject.Find("UI");
        Transform game = ui != null ? ui.transform.Find("Game UI") : null;
        if (game == null || game.Find(RootName) != null) return;
        game.gameObject.AddComponent<EPPVisualLayoutController>().Initialize(game);
    }

    private void Initialize(Transform root)
    {
        if (initialized) return;
        initialized = true;
        gameUI = root;

        foreach (DraggableCloth cloth in root.GetComponentsInChildren<DraggableCloth>(true))
            if (cloth != null && !clothes.Contains(cloth)) clothes.Add(cloth);
        foreach (ClothSlot slot in root.GetComponentsInChildren<ClothSlot>(true))
            if (slot != null && !slots.Contains(slot)) slots.Add(slot);

        // The original scene has some decorative images named Image (1), Image (14), etc.
        // Do not deactivate any objects here: some may be used by other scripts.

        BuildRoots();
        BuildPanels();
        PlaceCharacter();
        PlaceInventoryItems();
        BuildCategories();
        BuildOverview();
        BuildFooter();
        SelectCategory(selectedCategory);
    }

    private void BuildRoots()
    {
        GameObject root = new GameObject(RootName, typeof(RectTransform));
        root.transform.SetParent(gameUI, false);
        layoutRoot = root.transform;
        Stretch(root.GetComponent<RectTransform>());

        GameObject bg = new GameObject("Backplates", typeof(RectTransform));
        bg.transform.SetParent(layoutRoot, false);
        Stretch(bg.GetComponent<RectTransform>());
        backplates = bg.transform;

        GameObject fg = new GameObject("Controls", typeof(RectTransform));
        fg.transform.SetParent(layoutRoot, false);
        Stretch(fg.GetComponent<RectTransform>());
        controls = fg.transform;
        layoutRoot.SetAsLastSibling();
    }

    private void BuildPanels()
    {
        // 1920 x 1080 reference canvas; all panel centers stay inside safe horizontal bounds.
        Panel("CategoryPanel", new Vector2(-800, 0), new Vector2(220, 650), PanelWhite);
        Panel("CataloguePanel", new Vector2(-520, 0), new Vector2(330, 710), PanelWhite);
        Panel("OverviewPanel", new Vector2(745, 0), new Vector2(350, 710), PanelWhite);

        Label(controls, "CategoryTitle", "EQUIPO", new Vector2(-800, 280), new Vector2(180, 42), 23, TextBlue, FontStyle.Bold, TextAnchor.MiddleLeft);
        Label(controls, "CatalogueTitle", "Prendas", new Vector2(-520, 310), new Vector2(270, 42), 26, TextBlue, FontStyle.Bold, TextAnchor.MiddleLeft);
        Label(controls, "CatalogueHint", "Arrastra una prenda al personaje", new Vector2(-520, 273), new Vector2(280, 28), 13, TextBlue, FontStyle.Normal, TextAnchor.MiddleLeft);

        // Actual inventory content parent. Each draggable object gets its own bounded cell.
        GameObject content = new GameObject("CatalogueContent", typeof(RectTransform));
        content.transform.SetParent(layoutRoot, false);
        catalogueContent = content.transform;
        RectTransform cr = content.GetComponent<RectTransform>();
        Center(cr, new Vector2(-520, -20));
        cr.sizeDelta = new Vector2(300, 500);
    }

    private void PlaceCharacter()
    {
        Transform mannequin = gameUI.Find("Maniqui");
        if (mannequin != null)
        {
            Center(mannequin, new Vector2(0, -18));
            mannequin.localScale = Vector3.one * 1.18f;
        }

        string[] layers = { "HelmetLayer", "GlassesLayer", "HarnessLayer", "ShirtLayer",
            "GlovesLayer", "PantsLayer", "BootsLayer", "JeansLayer", "CasualShirtLayer",
            "BalacaLayer (1)", "EarringsLayer (1)", "WatchLayer", "Chanclas Layer" };
        foreach (string n in layers)
        {
            Transform t = gameUI.Find(n);
            RectTransform r = t as RectTransform;
            if (r == null) continue;
            r.anchoredPosition *= 1.18f;
            r.sizeDelta *= 1.18f;
        }

        // Make the existing slots invisible but keep their raycast targets active.
        foreach (ClothSlot slot in slots)
        {
            if (slot == null) continue;
            RectTransform r = slot.transform as RectTransform;
            if (r == null) continue;
            r.sizeDelta = new Vector2(100, 100);
            Image img = slot.GetComponent<Image>();
            if (img != null)
            {
                Color c = img.color; c.a = 0f; img.color = c;
                img.raycastTarget = true;
            }
        }
    }

    private void PlaceInventoryItems()
    {
        // Position each item in a fixed cell inside the visible catalogue.
        // Do not use the old 1000+ px scene positions: those caused clothing to appear off-screen.
        for (int i = 0; i < clothes.Count; i++)
        {
            DraggableCloth cloth = clothes[i];
            if (cloth == null) continue;

            bool alreadyEquipped = false;
            foreach (ClothSlot slot in slots)
                if (slot != null && slot.GetCurrentCloth() == cloth) { alreadyEquipped = true; break; }
            if (alreadyEquipped) continue;

            RectTransform rect = cloth.transform as RectTransform;
            if (rect == null) continue;

            cloth.transform.SetParent(catalogueContent, false);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.localScale = Vector3.one;
            rect.sizeDelta = FitSize(rect.sizeDelta, 105, 105);

            // DraggableObject uses these values when a drop is rejected.
            DraggableObject drag = cloth.GetComponent<DraggableObject>();
            if (drag != null) drag.SetInventoryHome(catalogueContent, Vector2.zero);

            cloth.gameObject.SetActive(false); // category selection activates the relevant items
        }
    }

    private void BuildCategories()
    {
        AddCategory("Cabeza", new Vector2(-800, 210));
        AddCategory("Torso", new Vector2(-800, 130));
        AddCategory("Pantalón", new Vector2(-800, 50));
        AddCategory("Guantes", new Vector2(-800, -30));
        AddCategory("Calzado", new Vector2(-800, -110));
        AddCategory("Accesorios", new Vector2(-800, -190));
    }

    private void AddCategory(string label, Vector2 pos)
    {
        categoryButtons[label] = MakeButton("Category_" + label, label, pos,
            new Vector2(188, 62), CardGrey, TextBlue, () => SelectCategory(label));
    }

    private void BuildOverview()
    {
        Label(controls, "OverviewTitle", "Vista general", new Vector2(745, 310), new Vector2(300, 44), 26, TextBlue, FontStyle.Bold, TextAnchor.MiddleLeft);
        string[] labels = { "Casco", "Camisa", "Pantalón", "Guantes", "Calzado", "Arnés", "Accesorios" };
        for (int i = 0; i < labels.Length; i++)
        {
            float y = 225 - i * 72;
            Panel("OverviewRow_" + i, new Vector2(745, y), new Vector2(305, 62), CardGrey);
            overviewTexts.Add(Label(controls, "OverviewText_" + i, labels[i] + "\nSin seleccionar",
                new Vector2(755, y), new Vector2(270, 55), 14, TextBlue, FontStyle.Normal, TextAnchor.MiddleLeft));
        }
    }

    private void BuildFooter()
    {
        MakeButton("BackButton", "←  Volver", new Vector2(-800, -465), new Vector2(220, 62),
            new Color32(245, 249, 253, 255), Blue, Back);
        MakeButton("FinishButton", "Finalizar  →", new Vector2(745, -465), new Vector2(270, 66),
            Green, Color.white, Finish);
    }

    private void SelectCategory(string category)
    {
        selectedCategory = category;
        foreach (var pair in categoryButtons)
        {
            Image image = pair.Value != null ? pair.Value.GetComponent<Image>() : null;
            if (image != null) image.color = pair.Key == category ? PaleGreen : CardGrey;
        }

        int index = 0;
        foreach (DraggableCloth cloth in clothes)
        {
            if (cloth == null) continue;
            bool isEquipped = false;
            foreach (ClothSlot slot in slots)
                if (slot != null && slot.GetCurrentCloth() == cloth) { isEquipped = true; break; }

            if (isEquipped)
            {
                cloth.gameObject.SetActive(true);
                continue;
            }

            bool show = Matches(category, cloth.Cloth);
            cloth.gameObject.SetActive(show);
            if (!show) continue;

            RectTransform rect = cloth.transform as RectTransform;
            if (rect == null) continue;
            rect.SetParent(catalogueContent, false);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2((index % 2 == 0 ? -72 : 72), 160 - (index / 2) * 130);
            rect.sizeDelta = FitSize(rect.sizeDelta, 105, 105);
            rect.localScale = Vector3.one;

            DraggableObject drag = cloth.GetComponent<DraggableObject>();
            if (drag != null) drag.SetInventoryHome(catalogueContent, rect.anchoredPosition);
            index++;
        }
    }

    private static bool Matches(string category, DraggableCloth.ClothType type)
    {
        switch (category)
        {
            case "Cabeza": return type == DraggableCloth.ClothType.Helmet;
            case "Torso": return type == DraggableCloth.ClothType.Shirt || type == DraggableCloth.ClothType.Harness;
            case "Pantalón": return type == DraggableCloth.ClothType.Pants;
            case "Guantes": return type == DraggableCloth.ClothType.Gloves;
            case "Calzado": return type == DraggableCloth.ClothType.Boots;
            case "Accesorios": return type == DraggableCloth.ClothType.Glasses;
            default: return false;
        }
    }

    private void RefreshOverview()
    {
        DraggableCloth.ClothType[] types = { DraggableCloth.ClothType.Helmet, DraggableCloth.ClothType.Shirt,
            DraggableCloth.ClothType.Pants, DraggableCloth.ClothType.Gloves, DraggableCloth.ClothType.Boots,
            DraggableCloth.ClothType.Harness, DraggableCloth.ClothType.Glasses };
        string[] labels = { "Casco", "Camisa", "Pantalón", "Guantes", "Calzado", "Arnés", "Accesorios" };

        for (int i = 0; i < overviewTexts.Count; i++)
        {
            DraggableCloth selected = null;
            foreach (ClothSlot slot in slots)
            {
                DraggableCloth current = slot != null ? slot.GetCurrentCloth() : null;
                if (current != null && current.Cloth == types[i]) { selected = current; break; }
            }
            if (overviewTexts[i] != null)
                overviewTexts[i].text = selected != null
                    ? labels[i] + "\n" + selected.name.Replace("(1)", "").Trim()
                    : labels[i] + "\nSin seleccionar";
        }
    }

    private void Update()
    {
        if (Time.unscaledTime < nextRefresh) return;
        RefreshOverview();
        ModuleDressUpEPPSManager manager = FindObjectOfType<ModuleDressUpEPPSManager>();
        if (manager != null) manager.RefreshProgress();
        nextRefresh = Time.unscaledTime + 0.25f;
    }

    private void Finish()
    {
        ModuleDressUpEPPSManager manager = FindObjectOfType<ModuleDressUpEPPSManager>();
        if (manager != null) manager.FinishModule();
        else Debug.LogError("No se encontró ModuleDressUpEPPSManager.");
    }

    private void Back()
    {
        const string sceneName = "Initial Menu game";
        if (Application.CanStreamedLevelBeLoaded(sceneName)) SceneManager.LoadScene(sceneName);
        else Debug.LogWarning("Configura el nombre real de la escena de menú en EPPVisualLayoutController.");
    }

    private Button MakeButton(string name, string label, Vector2 pos, Vector2 size, Color bg, Color fg, UnityEngine.Events.UnityAction click)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(controls, false);
        RectTransform r = go.GetComponent<RectTransform>(); Center(r, pos); r.sizeDelta = size;
        Image img = go.GetComponent<Image>(); img.sprite = Rounded(); img.type = img.sprite ? Image.Type.Sliced : Image.Type.Simple; img.color = bg;
        Button b = go.GetComponent<Button>(); b.targetGraphic = img; b.onClick.AddListener(click);
        Text t = Label(go.transform, name + "_Text", label, Vector2.zero, size - new Vector2(16, 8), 18, fg, FontStyle.Bold, TextAnchor.MiddleCenter);
        t.resizeTextForBestFit = true; t.resizeTextMinSize = 13; t.resizeTextMaxSize = 21;
        return b;
    }

    private void Panel(string name, Vector2 pos, Vector2 size, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(backplates, false);
        RectTransform r = go.GetComponent<RectTransform>(); Center(r, pos); r.sizeDelta = size;
        Image img = go.GetComponent<Image>(); img.sprite = Rounded(); img.type = img.sprite ? Image.Type.Sliced : Image.Type.Simple;
        img.color = color; img.raycastTarget = false;
    }

    private Text Label(Transform parent, string name, string value, Vector2 pos, Vector2 size, int fontSize, Color color, FontStyle style, TextAnchor align)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        RectTransform r = go.GetComponent<RectTransform>(); Center(r, pos); r.sizeDelta = size;
        Text t = go.GetComponent<Text>(); t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        t.text = value; t.fontSize = fontSize; t.color = color; t.fontStyle = style; t.alignment = align;
        t.horizontalOverflow = HorizontalWrapMode.Wrap; t.verticalOverflow = VerticalWrapMode.Truncate; t.raycastTarget = false;
        return t;
    }

    private static Sprite Rounded() => Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
    private static Vector2 FitSize(Vector2 current, float maxW, float maxH)
    {
        if (current.x <= 0 || current.y <= 0) return new Vector2(maxW, maxH);
        return current * Mathf.Min(maxW / current.x, maxH / current.y);
    }
    private static void Stretch(RectTransform r) { r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero; r.localScale = Vector3.one; }
    private static void Center(Transform t, Vector2 p) { RectTransform r = t as RectTransform; if (r == null) return; r.anchorMin = r.anchorMax = new Vector2(.5f, .5f); r.pivot = new Vector2(.5f, .5f); r.anchoredPosition = p; }
}
