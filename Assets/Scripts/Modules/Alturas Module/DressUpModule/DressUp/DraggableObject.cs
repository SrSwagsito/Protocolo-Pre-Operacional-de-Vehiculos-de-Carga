using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableObject : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private CanvasGroup canvasGroup;

    private RectTransform rectTransform;
    private Vector2 startAnchoredPosition;
    private Transform startParent;
    private ClothSlot currentSlot;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (canvas == null) canvas = GetComponentInParent<Canvas>();
    }

    private void Start()
    {
        if (startParent == null)
        {
            startParent = transform.parent;
            startAnchoredPosition = rectTransform != null ? rectTransform.anchoredPosition : Vector2.zero;
        }
    }

    /// <summary>Allows a runtime UI layout to set the true inventory return location.</summary>
    public void SetInventoryHome(Transform parent, Vector2 anchoredPosition)
    {
        startParent = parent;
        startAnchoredPosition = anchoredPosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        if (canvasGroup != null) canvasGroup.blocksRaycasts = false;

        if (currentSlot != null)
        {
            currentSlot.RemoveCloth();
            currentSlot = null;
        }

        if (canvas == null) canvas = GetComponentInParent<Canvas>();
        if (canvas != null) transform.SetParent(canvas.transform, true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvas == null || rectTransform == null) return;
        RectTransform canvasRect = canvas.transform as RectTransform;
        if (canvasRect == null) return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
            rectTransform.localPosition = localPoint;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (canvasGroup != null) canvasGroup.blocksRaycasts = true;
        if (currentSlot == null) ReturnToStart();
    }

    public void SetInSlot(ClothSlot slot) => currentSlot = slot;
    public void ClearSlot() => currentSlot = null;

    public void ReturnToStart()
    {
        if (startParent == null || rectTransform == null) return;
        transform.SetParent(startParent, false);
        rectTransform.anchorMin = rectTransform.anchorMax = new Vector2(.5f, .5f);
        rectTransform.pivot = new Vector2(.5f, .5f);
        rectTransform.anchoredPosition = startAnchoredPosition;
    }
}
