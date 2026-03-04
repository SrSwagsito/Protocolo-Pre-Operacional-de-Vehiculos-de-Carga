using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Sistema de carretera con pivotes que mueven las líneas laterales.
/// Las líneas son físicas (colliders) que empujan al camión y restan puntos.
/// </summary>
public class RoadPivotSystem : MonoBehaviour
{
    [Header("Scroll Settings")]
    [SerializeField] private float scrollSpeed = 3f;
    
    [Header("Lane Lines (Physical Boundaries)")]
    [Tooltip("Línea izquierda del carril con collider")]
    [SerializeField] private Transform leftLaneLine;
    
    [Tooltip("Línea derecha del carril con collider")]
    [SerializeField] private Transform rightLaneLine;
    
    [Tooltip("Ancho del carril (distancia entre líneas)")]
    [SerializeField] private float laneWidth = 3f;
    
    [Header("Pivot System")]
    [Tooltip("Lista de pivotes que definen la forma del camino")]
    [SerializeField] private List<RoadPivot> pivots = new List<RoadPivot>();
    
    [Tooltip("Cuántos pivotes crear automáticamente")]
    [SerializeField] private int autoPivotCount = 10;
    
    [Tooltip("Distancia entre pivotes")]
    [SerializeField] private float pivotSpacing = 5f;
    
    [Header("Curve Settings")]
    [Tooltip("Qué tan pronunciadas pueden ser las curvas")]
    [SerializeField] private float maxCurveOffset = 2f;
    
    [Tooltip("Cada cuánto tiempo cambia la curva objetivo")]
    [SerializeField] private float curveChangeInterval = 3f;
    
    [Tooltip("Velocidad de interpolación entre curvas")]
    [SerializeField] private float curveSpeed = 1f;
    
    // Estado interno
    private float nextCurveChangeTime;
    private float targetCurveOffset = 0f;
    
    private void Start()
    {
        // Crear pivotes automáticamente si no hay
        if (pivots.Count == 0)
        {
            CreatePivots();
        }
        
        nextCurveChangeTime = Time.time + curveChangeInterval;
        UpdateLaneLines();
    }

    private void Update()
    {
        // Actualizar curvas
        UpdateCurves();
        
        // Mover pivotes hacia adelante (scroll)
        ScrollPivots();
        
        // Actualizar posición de las líneas según los pivotes
        UpdateLaneLines();
    }

    /// <summary>
    /// Crea pivotes automáticamente distribuidos en el camino
    /// </summary>
    private void CreatePivots()
    {
        for (int i = 0; i < autoPivotCount; i++)
        {
            GameObject pivotGO = new GameObject($"Pivot_{i}");
            pivotGO.transform.parent = transform;
            pivotGO.transform.position = new Vector3(i * pivotSpacing, 0, 0);
            
            RoadPivot pivot = pivotGO.AddComponent<RoadPivot>();
            pivot.offset = 0f;
            
            pivots.Add(pivot);
        }
    }

    /// <summary>
    /// Actualiza las curvas de los pivotes
    /// </summary>
    private void UpdateCurves()
    {
        // Cambiar objetivo de curva periódicamente
        if (Time.time >= nextCurveChangeTime)
        {
            targetCurveOffset = Random.Range(-maxCurveOffset, maxCurveOffset);
            nextCurveChangeTime = Time.time + curveChangeInterval;
        }
        
        // Actualizar el pivote más adelante con la nueva curva
        if (pivots.Count > 0)
        {
            // Encontrar el pivote más adelante (mayor X)
            RoadPivot frontPivot = GetFrontPivot();
            if (frontPivot != null)
            {
                // Interpolar suavemente hacia el objetivo
                frontPivot.offset = Mathf.Lerp(
                    frontPivot.offset,
                    targetCurveOffset,
                    Time.deltaTime * curveSpeed
                );
            }
        }
    }

