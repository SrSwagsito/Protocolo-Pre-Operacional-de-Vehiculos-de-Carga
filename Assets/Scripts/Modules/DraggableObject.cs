using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableObject : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private CanvasGroup canvasGroup;

    private RectTransform rectTransform;

    // POSICIONES CLAVE
    private Vector2 startAnchoredPosition;
    private Transform startParent;

    // SLOT ACTUAL
    private ClothSlot currentSlot;

    private bool isDragging;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        // Guardamos posición inicial REAL del layout
        startParent = transform.parent;
        startAnchoredPosition = rectTransform.anchoredPosition;
    }

    // -------------------------
    // DRAG START
    // -------------------------
    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;

        canvasGroup.blocksRaycasts = false;

        // Si estaba en un slot → lo liberamos
        if (currentSlot != null)
        {
            currentSlot.RemoveCloth();
            currentSlot = null;
        }

        transform.SetParent(canvas.transform, true);
    }

    // -------------------------
    // DRAG
    // -------------------------
    public void OnDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
    canvas.transform as RectTransform,
    eventData.position,
    eventData.pressEventCamera,
    out Vector2 localPoint);

        rectTransform.localPosition = localPoint;
    }

    // -------------------------
    // DRAG END
    // -------------------------
    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;

        canvasGroup.blocksRaycasts = true;

        // Si no fue aceptado → volver a inventario
        if (currentSlot == null)
        {
            ReturnToStart();
        }
    }

    // -------------------------
    // SLOT ACCEPTANCE
    // -------------------------
    public void SetInSlot(ClothSlot slot)
    {
        currentSlot = slot;
    }

    public void ClearSlot()
    {
        currentSlot = null;
    }

    // -------------------------
    // RETURN TO INVENTORY
    // -------------------------
    public void ReturnToStart()
    {
        transform.SetParent(startParent, false);
        rectTransform.anchoredPosition = startAnchoredPosition;
    }
}