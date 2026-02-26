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

    [Header("Animaciones")]
    [SerializeField] private float hoverScale = 1.15f;
    [SerializeField] private float hoverDuration = 0.3f;
    [SerializeField] private float pulseScale = 1.1f;
    [SerializeField] private float pulseDuration = 1f;

    private static bool allowClicks = false;
    private static float clickEnableTime = 0f;
    private Vector3 originalScale;
    private Color originalColor;
    private bool isCompleted = false;
    private bool isHovering = false;

    // NUEVO: Para guardar el sprite original
    private Sprite originalSprite;

    public void Initialize(LiquidInspectionManager manager, LiquidLevelPanel panel)
    {
        liquidManager = manager;
        liquidPanel = panel;

        if (image == null)
            image = GetComponentInChildren<SpriteRenderer>();

        if (image != null)
        {
            originalScale = image.transform.localScale;
            originalColor = image.color;
            originalSprite = image.sprite; // NUEVO: Guardar sprite original

            // NUEVO: Resetear estado visual
            ResetVisuals();

            // Iniciar animación de pulso suave al inicio
            StartIdlePulse();
        }
    }

    // NUEVO: Resetear visuales al reiniciar nivel
    private void ResetVisuals()
    {
        if (image == null)
            return;

        // Cancelar todas las animaciones
        LeanTween.cancel(image.gameObject);

        // Restaurar valores originales
        image.gameObject.SetActive(true);
        image.sprite = originalSprite;
        image.transform.localScale = originalScale;
        image.color = originalColor;

        // Resetear estados
        isCompleted = false;
        isHovering = false;

        Debug.Log($"[{vehiclePart}] Visuales reseteados");
    }

    // NUEVO: Método público para resetear desde el manager
    public void ResetTask()
    {
        ResetVisuals();
        StartIdlePulse();
    }

    public static void EnableClicks()
    {
        clickEnableTime = Time.time + 0.5f;
        allowClicks = false;
    }

    public static void DisableClicks()
    {
        allowClicks = false;
    }

    private void Update()
    {
        if (!allowClicks && Time.time >= clickEnableTime && clickEnableTime > 0)
        {
            allowClicks = true;
            Debug.Log("Clics habilitados en las tareas");
        }

        if (!allowClicks)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            if (liquidPanel != null && liquidPanel.gameObject.activeSelf)
            {
                return;
            }

            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

            if (hit.collider != null)
            {
                LiquidInspectionTask task = hit.transform.GetComponentInParent<LiquidInspectionTask>();
                if (task == this && !isCompleted)
                {
                    OnTaskSelected?.Invoke(this);
                    OnClickAnimation();
                }
            }
        }

        DetectHover();
    }

    private void DetectHover()
    {
        if (isCompleted || !allowClicks)
            return;

        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

        bool isMouseOver = hit.collider != null && hit.transform.GetComponentInParent<LiquidInspectionTask>() == this;

        if (isMouseOver && !isHovering)
        {
            OnHoverEnter();
        }
        else if (!isMouseOver && isHovering)
        {
            OnHoverExit();
        }
    }

    private void OnHoverEnter()
    {
        if (image == null || isCompleted)
            return;

        isHovering = true;

        LeanTween.cancel(image.gameObject);

        LeanTween.scale(image.gameObject, originalScale * hoverScale, hoverDuration)
            .setEase(LeanTweenType.easeOutBack);

        Vector3 currentPos = image.transform.localPosition;
        LeanTween.moveLocalY(image.gameObject, currentPos.y + 0.1f, hoverDuration)
            .setEase(LeanTweenType.easeOutQuad);
    }

    private void OnHoverExit()
    {
        if (image == null || isCompleted)
            return;

        isHovering = false;

        LeanTween.cancel(image.gameObject);

        LeanTween.scale(image.gameObject, originalScale, hoverDuration)
            .setEase(LeanTweenType.easeInOutQuad);

        Vector3 currentPos = image.transform.localPosition;
        LeanTween.moveLocalY(image.gameObject, currentPos.y - 0.1f, hoverDuration)
            .setEase(LeanTweenType.easeInOutQuad);

        if (!isCompleted)
        {
            LeanTween.delayedCall(hoverDuration, () => {
                if (!isHovering && !isCompleted)
                    StartIdlePulse();
            });
        }
    }

    private void OnClickAnimation()
    {
        if (image == null)
            return;

        LeanTween.cancel(image.gameObject);
        isHovering = false;

        LeanTween.scale(image.gameObject, originalScale * 0.9f, 0.1f)
            .setEase(LeanTweenType.easeInQuad)
            .setOnComplete(() => {
                LeanTween.scale(image.gameObject, originalScale, 0.2f)
                    .setEase(LeanTweenType.easeOutBack);
            });

        if (image is SpriteRenderer sprite)
        {
            Color currentColor = sprite.color;
            LeanTween.value(image.gameObject, 0f, 1f, 0.3f)
                .setOnUpdate((float val) => {
                    sprite.color = Color.Lerp(Color.white, currentColor, val);
                });
        }
    }

    private void StartIdlePulse()
    {
        if (image == null || isCompleted || isHovering)
            return;

        LeanTween.cancel(image.gameObject);

        LeanTween.scale(image.gameObject, originalScale * pulseScale, pulseDuration)
            .setEase(LeanTweenType.easeInOutSine)
            .setLoopPingPong();
    }

    public void MarkAsCompleted()
    {
        if (image == null || isCompleted)
            return;

        isCompleted = true;

        LeanTween.cancel(image.gameObject);

        LeanTween.scale(image.gameObject, Vector3.zero, 0.5f)
            .setEase(LeanTweenType.easeInBack);

        if (image is SpriteRenderer sprite)
        {
            LeanTween.value(image.gameObject, sprite.color.a, 0f, 0.5f)
                .setOnUpdate((float alpha) => {
                    Color c = sprite.color;
                    c.a = alpha;
                    sprite.color = c;
                });
        }

        LeanTween.delayedCall(0.5f, () => {
            if (image != null)
                image.gameObject.SetActive(false);
        });
    }

    public void ShowErrorAnimation()
    {
        if (image == null)
            return;

        LeanTween.cancel(image.gameObject);

        Vector3 originalPos = image.transform.localPosition;

        LeanTween.moveLocalX(image.gameObject, originalPos.x + 0.1f, 0.05f)
            .setEase(LeanTweenType.easeInOutQuad)
            .setLoopPingPong()
            .setRepeat(4)
            .setOnComplete(() => {
                image.transform.localPosition = originalPos;

                if (!isCompleted)
                    StartIdlePulse();
            });

        if (image is SpriteRenderer sprite)
        {
            Color currentColor = sprite.color;
            LeanTween.value(image.gameObject, 0f, 1f, 0.5f)
                .setOnUpdate((float val) => {
                    sprite.color = Color.Lerp(Color.red, currentColor, val);
                });
        }
    }

    private void OnDisable()
    {
        if (image != null)
            LeanTween.cancel(image.gameObject);

        allowClicks = false;
        clickEnableTime = 0f;
    }

    private void OnDestroy()
    {
        if (image != null)
            LeanTween.cancel(image.gameObject);
    }
}