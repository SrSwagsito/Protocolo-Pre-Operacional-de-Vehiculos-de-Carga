using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class LiquidInspectionTask : MonoBehaviour
{
    public string vehiclePart;

    private LiquidInspectionManager liquidManager;
    private LiquidLevelPanel liquidPanel;

    public static Action<LiquidInspectionTask> OnTaskSelected;

    [SerializeField] private SpriteRenderer image;

    // ── Frame de gracia ───────────────────────────────────────────────────────
    // Guarda el frame en que este objeto se activó (o el módulo inició).
    // Ignoramos cualquier clic que ocurra en ese mismo frame o el siguiente,
    // así el clic que cerró el tutorial/UI anterior no se "cuela" aquí.
    private int frameActivo = -1;
    private const int FRAMES_GRACIA = 2;
    // ─────────────────────────────────────────────────────────────────────────

    public void Initialize(LiquidInspectionManager manager, LiquidLevelPanel panel)
    {
        liquidManager = manager;
        liquidPanel = panel;
        image = GetComponentInChildren<SpriteRenderer>();

        // Registrar el frame en que el módulo arranca
        frameActivo = Time.frameCount;
    }

    private void OnEnable()
    {
        // También registrar si el objeto se reactiva en medio del juego
        frameActivo = Time.frameCount;
    }

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        // ── Guardia 1: frame de gracia ────────────────────────────────────────
        if (Time.frameCount <= frameActivo + FRAMES_GRACIA) return;

        // ── Guardia 2: clic sobre UI ──────────────────────────────────────────
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        // ── Guardia 3: panel de líquido abierto ───────────────────────────────
        if (liquidPanel != null && liquidPanel.gameObject.activeSelf) return;

        // ── Raycast al mundo 2D ───────────────────────────────────────────────
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider == null) return;

        LiquidInspectionTask task = hit.transform.GetComponentInParent<LiquidInspectionTask>();
        if (task != this) return;

        // ── Acción ────────────────────────────────────────────────────────────
        OnTaskSelected?.Invoke(this);

        if (image != null)
            image.sprite = null;
    }
}