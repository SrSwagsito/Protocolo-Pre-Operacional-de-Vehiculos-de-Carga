using UnityEngine;

/// <summary>
/// Marca un grupo de geometría del vehículo como una parte inspeccionable.
/// Guarda la información que se muestra en pantalla y el ángulo de cámara preferido al enfocarla.
/// </summary>
[DisallowMultipleComponent]
public class CarPart : MonoBehaviour
{
    public string partName = "Parte";
    [TextArea(3, 8)] public string description;

    [Tooltip("Ángulo horizontal de la cámara al enfocar (0 = desde atrás, 180 = desde el frente, 90 = lado izquierdo, -90 = lado derecho).")]
    public float viewYaw = 180f;
    [Tooltip("Ángulo vertical de la cámara al enfocar.")]
    public float viewPitch = 20f;
    [Tooltip("Multiplicador del tamaño de la parte para calcular la distancia de enfoque.")]
    public float distanceMultiplier = 2.6f;
    public float minFocusDistance = 1.2f;

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly Color HighlightColor = new Color(0.1f, 0.85f, 1f, 1f);

    private Renderer[] renderers;
    private Color[] baseColors;
    private MaterialPropertyBlock block;
    private float highlight;
    private float targetHighlight;
    private float lastApplied = -1f;

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        baseColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            Material m = renderers[i].sharedMaterial;
            baseColors[i] = m != null && m.HasProperty(BaseColorId) ? m.GetColor(BaseColorId) : Color.white;
        }
        block = new MaterialPropertyBlock();
    }

    /// <summary>0 = sin resaltar, 0.4 = hover, 1 = seleccionada.</summary>
    public void SetHighlightLevel(float level) => targetHighlight = level;

    public Bounds GetWorldBounds()
    {
        if (renderers == null || renderers.Length == 0)
            return new Bounds(transform.position, Vector3.one * 0.5f);

        Bounds b = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);
        return b;
    }

    private void Update()
    {
        highlight = Mathf.MoveTowards(highlight, targetHighlight, Time.unscaledDeltaTime * 4f);
        float pulse = targetHighlight >= 1f ? 0.7f + 0.3f * Mathf.Sin(Time.unscaledTime * 4f) : 1f;
        float t = highlight * pulse * 0.6f;

        if (Mathf.Approximately(t, lastApplied)) return;
        lastApplied = t;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (t <= 0.001f)
            {
                renderers[i].SetPropertyBlock(null);
                continue;
            }
            renderers[i].GetPropertyBlock(block);
            block.SetColor(BaseColorId, Color.Lerp(baseColors[i], HighlightColor, t));
            renderers[i].SetPropertyBlock(block);
        }
    }
}
