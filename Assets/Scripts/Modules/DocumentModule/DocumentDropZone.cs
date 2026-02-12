using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class DocumentDropZone : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Feedback Visual")]
    [SerializeField] private GameObject highlightEffect;
    [SerializeField] private Color normalColor = new Color(1f, 1f, 1f, 0.3f);
    [SerializeField] private Color hoverColor = new Color(0.5f, 1f, 0.5f, 0.5f);

    [Header("Animación de Carpeta")]
    [SerializeField] private RectTransform folderTop; // La "tapa" de la carpeta que se abre
    [SerializeField] private float openAngle = -30f; // Ángulo de apertura
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private Vector3 folderClosedRotation = Vector3.zero; // Rotación cuando está cerrada

    private Image backgroundImage;
    private bool isFolderOpen = false;

    public Action<DraggableDocument> OnDocumentDropped;

    private void Awake()
    {
        backgroundImage = GetComponent<Image>();

        if (backgroundImage != null)
            backgroundImage.color = normalColor;

        if (highlightEffect != null)
        //highlightEffect.SetActive(false);


        // Asegurar que la carpeta inicia cerrada
        if (folderTop != null)
        {
            folderTop.localRotation = Quaternion.Euler(folderClosedRotation);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Cuando el mouse/dedo entra a la zona mientras arrastra
        if (eventData.pointerDrag != null)
        {
            if (backgroundImage != null)
                backgroundImage.color = hoverColor;

            //if (highlightEffect != null)
            //    highlightEffect.SetActive(true);

            // NUEVO: Abrir la carpeta con animación
            OpenFolder();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Cuando el mouse/dedo sale de la zona
        if (backgroundImage != null)
            backgroundImage.color = normalColor;

        if (highlightEffect != null)
        //highlightEffect.SetActive(false);

        // NUEVO: Cerrar la carpeta con animación
        CloseFolder();
    }

    public void OnDrop(PointerEventData eventData)
    {
        // Restablecer color
        if (backgroundImage != null)
            backgroundImage.color = normalColor;

        if (highlightEffect != null)
        //highlightEffect.SetActive(false);

        // NUEVO: Animación de que el documento entra en la carpeta
        // La carpeta se cierra después de recibir el documento
        CloseFolderWithDelay(0.2f);
    }

    public void ReceiveDocument(DraggableDocument document)
    {
        // NUEVO: Animar el documento entrando en la carpeta
        AnimateDocumentEnter(document);

        // Notificar al manager después de la animación
        OnDocumentDropped?.Invoke(document);
    }

    // NUEVO: Abrir carpeta
    private void OpenFolder()
    {
        if (folderTop == null || isFolderOpen)
            return;

        isFolderOpen = true;

        // Cancelar animaciones previas
        LeanTween.cancel(folderTop.gameObject);

        // Animar la apertura (rotar en eje Z)
        LeanTween.rotateZ(folderTop.gameObject, openAngle, animationDuration)
            .setEase(LeanTweenType.easeOutBack);
    }

    // NUEVO: Cerrar carpeta
    private void CloseFolder()
    {
        if (folderTop == null || !isFolderOpen)
            return;

        isFolderOpen = false;

        // Cancelar animaciones previas
        LeanTween.cancel(folderTop.gameObject);

        // Animar el cierre
        LeanTween.rotate(folderTop.gameObject, folderClosedRotation, animationDuration)
            .setEase(LeanTweenType.easeInOutQuad);
    }

    // NUEVO: Cerrar carpeta con delay (cuando recibe documento)
    private void CloseFolderWithDelay(float delay)
    {
        if (folderTop == null)
            return;

        LeanTween.cancel(folderTop.gameObject);

        // Esperar un momento y luego cerrar
        LeanTween.rotate(folderTop.gameObject, folderClosedRotation, animationDuration)
            .setDelay(delay)
            .setEase(LeanTweenType.easeInOutQuad)
            .setOnComplete(() => isFolderOpen = false);
    }

    // NUEVO: Animar el documento entrando en la carpeta
    private void AnimateDocumentEnter(DraggableDocument document)
    {
        if (document == null)
            return;

        RectTransform docRect = document.GetComponent<RectTransform>();

        // Cancelar animaciones previas del documento
        LeanTween.cancel(document.gameObject);

        // Guardar posición actual
        Vector3 startPos = docRect.position;
        Vector3 targetPos = transform.position; // Centro de la carpeta

        // Animar el documento hacia el centro de la carpeta
        LeanTween.move(docRect, targetPos, 0.3f)
            .setEase(LeanTweenType.easeInQuad);

        // Encoger el documento mientras entra
        LeanTween.scale(docRect, Vector3.zero, 0.3f)
            .setEase(LeanTweenType.easeInQuad)
            .setOnComplete(() => {
                // Desactivar el documento al terminar la animación
                document.gameObject.SetActive(false);
            });

        // Rotación mientras entra
        LeanTween.rotateZ(document.gameObject, 0f, 0.2f)
            .setEase(LeanTweenType.easeOutQuad);
    }

    private void OnDestroy()
    {
        // Cancelar todas las animaciones
        if (folderTop != null)
            LeanTween.cancel(folderTop.gameObject);
    }
}