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
    private bool isCompleted = false;
    private bool isHovering = false;

    public void Initialize(LiquidInspectionManager manager, LiquidLevelPanel panel)
    {
        liquidManager = manager;
        liquidPanel = panel;
        image = GetComponentInChildren<SpriteRenderer>();

        if (image != null)
        {
            originalScale = image.transform.localScale;

            // Iniciar animación de pulso suave al inicio
            StartIdlePulse();
        }
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
        // Verificar si ya pasó el tiempo de espera
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

        // Detectar hover con el mouse
        DetectHover();
    }

    // NUEVO: Detectar cuando el mouse está sobre la tarea
    private void DetectHover()
    {
        if (isCompleted || !allowClicks)
            return;

        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

        bool isMouseOver = hit.collider != null && hit.transform.GetComponentInParent<LiquidInspectionTask>() == this;

        if (isMouseOver && !isHovering)
        {
            // Mouse entra
            OnHoverEnter();
        }
        else if (!isMouseOver && isHovering)
        {
            // Mouse sale
            OnHoverExit();
        }
    }

    // NUEVO: Animación al entrar el mouse
    private void OnHoverEnter()
    {
        if (image == null || isCompleted)
            return;

        isHovering = true;

        // Cancelar animaciones previas
        LeanTween.cancel(image.gameObject);

        // Escalar y elevar ligeramente
        LeanTween.scale(image.gameObject, originalScale * hoverScale, hoverDuration)
            .setEase(LeanTweenType.easeOutBack);

        // Pequeño movimiento hacia arriba
        Vector3 currentPos = image.transform.localPosition;
        LeanTween.moveLocalY(image.gameObject, currentPos.y + 0.1f, hoverDuration)
            .setEase(LeanTweenType.easeOutQuad);
    }

    // NUEVO: Animación al salir el mouse
    private void OnHoverExit()
    {
        if (image == null || isCompleted)
            return;

        isHovering = false;

        // Cancelar animaciones previas
        LeanTween.cancel(image.gameObject);

        // Volver a escala original
        LeanTween.scale(image.gameObject, originalScale, hoverDuration)
            .setEase(LeanTweenType.easeInOutQuad);

        // Volver a posición original
        Vector3 currentPos = image.transform.localPosition;
        LeanTween.moveLocalY(image.gameObject, currentPos.y - 0.1f, hoverDuration)
            .setEase(LeanTweenType.easeInOutQuad);

        // Reiniciar pulso idle si no está completado
        if (!isCompleted)
        {
            LeanTween.delayedCall(hoverDuration, () => {
                if (!isHovering && !isCompleted)
                    StartIdlePulse();
            });
        }
    }

    // NUEVO: Animación al hacer clic
    private void OnClickAnimation()
    {
        if (image == null)
            return;

        // Cancelar animaciones previas
        LeanTween.cancel(image.gameObject);
        isHovering = false;

        // Animación de "presionar"
        LeanTween.scale(image.gameObject, originalScale * 0.9f, 0.1f)
            .setEase(LeanTweenType.easeInQuad)
            .setOnComplete(() => {
                // Rebotar de vuelta
                LeanTween.scale(image.gameObject, originalScale, 0.2f)
                    .setEase(LeanTweenType.easeOutBack);
            });

        // Flash de color (opcional)
        if (image is SpriteRenderer sprite)
        {
            Color originalColor = sprite.color;
            LeanTween.value(image.gameObject, 0f, 1f, 0.3f)
                .setOnUpdate((float val) => {
                    sprite.color = Color.Lerp(Color.white, originalColor, val);
                });
        }
    }

    // NUEVO: Pulso suave cuando está idle (esperando ser clickeado)
    private void StartIdlePulse()
    {
        if (image == null || isCompleted || isHovering)
            return;

        // Cancelar animaciones previas
        LeanTween.cancel(image.gameObject);

        // Pulso suave y lento
        LeanTween.scale(image.gameObject, originalScale * pulseScale, pulseDuration)
            .setEase(LeanTweenType.easeInOutSine)
            .setLoopPingPong();
    }

    // NUEVO: Marcar como completado con animación
    public void MarkAsCompleted()
    {
        if (image == null || isCompleted)
            return;

        isCompleted = true;

        // Cancelar todas las animaciones
        LeanTween.cancel(image.gameObject);

        // Animación de desaparición suave
        LeanTween.scale(image.gameObject, Vector3.zero, 0.5f)
            .setEase(LeanTweenType.easeInBack);

        // Fade out
        if (image is SpriteRenderer sprite)
        {
            LeanTween.value(image.gameObject, sprite.color.a, 0f, 0.5f)
                .setOnUpdate((float alpha) => {
                    Color c = sprite.color;
                    c.a = alpha;
                    sprite.color = c;
                });
        }

        // Desactivar después de la animación
        LeanTween.delayedCall(0.5f, () => {
            if (image != null)
                image.gameObject.SetActive(false);
        });
    }

    // NUEVO: Animación de error/rechazo
    public void ShowErrorAnimation()
    {
        if (image == null)
            return;

        // Cancelar animaciones previas
        LeanTween.cancel(image.gameObject);

        // Shake horizontal
        Vector3 originalPos = image.transform.localPosition;

        LeanTween.moveLocalX(image.gameObject, originalPos.x + 0.1f, 0.05f)
            .setEase(LeanTweenType.easeInOutQuad)
            .setLoopPingPong()
            .setRepeat(4)
            .setOnComplete(() => {
                // Volver a posición original
                image.transform.localPosition = originalPos;

                // Reiniciar pulso si no está completado
                if (!isCompleted)
                    StartIdlePulse();
            });

        // Flash rojo
        if (image is SpriteRenderer sprite)
        {
            Color originalColor = sprite.color;
            LeanTween.value(image.gameObject, 0f, 1f, 0.5f)
                .setOnUpdate((float val) => {
                    sprite.color = Color.Lerp(Color.red, originalColor, val);
                });
        }
    }

    private void OnDisable()
    {
        // Cancelar todas las animaciones al desactivar
        if (image != null)
            LeanTween.cancel(image.gameObject);

        allowClicks = false;
        clickEnableTime = 0f;
    }

    private void OnDestroy()
    {
        // Cancelar animaciones al destruir
        if (image != null)
            LeanTween.cancel(image.gameObject);
    }
}