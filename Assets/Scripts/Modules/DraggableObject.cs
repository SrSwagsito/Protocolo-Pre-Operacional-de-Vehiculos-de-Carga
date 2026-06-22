using TMPro;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableObject : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{

    [Header("UI References")]
    [SerializeField] private Image spriteOBJ;

    [Header("Settings")]
    [SerializeField] private float transparencyAmount = 0.6f;

    [Header("Config Animations")]
    float hoverScale = 1.1f;
    float hoverDuration = 0.2f;

    RectTransform rectTransform;
    Vector2 originalPosition;
    Vector3 originalScale;
    Transform originalParent;
    Canvas canvas;
    bool isDraggin;


    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
          
        originalScale = transform.localScale;
        canvas = GetComponentInParent<Canvas>();

        if (spriteOBJ == null) spriteOBJ = GetComponent<Image>();
    }

    public void Initialize()
    {
        originalPosition = rectTransform.anchoredPosition;
        originalParent = transform.parent;
    }


    //Dar Feedback de que me pueden arrastrar
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isDraggin)
        {
            LeanTween.cancel(gameObject);

            LeanTween.scale(gameObject, originalScale * hoverScale, hoverDuration).setEase(LeanTweenType.easeOutBack);
        }
    }

    //Dar Feedback de que me pueden arrastrar
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isDraggin)
        {
            LeanTween.cancel(gameObject);
            LeanTween.scale(gameObject, originalScale, hoverDuration).setEase(LeanTweenType.easeOutBack);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Sonido de arrastrar
        //AudioManager.Instance.Play("Document Drag");
        isDraggin = true;

        originalPosition = rectTransform.position;
        originalParent = transform.parent;

        LeanTween.cancel(gameObject);

        transform.localScale = originalScale;

        SetImageAlpha(transparencyAmount);
        spriteOBJ.raycastTarget = false;

        if(canvas != null) transform.SetParent(canvas.transform);
    }

    public void OnDrag(PointerEventData eventData)
    {

        rectTransform.position = eventData.position;
    }



    public void OnEndDrag(PointerEventData eventData)
    {
        isDraggin = false;
        SetImageAlpha(1f);
        spriteOBJ.raycastTarget = true;
    }

    private void SetImageAlpha(float alpha)
    {
        if (spriteOBJ != null)
        {
            Color color = spriteOBJ.color;
            color.a = alpha;
            spriteOBJ.color = color;
        }
    }

    public void ReturnToOriginalPosition()
    {
        transform.SetParent(originalParent, false);
        // rectTransform.anchoredPosition = originalPosition;

        LeanTween.cancel(gameObject);

        LeanTween.move(rectTransform, originalPosition, 0.3f).setEase(LeanTweenType.easeOutBack);

        LeanTween.rotateZ(gameObject, 0f, 0.3f).setEase(LeanTweenType.easeOutBack);
    }
}
