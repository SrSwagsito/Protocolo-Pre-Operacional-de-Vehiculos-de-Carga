using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Sistema de carretera mejorado con interpolación suave entre pivotes
/// y superficie visual de carretera.
/// </summary>
public class RoadPivotSystem : MonoBehaviour
{
    [Header("Scroll Settings")]
    [SerializeField] private float scrollSpeed = 3f;

    [Header("Lane Lines (Physical Boundaries)")]
    [Tooltip("Contenedor con ambas líneas y la carretera visual")]
    [SerializeField] private Transform roadContainer;

    [Tooltip("Línea izquierda del carril")]
    [SerializeField] private Transform leftLaneLine;

    [Tooltip("Línea derecha del carril")]
    [SerializeField] private Transform rightLaneLine;

    [Tooltip("Sprite de la carretera (entre las líneas)")]
    [SerializeField] private SpriteRenderer roadSurface;

    [Tooltip("Ancho del carril")]
    [SerializeField] private float laneWidth = 3f;

    [Header("Pivot System")]
    [Tooltip("Lista de pivotes")]
    [SerializeField] private List<RoadPivot> pivots = new List<RoadPivot>();

    [Tooltip("Cuántos pivotes crear")]
    [SerializeField] private int pivotCount = 15;

    [Tooltip("Distancia entre pivotes")]
    [SerializeField] private float pivotSpacing = 3f;

    [Header("Curve Settings")]
    [Tooltip("Qué tan pronunciadas son las curvas")]
    [SerializeField] private float maxCurveOffset = 2f;

    [Tooltip("Cada cuánto cambia la curva")]
    [SerializeField] private float curveChangeInterval = 3f;

    [Tooltip("Velocidad de transición entre curvas")]
    [SerializeField] private float curveSpeed = 2f;

    [Header("Smooth Interpolation")]
    [Tooltip("Número de puntos para interpolar entre pivotes")]
    [SerializeField] private int interpolationPoints = 20;

    [Tooltip("Suavizado adicional (0-1)")]
    [Range(0f, 1f)]
    [SerializeField] private float smoothness = 0.5f;

    // Estado interno
    private float nextCurveChangeTime;
    private float targetCurveOffset = 0f;
    private List<Vector2> smoothPath = new List<Vector2>();

    private void Start()
    {
        CreatePivots();
        nextCurveChangeTime = Time.time + curveChangeInterval;
        UpdateRoadVisuals();
    }

    private void Update()
    {
        UpdateCurves();
        ScrollPivots();
        UpdateRoadVisuals();
    }

    /// <summary>
    /// Crea pivotes automáticamente
    /// </summary>
    private void CreatePivots()
    {
        pivots.Clear();

        for (int i = 0; i < pivotCount; i++)
        {
            GameObject pivotGO = new GameObject($"Pivot_{i}");
            pivotGO.transform.parent = transform;

            // Posición inicial
            float xPos = i * pivotSpacing;
            pivotGO.transform.position = new Vector3(xPos, 0, 0);

            RoadPivot pivot = pivotGO.AddComponent<RoadPivot>();
            pivot.offset = 0f;

            pivots.Add(pivot);
        }
    }

    /// <summary>
    /// Actualiza las curvas de forma gradual
    /// </summary>
    private void UpdateCurves()
    {
        // Cambiar objetivo cada intervalo
        if (Time.time >= nextCurveChangeTime)
        {
            targetCurveOffset = Random.Range(-maxCurveOffset, maxCurveOffset);
            nextCurveChangeTime = Time.time + curveChangeInterval;
        }

        // Actualizar cada pivote gradualmente hacia el objetivo
        for (int i = 0; i < pivots.Count; i++)
        {
            if (pivots[i] == null) continue;

            // Los pivotes más adelante cambian primero
            float distanceFromFront = GetFrontPivotX() - pivots[i].transform.position.x;
            float delay = distanceFromFront * 0.1f; // Más atrás = más delay

            // Interpolar hacia el objetivo con delay
            float targetForThisPivot = i == pivots.Count - 1 ? targetCurveOffset : pivots[i].offset;

            if (i == pivots.Count - 1)
            {
                // El último pivote (más adelante) sigue el target
                pivots[i].offset = Mathf.Lerp(
                    pivots[i].offset,
                    targetCurveOffset,
                    Time.deltaTime * curveSpeed
                );
            }
            else
            {
                // Los demás siguen al que está adelante
                if (i < pivots.Count - 1)
                {
                    float nextOffset = pivots[i + 1].offset;
                    pivots[i].offset = Mathf.Lerp(
                        pivots[i].offset,
                        nextOffset,
                        Time.deltaTime * curveSpeed
                    );
                }
            }
        }
    }

    /// <summary>
    /// Mueve los pivotes (scroll)
    /// </summary>
    private void ScrollPivots()
    {
        foreach (var pivot in pivots)
        {
            if (pivot == null) continue;

            pivot.transform.Translate(Vector3.left * scrollSpeed * Time.deltaTime);

            // Reciclar si sale de pantalla
            if (pivot.transform.position.x < -pivotSpacing * 2)
            {
                RecyclePivot(pivot);
            }
        }
    }

    /// <summary>
    /// Recicla un pivote al frente
    /// </summary>
    private void RecyclePivot(RoadPivot pivot)
    {
        float frontX = GetFrontPivotX();
        pivot.transform.position = new Vector3(frontX + pivotSpacing, 0, 0);

        // Heredar offset del pivote anterior para continuidad
        RoadPivot nearestPivot = GetNearestPivotTo(frontX);
        if (nearestPivot != null)
        {
            pivot.offset = nearestPivot.offset;
        }
    }

