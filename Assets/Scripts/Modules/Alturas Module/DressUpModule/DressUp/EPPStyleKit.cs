using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Style tokens and procedural chrome for the EPP dress-up screen.
/// Rounded cards, pills and badges are generated as 9-sliced sprites so the look
/// does not depend on hand-authored art and stays crisp at any size.
/// </summary>
public static class EPPStyleKit
{
    // Palette sampled from the reference design (soft illustrated UI, navy text, green accent).
    public static readonly Color Ink = new Color32(16, 41, 74, 255);
    public static readonly Color InkSoft = new Color32(104, 125, 149, 255);
    public static readonly Color Green = new Color32(26, 158, 86, 255);
    public static readonly Color GreenDeep = new Color32(17, 130, 70, 255);
    public static readonly Color PaleGreen = new Color32(218, 242, 226, 255);
    public static readonly Color CardWhite = new Color32(255, 255, 255, 253);
    public static readonly Color RowBlue = new Color32(235, 242, 250, 255);
    public static readonly Color BorderSoft = new Color32(224, 233, 243, 255);
    public static readonly Color Shadow = new Color32(16, 41, 74, 24);
    public static readonly Color Platform = new Color32(248, 200, 72, 240);
    public static readonly Color PlatformRing = new Color32(226, 170, 40, 255);
    public static readonly Color PureWhite = new Color32(255, 255, 255, 255);

    public const int RadiusPanel = 26;
    public const int RadiusCard = 22;
    public const int RadiusTile = 16;
    public const int RadiusPill = 30;

    private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

    // ---------------------------------------------------------------- sprites

    public static Sprite Rounded(int radius, int outline = 0)
    {
        string key = "r" + radius + "_" + outline;
        if (Cache.TryGetValue(key, out Sprite cached) && cached != null) return cached;

        // 9-sliced: the corner block must contain the whole curve plus the outline.
        int border = radius + outline;
        int size = border * 2 + 2;
        float half = size * 0.5f;

        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            name = key,
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
            hideFlags = HideFlags.HideAndDontSave
        };

