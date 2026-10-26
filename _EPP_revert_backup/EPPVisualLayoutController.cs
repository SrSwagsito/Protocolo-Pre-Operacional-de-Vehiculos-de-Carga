using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Builds the EPP dress-up screen at runtime: category rail on the left, item
/// catalogue in the middle, live character preview and "Vista general" summary on
/// the right. Items are picked from the catalogue with a click (a drag onto the
/// body still works) and the worn clothing is rendered by CharacterGraphic layers.
/// </summary>
public sealed class EPPVisualLayoutController : MonoBehaviour
{
    private const string RootName = "__EPPVisualLayout";

    // Canvas reference is 1920 x 1080, anchored centre => x in [-960, 960], y in [-540, 540].
    private const float SidebarX = -800f;
    private const float SidebarWidth = 244f;
    private const float SidebarHeight = 84f;
    private const float CatalogueX = -505f;
    private const float CatalogueTop = 215f;
    private const float CatalogueGridY = -12f;
    private const float CardSize = 150f;
    private const float CardGap = 164f;
    private const float OverviewX = 770f;
    private const float OverviewTop = 300f;
    private const float OverviewRowStart = 216f;
    private const float OverviewRowStep = 76f;
    private const float CharacterScale = 1.08f;

    private sealed class Category
    {
        public string Key;
        public string Title;
        public DraggableCloth.ClothType[] Types;
        public Image Fill;
        public Image Border;
        public TMP_Text Chevron;
        public Image Badge;
    }

    private sealed class Card
    {
        public DraggableCloth Cloth;
        public RectTransform Root;
        public Image Fill;
        public Image Border;
        public Image Badge;
    }

    private sealed class OverviewRow
    {
        public DraggableCloth.ClothType Type;
        public Image Icon;
        public TMP_Text Value;
    }

    private static readonly string[] CategoryKeys = { "Cabeza", "Torso", "Pantalón", "Guantes", "Calzado", "Accesorios" };
    private static readonly string[] CategoryTitles = { "Cascos", "Camisetas", "Pantalones", "Guantes", "Calzado", "Accesorios" };

    private static readonly DraggableCloth.ClothType[][] CategoryTypes =
    {
        new[] { DraggableCloth.ClothType.Helmet },
        new[] { DraggableCloth.ClothType.Shirt },
        new[] { DraggableCloth.ClothType.Pants },
        new[] { DraggableCloth.ClothType.Gloves },
        new[] { DraggableCloth.ClothType.Boots },
        new[] { DraggableCloth.ClothType.Glasses, DraggableCloth.ClothType.Harness }
    };

    private static readonly float[] SidebarY = { 250f, 154f, 58f, -38f, -134f, -230f };

    private static readonly DraggableCloth.ClothType[] OverviewTypes =
    {
        DraggableCloth.ClothType.Helmet,
        DraggableCloth.ClothType.Shirt,
        DraggableCloth.ClothType.Pants,
        DraggableCloth.ClothType.Gloves,
        DraggableCloth.ClothType.Boots,
        DraggableCloth.ClothType.Harness,
        DraggableCloth.ClothType.Glasses
    };

    private static readonly string[] OverviewTitles = { "Casco", "Camiseta", "Pantalón", "Guantes", "Calzado", "Arnés", "Lentes" };

    private static readonly string[] CharacterLayerNames =
    {
        "HelmetLayer", "GlassesLayer", "HarnessLayer", "ShirtLayer", "GlovesLayer", "PantsLayer",
        "BootsLayer", "JeansLayer", "CasualShirtLayer", "WatchLayer", "BalacaLayer (1)",
        "EarringsLayer (1)", "Chanclas Layer"
    };

    private Transform gameUI;
    private Transform backplates;
    private Transform controls;
    private Transform catalogue;

    private readonly List<DraggableCloth> clothes = new List<DraggableCloth>();
    private readonly List<ClothSlot> slots = new List<ClothSlot>();
    private readonly List<Category> categories = new List<Category>();
    private readonly List<Card> cards = new List<Card>();
    private readonly List<OverviewRow> rows = new List<OverviewRow>();
    private readonly Dictionary<DraggableCloth, Sprite> thumbnails = new Dictionary<DraggableCloth, Sprite>();
    private readonly Dictionary<DraggableCloth.ClothType, Sprite> typeIcons = new Dictionary<DraggableCloth.ClothType, Sprite>();