    /// <summary>
    /// Actualiza visuales de la carretera con interpolación suave
    /// </summary>
    private void UpdateRoadVisuals()
    {
        if (roadContainer == null) return;

        // Calcular path suave interpolando entre pivotes
        CalculateSmoothPath();

        // Obtener offset en la posición del camión (centro de pantalla)
        float truckX = 0f; // Asumiendo que el camión está en X=0
        float roadOffset = GetOffsetAtX(truckX);

        // Mover el contenedor de la carretera
        Vector3 containerPos = roadContainer.position;
        containerPos.y = roadOffset;
        roadContainer.position = containerPos;

        // Actualizar líneas laterales
        if (leftLaneLine != null)
        {
            Vector3 leftPos = leftLaneLine.localPosition;
            leftPos.y = -(laneWidth / 2f);
            leftLaneLine.localPosition = leftPos;
        }

        if (rightLaneLine != null)
        {
            Vector3 rightPos = rightLaneLine.localPosition;
            rightPos.y = (laneWidth / 2f);
            rightLaneLine.localPosition = rightPos;
        }

        // Actualizar tamaño de la superficie de carretera
        if (roadSurface != null)
        {
            Vector3 scale = roadSurface.transform.localScale;
            scale.y = laneWidth;
            roadSurface.transform.localScale = scale;

            // Centrar
            Vector3 surfacePos = roadSurface.transform.localPosition;
            surfacePos.y = 0f;
            roadSurface.transform.localPosition = surfacePos;
        }
    }

    /// <summary>
    /// Calcula un path suave interpolando entre pivotes
    /// </summary>
    private void CalculateSmoothPath()
    {
        smoothPath.Clear();

        if (pivots.Count < 2) return;

        // Ordenar pivotes por posición X
        List<RoadPivot> sortedPivots = new List<RoadPivot>(pivots);
        sortedPivots.Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x));

        // Interpolar entre cada par de pivotes
        for (int i = 0; i < sortedPivots.Count - 1; i++)
        {
            Vector2 p0 = new Vector2(sortedPivots[i].transform.position.x, sortedPivots[i].offset);
            Vector2 p1 = new Vector2(sortedPivots[i + 1].transform.position.x, sortedPivots[i + 1].offset);

            // Puntos de control para curva suave (Catmull-Rom)
            Vector2 p_1 = i > 0 ?
                new Vector2(sortedPivots[i - 1].transform.position.x, sortedPivots[i - 1].offset) :
                p0;

            Vector2 p2 = i < sortedPivots.Count - 2 ?
                new Vector2(sortedPivots[i + 2].transform.position.x, sortedPivots[i + 2].offset) :
                p1;

            // Interpolar puntos
            for (int j = 0; j < interpolationPoints; j++)
            {
                float t = j / (float)interpolationPoints;
                Vector2 point = CatmullRomInterpolate(p_1, p0, p1, p2, t);
                smoothPath.Add(point);
            }
        }
    }

    /// <summary>
    /// Interpolación Catmull-Rom para curvas suaves
    /// </summary>
    private Vector2 CatmullRomInterpolate(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        Vector2 result = 0.5f * (
            (2f * p1) +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t3
        );

        // Aplicar smoothness
        result.y = Mathf.Lerp(p1.y, result.y, smoothness);

        return result;
    }

    /// <summary>
    /// Obtiene el offset de la carretera en una posición X específica
    /// </summary>
    public float GetOffsetAtX(float x)
    {
        if (smoothPath.Count == 0) return 0f;

        // Encontrar el punto más cercano en el path
        float closestDistance = float.MaxValue;
        float closestOffset = 0f;

        foreach (var point in smoothPath)
        {
            float distance = Mathf.Abs(point.x - x);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestOffset = point.y;
            }
        }

        return closestOffset;
    }

    /// <summary>
    /// Obtiene la posición X del pivote más adelante
    /// </summary>
    private float GetFrontPivotX()
    {
        float maxX = float.MinValue;
        foreach (var pivot in pivots)
        {
            if (pivot != null && pivot.transform.position.x > maxX)
            {
                maxX = pivot.transform.position.x;
            }
        }
        return maxX;
    }

    /// <summary>
    /// Obtiene el pivote más cercano a una posición X
    /// </summary>
    private RoadPivot GetNearestPivotTo(float x)
    {
        RoadPivot nearest = null;
        float minDistance = float.MaxValue;

        foreach (var pivot in pivots)
        {
            if (pivot == null) continue;

            float distance = Mathf.Abs(pivot.transform.position.x - x);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = pivot;
            }
        }

        return nearest;
    }

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
    /// Debug: Dibuja el path suave
    /// </summary>
    private void OnDrawGizmos()
    {
        // Dibujar pivotes
        if (pivots != null)
        {
            Gizmos.color = Color.cyan;
            foreach (var pivot in pivots)
            {
                if (pivot == null) continue;
                Vector3 pos = new Vector3(pivot.transform.position.x, pivot.offset, 0);
                Gizmos.DrawWireSphere(pos, 0.2f);
            }
        }

        // Dibujar path suave
        if (smoothPath != null && smoothPath.Count > 1)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < smoothPath.Count - 1; i++)
            {
                Vector3 p1 = new Vector3(smoothPath[i].x, smoothPath[i].y, 0);
                Vector3 p2 = new Vector3(smoothPath[i + 1].x, smoothPath[i + 1].y, 0);
                Gizmos.DrawLine(p1, p2);
            }
        }
    }
}