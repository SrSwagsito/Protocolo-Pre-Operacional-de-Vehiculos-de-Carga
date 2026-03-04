using System;
using UnityEngine;

/// <summary>
/// Controlador del camión con física para interactuar con las líneas del carril.
/// Las líneas lo empujan físicamente.
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
    [Tooltip("Masa del camión")]
    [SerializeField] private float mass = 1f;
    
    [Tooltip("Fricción con las líneas")]
    [SerializeField] private float friction = 0.4f;
    
    [Header("Penalty")]
    [Tooltip("Puntos que pierde al tocar bordes")]
    [SerializeField] private int boundaryPenalty = 2;
    
    [Header("Visual Feedback")]
    [SerializeField] private SpriteRenderer truckSprite;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warningColor = Color.yellow;
    
    // Referencias
    private Rigidbody2D rb;
    private bool isTouchingBoundary = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Configurar Rigidbody2D
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.mass = mass;
        rb.linearDamping = 0.5f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        
        // Tag
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
        HandleInput();
        ClampPosition();
    }

    private void HandleInput()
    {
        float inputY = Input.GetAxis("Vertical");
        
        // Control táctil
        #if UNITY_ANDROID || UNITY_IOS || UNITY_EDITOR
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
        
        // Aplicar fuerza en vez de mover directamente
        if (inputY != 0f)
        {
            Vector2 force = new Vector2(0, inputY * moveSpeed);
            rb.AddForce(force);
        }
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
        // Cambiar a color de advertencia
        if (truckSprite != null && !isTouchingBoundary)
        {
            truckSprite.color = warningColor;
        }
        
        isTouchingBoundary = true;
        
        // Aplicar penalización
        OnBoundaryPenalty?.Invoke(boundaryPenalty);
        
        // Volver a color normal después de un momento
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
            
            if (truckSprite != null)
            {
                StartCoroutine(FlashColor(Color.green, 0.2f));
            }
        }
        else if (collision.CompareTag("Obstacle"))
        {
            OnObstacleHit?.Invoke(5);
            Destroy(collision.gameObject);
            
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
        
        if (truckSprite != null)
        {
            truckSprite.color = normalColor;
        }
    }
}