    private ModuleDressUpEPPSManager manager;
    private string selectedCategory = "Cabeza";
    private float nextRefresh;
    private bool initialized;

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
        if (!scene.IsValid() || scene.name != "Module1EPPS") return;

        GameObject ui = GameObject.Find("UI");
        Transform game = ui != null ? ui.transform.Find("Game UI") : null;
        if (game == null)
        {
            Debug.LogWarning("EPPVisualLayoutController: no se encontró UI/Game UI en Module1EPPS.");
            return;
        }

        if (game.Find(RootName) != null) return;
        game.gameObject.AddComponent<EPPVisualLayoutController>().Initialize(game);
    }

    private void Initialize(Transform root)
    {
        if (initialized) return;
        initialized = true;
        gameUI = root;
        manager = FindFirstObjectByType<ModuleDressUpEPPSManager>();

        foreach (DraggableCloth cloth in root.GetComponentsInChildren<DraggableCloth>(true))
            if (cloth != null && !clothes.Contains(cloth)) clothes.Add(cloth);

        foreach (ClothSlot slot in root.GetComponentsInChildren<ClothSlot>(true))
            if (slot != null && !slots.Contains(slot)) slots.Add(slot);

        CacheThumbnails();

        BuildRoots();
        BuildStage();
        PrepareCharacter();
        BuildSidebar();
        BuildCatalogue();
        ShuffleCards();
        BuildOverview();
        BuildFooter();

        SelectCategory(selectedCategory);
    }

    private void CacheThumbnails()
    {
        foreach (DraggableCloth cloth in clothes)
        {
            Image image = cloth.GetComponent<Image>();
            Sprite sprite = image != null ? image.sprite : null;
            thumbnails[cloth] = sprite;

            if (sprite != null && cloth.IsCorrect && !typeIcons.ContainsKey(cloth.Cloth))
                typeIcons[cloth.Cloth] = sprite;
        }
    }

    // ------------------------------------------------------------------- roots

    private void BuildRoots()
    {
        Transform background = gameUI.Find("Image");

        GameObject back = new GameObject(RootName + "Back", typeof(RectTransform));
        back.transform.SetParent(gameUI, false);
        backplates = back.transform;
        Stretch(back.GetComponent<RectTransform>());

        GameObject front = new GameObject(RootName, typeof(RectTransform));
        front.transform.SetParent(gameUI, false);
        controls = front.transform;
        Stretch(front.GetComponent<RectTransform>());

        // Stage sits directly above the room artwork but behind the character.
        if (background != null)
            backplates.SetSiblingIndex(background.GetSiblingIndex() + 1);

        controls.SetAsLastSibling();
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
    }

    // ------------------------------------------------------------------- stage

    private void BuildStage()
    {
        EPPStyleKit.NewImage(backplates, "StageShadow", new Vector2(0f, -378f), new Vector2(556f, 122f),
            EPPStyleKit.Disc(), new Color32(16, 41, 74, 40), false, false);

        EPPStyleKit.NewImage(backplates, "StageRing", new Vector2(0f, -372f), new Vector2(540f, 110f),
            EPPStyleKit.Disc(), EPPStyleKit.PlatformRing, false, false);

        EPPStyleKit.NewImage(backplates, "StagePlatform", new Vector2(0f, -370f), new Vector2(524f, 98f),
            EPPStyleKit.Disc(), EPPStyleKit.Platform, false, false);
    }

    private void PrepareCharacter()
    {
        List<RectTransform> parts = new List<RectTransform>();

        RectTransform mannequin = FindRect("Maniqui");
        if (mannequin != null) parts.Add(mannequin);

        foreach (string layerName in CharacterLayerNames)
        {
            RectTransform layer = FindRect(layerName);
            if (layer != null && !parts.Contains(layer)) parts.Add(layer);
        }

        // Slots stay as invisible drop targets around the body.
        foreach (ClothSlot slot in slots)
        {
            if (slot == null) continue;
            RectTransform rect = slot.GetComponent<RectTransform>();
            if (rect == null) continue;

            rect.sizeDelta = new Vector2(112f, 112f);
            Image image = slot.GetComponent<Image>();
            if (image != null)
            {
                Color color = image.color;
                color.a = 0f;
                image.color = color;
                image.raycastTarget = true;
            }

            parts.Add(rect);
        }

        Vector2 anchorPoint = new Vector2(0f, -35f);
        foreach (RectTransform part in parts)
            EPPStyleKit.ScaleAbout(part, anchorPoint, CharacterScale);

        // Leftover template buttons from the vehicle modules are not part of this screen.
        Transform dispatch = gameUI.Find("Dispatch_Button");
        if (dispatch != null) dispatch.gameObject.SetActive(false);
    }

    private RectTransform FindRect(string name)
    {
        Transform found = gameUI.Find(name);
        return found as RectTransform;
    }

    // ----------------------------------------------------------------- sidebar

    private void BuildSidebar()
    {
        EPPStyleKit.Label(controls, "RailTitle", "EQUIPO", new Vector2(-878f, 320f), new Vector2(150f, 30f),
            16f, EPPStyleKit.InkSoft, true, TextAlignmentOptions.MidlineLeft);

        for (int i = 0; i < CategoryKeys.Length; i++)
        {
            Category category = new Category
            {
                Key = CategoryKeys[i],
                Title = CategoryTitles[i],
                Types = CategoryTypes[i]
            };

            Vector2 position = new Vector2(SidebarX, SidebarY[i]);
            Vector2 size = new Vector2(SidebarWidth, SidebarHeight);

            Image fill = EPPStyleKit.Card(controls, "Category_" + category.Key, position, size,
                EPPStyleKit.RadiusPanel, EPPStyleKit.CardWhite, EPPStyleKit.BorderSoft, true, true);

            Button button = fill.gameObject.AddComponent<Button>();
            button.targetGraphic = fill;

            string key = category.Key;
            button.onClick.AddListener(() => SelectCategory(key));

            Sprite icon = FirstIcon(category.Types);
            if (icon != null)
            {
                Image iconImage = EPPStyleKit.NewImage(fill.transform, "Icon", new Vector2(-88f, 0f),
                    new Vector2(46f, 46f), icon, Color.white, false, false);
                iconImage.preserveAspect = true;
            }

            EPPStyleKit.Label(fill.transform, "Label", category.Key, new Vector2(10f, 0f), new Vector2(132f, 40f),
                22f, EPPStyleKit.Ink, true, TextAlignmentOptions.MidlineLeft);

            category.Chevron = EPPStyleKit.Label(fill.transform, "Chevron", ">", new Vector2(100f, 0f),
                new Vector2(28f, 40f), 26f, EPPStyleKit.InkSoft, true, TextAlignmentOptions.Center);

            Image badge = EPPStyleKit.NewImage(fill.transform, "Done", new Vector2(96f, 28f),
                new Vector2(26f, 26f), EPPStyleKit.Disc(), EPPStyleKit.Green, false, false);
            EPPStyleKit.NewImage(badge.transform, "Tick", Vector2.zero, new Vector2(18f, 18f),
                EPPStyleKit.Check(), EPPStyleKit.PureWhite, false, false);
            badge.gameObject.SetActive(false);

            category.Fill = fill;
            category.Border = SiblingImage(fill, "_Border");
            category.Badge = badge;
            categories.Add(category);
        }
    }

    private Sprite FirstIcon(DraggableCloth.ClothType[] types)
    {
        foreach (DraggableCloth.ClothType type in types)
            if (typeIcons.TryGetValue(type, out Sprite sprite) && sprite != null) return sprite;
        return null;
    }

    // --------------------------------------------------------------- catalogue

    private void BuildCatalogue()
    {
        Vector2 panelCentre = new Vector2(CatalogueX, -20f);
        EPPStyleKit.Card(controls, "CataloguePanel", panelCentre, new Vector2(350f, 470f),
            EPPStyleKit.RadiusPanel, EPPStyleKit.CardWhite, EPPStyleKit.BorderSoft, false, true);

        EPPStyleKit.Label(controls, "CatalogueTitle", "Cascos", new Vector2(-504f, CatalogueTop - 40f),
            new Vector2(300f, 46f), 30f, EPPStyleKit.Ink, true, TextAlignmentOptions.MidlineLeft);

        EPPStyleKit.Label(controls, "CatalogueHint", "Elige una prenda", new Vector2(-504f, CatalogueTop - 76f),
            new Vector2(300f, 24f), 14f, EPPStyleKit.InkSoft, false, TextAlignmentOptions.MidlineLeft);

        RectTransform content = EPPStyleKit.NewRect(controls, "CatalogueContent",
            new Vector2(CatalogueX, CatalogueGridY), new Vector2(CardGap * 2f - (CardGap - CardSize), CardGap * 2f));

        foreach (DraggableCloth cloth in clothes)
        {
            if (cloth == null) continue;

            RectTransform cardRoot = EPPStyleKit.NewRect(content, "Card_" + cloth.name, Vector2.zero,
                new Vector2(CardSize, CardSize));

            Image fill = EPPStyleKit.Card(cardRoot, "Card", Vector2.zero, new Vector2(CardSize, CardSize),
                EPPStyleKit.RadiusCard, EPPStyleKit.CardWhite, EPPStyleKit.BorderSoft, true, true);

            Button button = fill.gameObject.AddComponent<Button>();
            button.targetGraphic = fill;
            DraggableCloth captured = cloth;
            button.onClick.AddListener(() => OnItemClicked(captured));

            Image badge = EPPStyleKit.NewImage(fill.transform, "Selected", new Vector2(58f, 58f),
                new Vector2(30f, 30f), EPPStyleKit.Disc(), EPPStyleKit.Green, false, false);
            EPPStyleKit.NewImage(badge.transform, "Tick", Vector2.zero, new Vector2(20f, 20f),
                EPPStyleKit.Check(), EPPStyleKit.PureWhite, false, false);
            badge.gameObject.SetActive(false);

            // The draggable item itself is the thumbnail: it keeps drag & drop working
            // while the card owns the click handler and the selection state.
            RectTransform clothRect = cloth.transform as RectTransform;
            if (clothRect != null)
            {
                clothRect.SetParent(fill.transform, false);
                clothRect.anchorMin = clothRect.anchorMax = new Vector2(0.5f, 0.5f);
                clothRect.pivot = new Vector2(0.5f, 0.5f);
                clothRect.anchoredPosition = Vector2.zero;

                // Fit from the sprite's own proportions, never from the rect the item
                // happened to have before it was re-skinned.
                Image thumbnail = cloth.GetComponent<Image>();
                Vector2 natural = thumbnail != null && thumbnail.sprite != null
                    ? new Vector2(thumbnail.sprite.rect.width, thumbnail.sprite.rect.height)
                    : clothRect.sizeDelta;
                clothRect.sizeDelta = Fit(natural, 114f, 114f);
                clothRect.localScale = Vector3.one;

                DraggableObject draggable = cloth.GetComponent<DraggableObject>();
                if (draggable != null) draggable.SetInventoryHome(fill.transform, Vector2.zero);
            }

            cards.Add(new Card
            {
                Cloth = cloth,
                Root = cardRoot,
                Fill = fill,
                Border = SiblingImage(fill, "_Border"),
                Badge = badge
            });
        }

        catalogue = content;
    }

    private static Vector2 Fit(Vector2 current, float maxWidth, float maxHeight)
    {
        if (current.x <= 0f || current.y <= 0f) return new Vector2(maxWidth, maxHeight);
        float factor = Mathf.Min(maxWidth / current.x, maxHeight / current.y);
        return current * factor;
    }

    // --------------------------------------------------------------- shuffle

    /// <summary>Fisher-Yates over the cards so the correct garment never has a fixed spot.</summary>
    private void ShuffleCards()
    {
        for (int i = cards.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            Card swap = cards[i];
            cards[i] = cards[j];
            cards[j] = swap;
        }
    }

    private void OnItemClicked(DraggableCloth cloth)
    {
        if (cloth == null) return;

        ClothSlot target = FindSlot(cloth.Cloth);
        if (target == null) return;

        if (target.GetCurrentCloth() == cloth) target.UnequipCurrent();
        else target.TryEquip(cloth);

        RefreshState();
    }

    private ClothSlot FindSlot(DraggableCloth.ClothType type)
    {
        foreach (ClothSlot slot in slots)
            if (slot != null && slot.AcceptedType == type) return slot;
        return null;
    }

    // ---------------------------------------------------------------- overview

    private void BuildOverview()
    {
        EPPStyleKit.Card(controls, "OverviewPanel", new Vector2(OverviewX, -20f), new Vector2(350f, 640f),
            EPPStyleKit.RadiusPanel, EPPStyleKit.CardWhite, EPPStyleKit.BorderSoft, false, true);

        EPPStyleKit.Label(controls, "OverviewTitle", "Vista general", new Vector2(781f, OverviewTop - 40f),
            new Vector2(320f, 46f), 30f, EPPStyleKit.Ink, true, TextAlignmentOptions.MidlineLeft);

        for (int i = 0; i < OverviewTypes.Length; i++)
        {
            float y = OverviewRowStart - i * OverviewRowStep;

            EPPStyleKit.NewImage(controls, "Row_" + i, new Vector2(OverviewX, y), new Vector2(302f, 66f),
                EPPStyleKit.Rounded(EPPStyleKit.RadiusTile), EPPStyleKit.RowBlue, true, false);

            Image icon = EPPStyleKit.NewImage(controls, "RowIcon_" + i, new Vector2(651f, y),
                new Vector2(46f, 46f), FirstIcon(new[] { OverviewTypes[i] }), Color.white, false, false);
            icon.preserveAspect = true;

            EPPStyleKit.Label(controls, "RowName_" + i, OverviewTitles[i], new Vector2(824f, y + 13f),
                new Vector2(215f, 26f), 19f, EPPStyleKit.Ink, true, TextAlignmentOptions.MidlineLeft);

            TMP_Text value = EPPStyleKit.Label(controls, "RowValue_" + i, "Sin seleccionar",
                new Vector2(824f, y - 13f), new Vector2(215f, 22f), 14f, EPPStyleKit.InkSoft, false,
                TextAlignmentOptions.MidlineLeft);

            rows.Add(new OverviewRow { Type = OverviewTypes[i], Icon = icon, Value = value });
        }
    }

    // ------------------------------------------------------------------ footer

    private void BuildFooter()
    {
        Button back = EPPStyleKit.NewButton(controls, "BackButton", new Vector2(-790f, -470f),
            new Vector2(230f, 68f), EPPStyleKit.RadiusPill, EPPStyleKit.PureWhite, EPPStyleKit.BorderSoft,
            EPPStyleKit.Ink, "←   Volver", 24f);
        back.onClick.AddListener(Back);

        Button finish = EPPStyleKit.NewButton(controls, "FinishButton", new Vector2(770f, -470f),
            new Vector2(270f, 70f), EPPStyleKit.RadiusPill, EPPStyleKit.Green, EPPStyleKit.GreenDeep,
            EPPStyleKit.PureWhite, "Finalizar   →", 26f);
        finish.onClick.AddListener(Finish);
    }

    // ------------------------------------------------------------------- state

    private void SelectCategory(string key)
    {
        selectedCategory = key;

        Category active = null;
        foreach (Category category in categories)
            if (category.Key == key) { active = category; break; }

        if (active != null)
        {
            TMP_Text title = FindTitle(controls, "CatalogueTitle");
            if (title != null) title.text = active.Title;
        }

        int index = 0;
        foreach (Card card in cards)
        {
            bool visible = active != null && Contains(active.Types, card.Cloth.Cloth);
            card.Root.gameObject.SetActive(visible);
            if (!visible) continue;

            card.Root.anchoredPosition = new Vector2(
                index % 2 == 0 ? -CardGap * 0.5f : CardGap * 0.5f,
                index < 2 ? CardGap * 0.5f : -CardGap * 0.5f);
            index++;
        }

        RefreshState();
    }

    private static TMP_Text FindTitle(Transform parent, string name)
    {
        Transform found = parent.Find(name);
        return found != null ? found.GetComponent<TMP_Text>() : null;
    }

    private static bool Contains(DraggableCloth.ClothType[] types, DraggableCloth.ClothType type)
    {
        foreach (DraggableCloth.ClothType candidate in types)
            if (candidate == type) return true;
        return false;
    }

    private void RefreshState()
    {
        foreach (Category category in categories)
        {
            bool isActive = category.Key == selectedCategory;
            bool isComplete = IsCategoryComplete(category);

            if (category.Fill != null)
                category.Fill.color = isActive ? EPPStyleKit.PaleGreen : EPPStyleKit.CardWhite;

            if (category.Border != null)
                category.Border.color = isActive ? EPPStyleKit.Green : EPPStyleKit.BorderSoft;

            if (category.Chevron != null)
            {
                category.Chevron.gameObject.SetActive(!isComplete);
                category.Chevron.color = isActive ? EPPStyleKit.Green : EPPStyleKit.InkSoft;
            }

            if (category.Badge != null) category.Badge.gameObject.SetActive(isComplete);
        }

        foreach (Card card in cards)
        {
            ClothSlot slot = FindSlot(card.Cloth.Cloth);
            bool equipped = slot != null && slot.GetCurrentCloth() == card.Cloth;

            if (card.Border != null)
                card.Border.color = equipped ? EPPStyleKit.Green : EPPStyleKit.BorderSoft;

            if (card.Badge != null) card.Badge.gameObject.SetActive(equipped);
        }

        foreach (OverviewRow row in rows)
        {
            ClothSlot slot = FindSlot(row.Type);
            DraggableCloth worn = slot != null ? slot.GetCurrentCloth() : null;

            if (row.Value != null)
            {
                row.Value.text = worn != null ? DisplayName(worn) : "Sin seleccionar";
                row.Value.color = worn != null ? EPPStyleKit.InkSoft : new Color32(150, 166, 184, 255);
            }

            if (row.Icon != null)
            {
                Sprite sprite = worn != null && thumbnails.TryGetValue(worn, out Sprite wornSprite) && wornSprite != null
                    ? wornSprite
                    : FirstIcon(new[] { row.Type });
                if (sprite != null) row.Icon.sprite = sprite;
                row.Icon.color = worn != null ? Color.white : new Color(1f, 1f, 1f, 0.55f);
            }
        }
    }

    private bool IsCategoryComplete(Category category)
    {
        foreach (DraggableCloth.ClothType type in category.Types)
        {
            ClothSlot slot = FindSlot(type);
            DraggableCloth worn = slot != null ? slot.GetCurrentCloth() : null;
            if (worn == null || !worn.IsCorrect) return false;
        }
        return true;
    }

    private static string DisplayName(DraggableCloth cloth)
    {
        return cloth.name.Trim();
    }

    private void Update()
    {
        if (Time.unscaledTime < nextRefresh) return;
        nextRefresh = Time.unscaledTime + 0.25f;

        RefreshState();
        if (manager != null) manager.RefreshProgress();
    }

    // ----------------------------------------------------------------- actions

    private void Finish()
    {
        if (manager == null) manager = FindFirstObjectByType<ModuleDressUpEPPSManager>();
        if (manager != null) manager.FinishModule();
        else Debug.LogError("EPPVisualLayoutController: no se encontró ModuleDressUpEPPSManager.");
    }

    private void Back()
    {
        string[] candidates = { "Alturas Initial Menu", "Initial Menu game" };
        foreach (string sceneName in candidates)
        {
            if (!Application.CanStreamedLevelBeLoaded(sceneName)) continue;
            SceneManager.LoadScene(sceneName);
            return;
        }

        int previous = SceneManager.GetActiveScene().buildIndex - 1;
        if (previous >= 0)
        {
            SceneManager.LoadScene(previous);
            return;
        }

        Debug.LogWarning("EPPVisualLayoutController: no hay escena de menú configurada para volver.");
    }

    // ------------------------------------------------------------------ helpers

    private static Image SiblingImage(Image fill, string suffix)
    {
        Transform parent = fill.transform.parent;
        if (parent == null) return null;
        Transform found = parent.Find(fill.name + suffix);
        return found != null ? found.GetComponent<Image>() : null;
    }
}
