using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controlador del camión con:
/// - Control de habilitación de input
/// - Soporte para botones móviles
/// - Sistema de pausa completo
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class TruckPhysicalController : MonoBehaviour
{
    public Action<int> OnCollectibleHit;
    public Action<int> OnObstacleHit;
    public Action<int> OnBoundaryPenalty;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float verticalLimit = 4f;

    [Header("Physics")]
    [SerializeField] private float mass = 1f;
    [SerializeField] private float friction = 0.4f;

    [Header("Penalty")]
    [SerializeField] private int boundaryPenalty = 2;

    [Header("Visual Feedback")]
    [SerializeField] private SpriteRenderer truckSprite;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warningColor = Color.yellow;


    [Header("Mobile Button Control - NUEVO")]
    [Tooltip("Fuerza aplicada al presionar botones móviles")]
    [SerializeField] private float buttonMoveForce = 300f;

    // Referencias
    private Rigidbody2D rb;
    private bool isTouchingBoundary = false;

    // NUEVO: Control de input
    private bool inputEnabled = false;
    private bool isPressingUp = false;
    private bool isPressingDown = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.mass = mass;
        rb.linearDamping = 0.5f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        if (!gameObject.CompareTag("Player") && !gameObject.CompareTag("Truck"))
        {
            gameObject.tag = "Player";
        }
    }

    private void OnEnable()
    {
        PhysicalLaneBoundary.OnTruckHitBoundary += OnHitBoundary;
    }

    private void OnDisable()
    {
        PhysicalLaneBoundary.OnTruckHitBoundary -= OnHitBoundary;
    }

    private void Update()
    {
        // Solo procesar input si está habilitado
        if (inputEnabled)
        {
            HandleInput();
        }

        ClampPosition();
    }

    private void FixedUpdate()
    {
        // Aplicar fuerza de botones móviles en FixedUpdate
        if (inputEnabled)
        {
            ApplyButtonForces();
        }
    }

    /// <summary>
    /// Habilita o deshabilita el input del jugador
    /// </summary>
    public void SetInputEnabled(bool enabled)
    {
        inputEnabled = enabled;

        if (!enabled)
        {
            // Al deshabilitar, detener el camión
            rb.linearVelocity = Vector2.zero;
            isPressingUp = false;
            isPressingDown = false;
        }
    }

    private void HandleInput()
    {
        float inputY = 0;

        if (Keyboard.current != null) 
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
            {
                inputY = 1f;
            }
            else if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
            {
                inputY = -1f;
            }
        }

        // Control táctil (arrastrar)
#if UNITY_ANDROID || UNITY_IOS
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector3 touchWorldPos = Camera.main.ScreenToWorldPoint(touch.position);
            float targetY = touchWorldPos.y;
            float currentY = transform.position.y;

            if (Mathf.Abs(targetY - currentY) > 0.2f)
            {
                inputY = Mathf.Sign(targetY - currentY);
            }
        }
#endif

        // Aplicar fuerza
        if (inputY != 0f)
        {
            Vector2 force = new Vector2(0, inputY * moveSpeed);
            rb.AddForce(force);
        }
    }

    /// <summary>
    /// Aplica fuerzas desde los botones móviles
    /// </summary>
    private void ApplyButtonForces()
    {
        if (isPressingUp)
        {
            rb.AddForce(Vector2.up * buttonMoveForce * Time.fixedDeltaTime);
        }

        if (isPressingDown)
        {
            rb.AddForce(Vector2.down * buttonMoveForce * Time.fixedDeltaTime);
        }
    }

    /// <summary>
    /// Llamado por el botón UP (móvil)
    /// </summary>
    public void MoveUp()
    {
        if (!inputEnabled) return;
        isPressingUp = true;
        Invoke(nameof(StopMoveUp), 0.1f);
    }

    /// <summary>
    /// Llamado por el botón DOWN (móvil)
    /// </summary>
    public void MoveDown()
    {
        if (!inputEnabled) return;
        isPressingDown = true;
        Invoke(nameof(StopMoveDown), 0.1f);
    }

    private void StopMoveUp()
    {
        isPressingUp = false;
    }

    private void StopMoveDown()
    {
        isPressingDown = false;
    }

    private void ClampPosition()
    {
        Vector3 pos = transform.position;

        if (pos.y < -verticalLimit)
        {
            pos.y = -verticalLimit;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        }
        else if (pos.y > verticalLimit)
        {
            pos.y = verticalLimit;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        }

        transform.position = pos;
    }

    private void OnHitBoundary()
    {
        if (truckSprite != null && !isTouchingBoundary)
        {
            truckSprite.color = warningColor;
        }

        isTouchingBoundary = true;
        OnBoundaryPenalty?.Invoke(boundaryPenalty);

        Invoke(nameof(ResetColor), 0.3f);
    }

    private void ResetColor()
    {
        isTouchingBoundary = false;

        if (truckSprite != null)
        {
            truckSprite.color = normalColor;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Collectible"))
        {
            OnCollectibleHit?.Invoke(10);
            Destroy(collision.gameObject);
            AudioManager.Instance.Play("Truck collect");

            if (truckSprite != null)
            {
                StartCoroutine(FlashColor(Color.green, 0.2f));
            }
        }
        else if (collision.CompareTag("Obstacle"))
        {
            OnObstacleHit?.Invoke(5);
            Destroy(collision.gameObject);
            AudioManager.Instance.Play("Truck hit");

            if (truckSprite != null)
            {
                StartCoroutine(FlashColor(Color.red, 0.3f));
            }
        }
    }

    private System.Collections.IEnumerator FlashColor(Color flashColor, float duration)
    {
        if (truckSprite == null)
            yield break;

        Color original = truckSprite.color;
        truckSprite.color = flashColor;
        yield return new WaitForSeconds(duration);
        truckSprite.color = isTouchingBoundary ? warningColor : normalColor;
    }

    public void ResetState()
    {
        rb.linearVelocity = Vector2.zero;
        isTouchingBoundary = false;
        isPressingUp = false;
        isPressingDown = false;

        if (truckSprite != null)
        {
            truckSprite.color = normalColor;
        }
    }
}