        Color32[] pixels = new Color32[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float px = x + 0.5f - half;
                float py = y + 0.5f - half;

                // Rounded-box signed distance, box half extents shrink by the corner radius.
                float dx = Mathf.Abs(px) - (half - 1f - radius);
                float dy = Mathf.Abs(py) - (half - 1f - radius);
                float outsideX = Mathf.Max(dx, 0f);
                float outsideY = Mathf.Max(dy, 0f);
                float outside = Mathf.Sqrt(outsideX * outsideX + outsideY * outsideY);
                float inside = Mathf.Min(Mathf.Max(dx, dy), 0f);
                float sd = outside + inside - radius;

                float alpha = Mathf.Clamp01(1f - sd);
                if (outline > 0)
                {
                    // Keep only the rim so the same shape can be used as a border.
                    float inner = Mathf.Clamp01(1f - (sd + outline));
                    alpha = Mathf.Clamp01(alpha - inner);
                }

                pixels[y * size + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(alpha * 255f));
            }
        }

        tex.SetPixels32(pixels);
        tex.Apply(false, false);

        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0,
            SpriteMeshType.FullRect, new Vector4(border, border, border, border));
        sprite.name = key;
        sprite.hideFlags = HideFlags.HideAndDontSave;
        Cache[key] = sprite;
        return sprite;
    }

    /// <summary>Filled disc, stretched by the RectTransform when an ellipse is needed.</summary>
    public static Sprite Disc(int outline = 0)
    {
        string key = "disc" + outline;
        if (Cache.TryGetValue(key, out Sprite cached) && cached != null) return cached;

        const int radius = 32;
        int size = radius * 2 + 2;
        float half = size * 0.5f;

        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            name = key,
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
            hideFlags = HideFlags.HideAndDontSave
        };

        Color32[] pixels = new Color32[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float px = x + 0.5f - half;
                float py = y + 0.5f - half;
                float sd = Mathf.Sqrt(px * px + py * py) - radius;

                float alpha = Mathf.Clamp01(1f - sd);
                if (outline > 0)
                {
                    float inner = Mathf.Clamp01(1f - (sd + outline));
                    alpha = Mathf.Clamp01(alpha - inner);
                }

                pixels[y * size + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(alpha * 255f));
            }
        }

        tex.SetPixels32(pixels);
        tex.Apply(false, false);

        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0,
            SpriteMeshType.FullRect);
        sprite.name = key;
        sprite.hideFlags = HideFlags.HideAndDontSave;
        Cache[key] = sprite;
        return sprite;
    }

    /// <summary>Checkmark glyph drawn as a two-segment capsule stroke.</summary>
    public static Sprite Check(int size = 48, float thickness = 8f)
    {
        string key = "check" + size + "_" + thickness;
        if (Cache.TryGetValue(key, out Sprite cached) && cached != null) return cached;

        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            name = key,
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
            hideFlags = HideFlags.HideAndDontSave
        };

        Vector2 a = new Vector2(0.22f, 0.53f) * size;
        Vector2 b = new Vector2(0.42f, 0.31f) * size;
        Vector2 c = new Vector2(0.80f, 0.70f) * size;
        float halfStroke = thickness * 0.5f;

        Color32[] pixels = new Color32[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 p = new Vector2(x + 0.5f, y + 0.5f);
                float d = Mathf.Min(DistanceToSegment(p, a, b), DistanceToSegment(p, b, c));
                float alpha = Mathf.Clamp01(halfStroke + 0.5f - d);
                pixels[y * size + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(alpha * 255f));
            }
        }

        tex.SetPixels32(pixels);
        tex.Apply(false, false);

        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0,
            SpriteMeshType.FullRect);
        sprite.name = key;
        sprite.hideFlags = HideFlags.HideAndDontSave;
        Cache[key] = sprite;
        return sprite;
    }

    /// <summary>Right-pointing play triangle drawn from three half-plane distances.</summary>
    public static Sprite Triangle(int size = 64)
    {
        string key = "triangle" + size;
        if (Cache.TryGetValue(key, out Sprite cached) && cached != null) return cached;

        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            name = key,
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
            hideFlags = HideFlags.HideAndDontSave
        };

        Vector2 a = new Vector2(0.26f, 0.12f) * size;
        Vector2 b = new Vector2(0.26f, 0.88f) * size;
        Vector2 c = new Vector2(0.86f, 0.50f) * size;

        Color32[] pixels = new Color32[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 p = new Vector2(x + 0.5f, y + 0.5f);
                float sd = Mathf.Max(Mathf.Max(HalfPlane(p, a, b, c), HalfPlane(p, b, c, a)),
                    HalfPlane(p, c, a, b));
                float alpha = Mathf.Clamp01(0.5f - sd);
                pixels[y * size + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(alpha * 255f));
            }
        }

        tex.SetPixels32(pixels);
        tex.Apply(false, false);

        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0,
            SpriteMeshType.FullRect);
        sprite.name = key;
        sprite.hideFlags = HideFlags.HideAndDontSave;
        Cache[key] = sprite;
        return sprite;
    }

    /// <summary>Signed distance to an edge line, negative on the side that contains <paramref name="other"/>.</summary>
    private static float HalfPlane(Vector2 p, Vector2 from, Vector2 to, Vector2 other)
    {
        Vector2 normal = new Vector2(-(to.y - from.y), to.x - from.x).normalized;
        if (Vector2.Dot(normal, other - from) > 0f) normal = -normal;
        return Vector2.Dot(normal, p - from);
    }

    private static float DistanceToSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        float lengthSq = ab.sqrMagnitude;
        if (lengthSq <= 0.0001f) return Vector2.Distance(p, a);
        float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / lengthSq);
        return Vector2.Distance(p, a + ab * t);
    }

    // ---------------------------------------------------------------- builders

    public static RectTransform NewRect(Transform parent, string name, Vector2 position, Vector2 size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        rect.localScale = Vector3.one;
        return rect;
    }

    public static Image NewImage(Transform parent, string name, Vector2 position, Vector2 size,
        Sprite sprite, Color color, bool sliced, bool raycast)
    {
        RectTransform rect = NewRect(parent, name, position, size);
        Image image = rect.gameObject.AddComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.type = sliced && sprite != null && sprite.border != Vector4.zero ? Image.Type.Sliced : Image.Type.Simple;
        image.raycastTarget = raycast;
        return image;
    }

    /// <summary>Card = shadow + fill + border ring (the ring is drawn on top of the fill).</summary>
    public static Image Card(Transform parent, string name, Vector2 position, Vector2 size, int radius,
        Color fill, Color? border = null, bool raycast = false, bool shadow = true)
    {
        if (shadow)
            NewImage(parent, name + "_Shadow", position + new Vector2(0f, -6f), size + new Vector2(6f, 6f),
                Rounded(radius), Shadow, true, false);

        RectTransform borderRect = null;
        if (border.HasValue)
            borderRect = NewImage(parent, name + "_Border", position, size + new Vector2(4f, 4f),
                Rounded(radius, 2), border.Value, true, false).rectTransform;

        Image image = NewImage(parent, name, position, size, Rounded(radius), fill, true, raycast);

        if (borderRect != null) borderRect.SetAsLastSibling();
        return image;
    }

    public static TextMeshProUGUI Label(Transform parent, string name, string value, Vector2 position,
        Vector2 size, float fontSize, Color color, bool bold, TextAlignmentOptions alignment)
    {
        RectTransform rect = NewRect(parent, name, position, size);
        TextMeshProUGUI text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        if (TMP_Settings.defaultFontAsset != null) text.font = TMP_Settings.defaultFontAsset;
        text.text = value;
        text.fontSize = fontSize;
        text.color = color;
        text.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        text.alignment = alignment;
        text.raycastTarget = false;
        text.overflowMode = TextOverflowModes.Overflow;
        return text;
    }

    public static Button NewButton(Transform parent, string name, Vector2 position, Vector2 size, int radius,
        Color fill, Color? border, Color labelColor, string label, float fontSize)
    {
        Image background = Card(parent, name, position, size, radius, fill, border, true, true);
        Button button = background.gameObject.AddComponent<Button>();
        button.targetGraphic = background;

        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(1f, 1f, 1f, 0.94f);
        colors.pressedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        colors.fadeDuration = 0.08f;
        button.colors = colors;

        Label(background.transform, name + "_Label", label, Vector2.zero, size - new Vector2(28f, 8f),
            fontSize, labelColor, true, TextAlignmentOptions.Center);
        return button;
    }

    /// <summary>Scales a UI element about an arbitrary point in its parent's local space.</summary>
    public static void ScaleAbout(RectTransform target, Vector2 fixedPoint, float scale)
    {
        if (target == null) return;
        RectTransform parent = target.parent as RectTransform;

        // target.position is the pivot point; convert it to the rect's visual centre.
        Vector2 pivotLocal = parent != null
            ? (Vector2)parent.InverseTransformPoint(target.position)
            : target.anchoredPosition;
        Vector2 pivotBias = Vector2.Scale(new Vector2(0.5f, 0.5f) - target.pivot, target.sizeDelta);
        Vector2 center = pivotLocal + pivotBias;

        target.anchorMin = target.anchorMax = new Vector2(0.5f, 0.5f);
        target.sizeDelta = target.sizeDelta * scale;

        Vector2 newCenter = fixedPoint + (center - fixedPoint) * scale;
        Vector2 newPivotBias = Vector2.Scale(new Vector2(0.5f, 0.5f) - target.pivot, target.sizeDelta);
        target.anchoredPosition = newCenter - newPivotBias;
    }
}
