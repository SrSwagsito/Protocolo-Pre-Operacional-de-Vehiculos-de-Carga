using UnityEngine;
using System;

/// <summary>
/// Línea lateral del carril con collider físico.
/// Empuja al camión y activa eventos cuando lo toca.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class PhysicalLaneBoundary : MonoBehaviour
{
    public static Action OnTruckHitBoundary;
    
    [Header("Configuración")]
    [Tooltip("¿Es el borde izquierdo?")]
    [SerializeField] private bool isLeftBoundary = true;
    
    [Tooltip("Puntos que resta por segundo al tocar")]
    [SerializeField] private int penaltyPerSecond = 2;
    
    [Header("Visual")]
    [Tooltip("Color de la línea")]
    [SerializeField] private Color lineColor = Color.white;
    
    [Tooltip("SpriteRenderer de la línea")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    // Referencias
    private BoxCollider2D boxCollider;
    private GameObject currentTruck = null;
    private float lastPenaltyTime = 0f;
    private float penaltyInterval = 0.5f;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        
        // Configurar collider
        boxCollider.isTrigger = false; // NO es trigger, es físico
        
        // Si tiene sprite, aplicar color
        if (spriteRenderer != null)
        {
            spriteRenderer.color = lineColor;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") || 
            collision.gameObject.CompareTag("Truck"))
        {
            currentTruck = collision.gameObject;
            Debug.Log($"Camión tocó borde {(isLeftBoundary ? "izquierdo" : "derecho")}");
            
            // Emitir evento inmediatamente
            OnTruckHitBoundary?.Invoke();
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") || 
            collision.gameObject.CompareTag("Truck"))
        {
            // Aplicar penalización periódica mientras está tocando
            if (Time.time - lastPenaltyTime >= penaltyInterval)
            {
                OnTruckHitBoundary?.Invoke();
                lastPenaltyTime = Time.time;
                
                Debug.Log("Penalización por tocar borde");
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject == currentTruck)
        {
            currentTruck = null;
            Debug.Log("Camión dejó de tocar el borde");
        }
    }

    /// <summary>
    /// Verifica si el camión está tocando este borde actualmente
    /// </summary>
    public bool IsTouchingTruck()
    {
        return currentTruck != null;
    }
}