    /// <summary>
    /// Mueve los pivotes hacia atrás (simulando scroll)
    /// </summary>
    private void ScrollPivots()
    {
        foreach (var pivot in pivots)
        {
            if (pivot == null) continue;
            
            // Mover hacia la izquierda
            pivot.transform.Translate(Vector3.left * scrollSpeed * Time.deltaTime);
            
            // Si sale de pantalla, reciclarlo al frente
            if (pivot.transform.position.x < -pivotSpacing)
            {
                RecyclePivot(pivot);
            }
        }
    }

    /// <summary>
    /// Recicla un pivote que salió de pantalla, poniéndolo al frente
    /// </summary>
    private void RecyclePivot(RoadPivot pivot)
    {
        // Encontrar el pivote más adelante
        RoadPivot frontPivot = GetFrontPivot();
        
        if (frontPivot != null)
        {
            // Posicionar delante del pivote más adelante
            float newX = frontPivot.transform.position.x + pivotSpacing;
            pivot.transform.position = new Vector3(newX, 0, 0);
            
            // Heredar el offset del pivote anterior para continuidad
            pivot.offset = frontPivot.offset;
        }
    }

    /// <summary>
    /// Actualiza la posición de las líneas laterales según los pivotes
    /// </summary>
    private void UpdateLaneLines()
    {
        if (leftLaneLine == null || rightLaneLine == null)
            return;
        
        // Calcular offset promedio de los pivotes visibles
        float averageOffset = GetAverageVisibleOffset();
        
        // Posicionar líneas
        Vector3 leftPos = leftLaneLine.position;
        leftPos.y = averageOffset - (laneWidth / 2f);
        leftLaneLine.position = leftPos;
        
        Vector3 rightPos = rightLaneLine.position;
        rightPos.y = averageOffset + (laneWidth / 2f);
        rightLaneLine.position = rightPos;
    }

    /// <summary>
    /// Obtiene el offset promedio de los pivotes visibles en pantalla
    /// </summary>
    private float GetAverageVisibleOffset()
    {
        float sum = 0f;
        int count = 0;
        
        foreach (var pivot in pivots)
        {
            if (pivot == null) continue;
            
            // Solo considerar pivotes en pantalla
            if (pivot.transform.position.x > -10 && pivot.transform.position.x < 10)
            {
                sum += pivot.offset;
                count++;
            }
        }
        
        return count > 0 ? sum / count : 0f;
    }

    /// <summary>
    /// Obtiene el pivote que está más adelante (mayor X)
    /// </summary>
    private RoadPivot GetFrontPivot()
    {
        RoadPivot frontPivot = null;
        float maxX = float.MinValue;
        
        foreach (var pivot in pivots)
        {
            if (pivot == null) continue;
            
            if (pivot.transform.position.x > maxX)
            {
                maxX = pivot.transform.position.x;
                frontPivot = pivot;
            }
        }
        
        return frontPivot;
    }

    /// <summary>
    /// Ajusta dificultad del camino
    /// </summary>
    public void SetDifficulty(bool challengeMode)
    {
        if (challengeMode)
        {
            maxCurveOffset *= 1.5f;
            curveChangeInterval *= 0.7f;
            scrollSpeed *= 1.2f;
        }
    }

    /// <summary>
    /// Debug visual de los pivotes
    /// </summary>
    private void OnDrawGizmos()
    {
        if (pivots == null || pivots.Count == 0)
            return;
        
        // Dibujar línea conectando pivotes
        Gizmos.color = Color.green;
        for (int i = 0; i < pivots.Count - 1; i++)
        {
            if (pivots[i] == null || pivots[i + 1] == null)
                continue;
            
            Vector3 p1 = new Vector3(
                pivots[i].transform.position.x,
                pivots[i].offset,
                0
            );
            
            Vector3 p2 = new Vector3(
                pivots[i + 1].transform.position.x,
                pivots[i + 1].offset,
                0
            );
            
            Gizmos.DrawLine(p1, p2);
        }
    }
}
