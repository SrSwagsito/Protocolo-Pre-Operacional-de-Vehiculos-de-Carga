using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableDocument : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Document Information")]
    [SerializeField] private string documentName;
    [SerializeField] private bool isCorrectDocument = true;

    [Header("UI References")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Image documentImage;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Config Animations")]
    float hoverScale = 1.1f;
    float hoverDuration = 0.2f;
    float dragRotation = 5f;

    DocumentCheckerManager documentCheckerManager;
    RectTransform rectTransform;
    Vector3 originalPosition;
    Vector3 originalScale;
    Transform originalParent;
    Canvas canvas;
    bool isDraggin;

    public string DocumentName => documentName;
    public bool IsRequired => isCorrectDocument;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        originalScale = transform.localScale;
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
        
        if (nameText != null)
        {
            nameText.text = documentName;
        }
    }

    public void Initialize(DocumentCheckerManager documentManager)
    {
        documentCheckerManager = documentManager;
        originalPosition = rectTransform.anchoredPosition;
        originalParent = transform.parent;

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isDraggin)
        {
            LeanTween.cancel(gameObject);

            LeanTween.scale(gameObject, originalScale * hoverScale, hoverDuration).setEase(LeanTweenType.easeOutBack);
        }
    }

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
        AudioManager.Instance.Play("Document Drag");
        isDraggin = true;

        originalPosition = rectTransform.position;
        originalParent = transform.parent;

        LeanTween.cancel(gameObject);

        transform.localScale = originalScale;

        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;

        transform.SetParent(canvas.transform);

        LeanTween.rotateZ(gameObject, dragRotation, 0.2f).setEase(LeanTweenType.easeOutBack);
    }   

    public void OnDrag(PointerEventData eventData)
    {
        
        rectTransform.position = eventData.position;
    }



    public void OnEndDrag(PointerEventData eventData)
    {
        isDraggin = false;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        bool droppedZone = false;

        if(eventData.pointerEnter != null)
        {
            DocumentDropZone dropZone = eventData.pointerEnter.GetComponent<DocumentDropZone>();
            if (dropZone != null)
            {
                droppedZone = true;
                dropZone.ReceiveDocument(this);
            }
        }

        if(!droppedZone)
        {

            AudioManager.Instance.Play("Document regret");
            transform.SetParent(originalParent);
            rectTransform.position = originalPosition;

           
        }
    }

    public void ReturnToOriginalPosition()
    {
        transform.SetParent(originalParent);
        rectTransform.anchoredPosition = originalPosition;

        LeanTween.cancel(gameObject);

        LeanTween.move(rectTransform, originalPosition, 0.3f).setEase(LeanTweenType.easeOutBack);

        LeanTween.rotateZ(gameObject, 0f, 0.3f).setEase(LeanTweenType.easeOutBack);
    }
}
