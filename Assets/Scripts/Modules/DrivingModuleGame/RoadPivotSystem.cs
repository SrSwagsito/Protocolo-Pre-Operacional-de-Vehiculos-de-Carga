using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Sistema de carretera con rango limitado en Y.
/// Las curvas solo se mueven dentro de un área definida (ej: mitad inferior de la pantalla)
/// </summary>
public class RoadPivotSystem : MonoBehaviour
{
    [Header("Scroll Settings")]
    [SerializeField] private float scrollSpeed = 3f;

    [Header("Lane Lines (Physical Boundaries)")]
    [SerializeField] private Transform roadContainer;
    [SerializeField] private Transform leftLaneLine;
    [SerializeField] private Transform rightLaneLine;
    [SerializeField] private SpriteRenderer roadSurface;
    [SerializeField] private float laneWidth = 3f;

    [Header("Y Position Limits - NUEVO")]
    [Tooltip("Posición Y mínima del centro de la carretera")]
    [SerializeField] private float minYPosition = -3f;

    [Tooltip("Posición Y máxima del centro de la carretera")]
    [SerializeField] private float maxYPosition = -1f;

    [Tooltip("Usar límites automáticos basados en la cámara")]
    [SerializeField] private bool useAutomaticLimits = true;

    [Tooltip("Si es automático: 0.5 = mitad inferior, 0.75 = cuarto inferior")]
    [Range(0.25f, 1f)]
    [SerializeField] private float screenCoverageBottom = 0.5f;

    [Header("Pivot System")]
    [SerializeField] private List<RoadPivot> pivots = new List<RoadPivot>();
    [SerializeField] private int pivotCount = 15;
    [SerializeField] private float pivotSpacing = 3f;

    [Header("Curve Settings")]
    [SerializeField] private float maxCurveOffset = 1f; // Reducido para que quepa en el rango
    [SerializeField] private float curveChangeInterval = 3f;
    [SerializeField] private float curveSpeed = 2f;

    [Header("Smooth Interpolation")]
    [SerializeField] private int interpolationPoints = 20;
    [Range(0f, 1f)]
    [SerializeField] private float smoothness = 0.5f;

    // Estado interno
    private float nextCurveChangeTime;
    private float targetCurveOffset = 0f;
    private List<Vector2> smoothPath = new List<Vector2>();
    private float centerYPosition; // Posición central del rango permitido

    private void Start()
    {
        CalculateYLimits();
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
    /// Calcula los límites Y automáticamente basándose en la cámara
    /// </summary>
    private void CalculateYLimits()
    {
        if (useAutomaticLimits)
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                float cameraHeight = cam.orthographicSize * 2f;
                float cameraBottom = -cam.orthographicSize;

                // Calcular rango basado en screenCoverageBottom
                // 0.5 = mitad inferior, 0.75 = tres cuartos inferiores, etc.
                float rangeHeight = cameraHeight * screenCoverageBottom;

                minYPosition = cameraBottom;
                maxYPosition = cameraBottom + rangeHeight;

                Debug.Log($"Límites automáticos: Min={minYPosition:F2}, Max={maxYPosition:F2}");
            }
        }

        // Calcular centro del rango
        centerYPosition = (minYPosition + maxYPosition) / 2f;
    }

    /// <summary>
    /// Crea pivotes dentro del rango permitido
    /// </summary>
    private void CreatePivots()
    {
        pivots.Clear();

        for (int i = 0; i < pivotCount; i++)
        {
            GameObject pivotGO = new GameObject($"Pivot_{i}");
            pivotGO.transform.parent = transform;

            float xPos = i * pivotSpacing;
            pivotGO.transform.position = new Vector3(xPos, 0, 0);

            RoadPivot pivot = pivotGO.AddComponent<RoadPivot>();
            pivot.offset = centerYPosition; // Empezar en el centro del rango

            pivots.Add(pivot);
        }
    }

    /// <summary>
    /// Actualiza las curvas respetando los límites Y
    /// </summary>
    private void UpdateCurves()
    {
        if (Time.time >= nextCurveChangeTime)
        {
            // Generar offset DENTRO del rango permitido
            float rangeSize = maxYPosition - minYPosition;
            float halfRange = rangeSize / 2f;

            // El offset ahora es relativo al centro, pero limitado
            float maxOffset = Mathf.Min(maxCurveOffset, halfRange);
            targetCurveOffset = Random.Range(-maxOffset, maxOffset);

            nextCurveChangeTime = Time.time + curveChangeInterval;
        }

        // Actualizar pivotes
        for (int i = 0; i < pivots.Count; i++)
        {
            if (pivots[i] == null) continue;

            if (i == pivots.Count - 1)
            {
                // Último pivote sigue el target
                float targetY = centerYPosition + targetCurveOffset;
                pivots[i].offset = Mathf.Lerp(
                    pivots[i].offset,
                    targetY,
                    Time.deltaTime * curveSpeed
                );
            }
            else
            {
                // Los demás siguen al siguiente
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

            // CLAMP: Asegurar que el pivote está dentro del rango
            pivots[i].offset = Mathf.Clamp(pivots[i].offset, minYPosition, maxYPosition);
        }
    }

    private void ScrollPivots()
    {
        foreach (var pivot in pivots)
        {
            if (pivot == null) continue;

            pivot.transform.Translate(Vector3.left * scrollSpeed * Time.deltaTime);

            if (pivot.transform.position.x < -pivotSpacing * 2)
            {
                RecyclePivot(pivot);
            }
        }
    }

    private void RecyclePivot(RoadPivot pivot)
    {
        float frontX = GetFrontPivotX();
        pivot.transform.position = new Vector3(frontX + pivotSpacing, 0, 0);

        RoadPivot nearestPivot = GetNearestPivotTo(frontX);
        if (nearestPivot != null)
        {
            pivot.offset = nearestPivot.offset;

            // Asegurar que está en rango
            pivot.offset = Mathf.Clamp(pivot.offset, minYPosition, maxYPosition);
        }
    }

    private void UpdateRoadVisuals()
    {
        if (roadContainer == null) return;

        CalculateSmoothPath();

        float truckX = 0f;
        float roadOffset = GetOffsetAtX(truckX);

        // CLAMP: Asegurar que la carretera está en el rango
        roadOffset = Mathf.Clamp(roadOffset, minYPosition, maxYPosition);

        Vector3 containerPos = roadContainer.position;
        containerPos.y = roadOffset;
        roadContainer.position = containerPos;

        // Actualizar líneas (posiciones locales relativas al container)
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

        // Actualizar superficie
        if (roadSurface != null)
        {
            Vector3 scale = roadSurface.transform.localScale;
            scale.y = laneWidth;
            roadSurface.transform.localScale = scale;

            Vector3 surfacePos = roadSurface.transform.localPosition;
            surfacePos.y = 0f;
            roadSurface.transform.localPosition = surfacePos;
        }
    }

    private void CalculateSmoothPath()
    {
        smoothPath.Clear();

        if (pivots.Count < 2) return;

        List<RoadPivot> sortedPivots = new List<RoadPivot>(pivots);
        sortedPivots.Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x));

        for (int i = 0; i < sortedPivots.Count - 1; i++)
        {
            Vector2 p0 = new Vector2(sortedPivots[i].transform.position.x, sortedPivots[i].offset);
            Vector2 p1 = new Vector2(sortedPivots[i + 1].transform.position.x, sortedPivots[i + 1].offset);

            Vector2 p_1 = i > 0 ?
                new Vector2(sortedPivots[i - 1].transform.position.x, sortedPivots[i - 1].offset) :
                p0;

            Vector2 p2 = i < sortedPivots.Count - 2 ?
                new Vector2(sortedPivots[i + 2].transform.position.x, sortedPivots[i + 2].offset) :
                p1;

            for (int j = 0; j < interpolationPoints; j++)
            {
                float t = j / (float)interpolationPoints;
                Vector2 point = CatmullRomInterpolate(p_1, p0, p1, p2, t);

                // CLAMP: Asegurar que el punto interpolado está en rango
                point.y = Mathf.Clamp(point.y, minYPosition, maxYPosition);

                smoothPath.Add(point);
            }
        }
    }

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

        result.y = Mathf.Lerp(p1.y, result.y, smoothness);

        return result;
    }

    public float GetOffsetAtX(float x)
    {
        if (smoothPath.Count == 0) return centerYPosition;

        float closestDistance = float.MaxValue;
        float closestOffset = centerYPosition;

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
            // Mantener dentro del rango
            float rangeSize = maxYPosition - minYPosition;
            maxCurveOffset = Mathf.Min(maxCurveOffset * 1.5f, rangeSize / 2f);

            curveChangeInterval *= 0.7f;
            scrollSpeed *= 1.2f;
        }
    }

    /// <summary>
    /// Dibuja los límites del área de la carretera
    /// </summary>
    private void OnDrawGizmos()
    {
        // Dibujar límites del rango Y
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(-10, minYPosition, 0), new Vector3(10, minYPosition, 0));
        Gizmos.DrawLine(new Vector3(-10, maxYPosition, 0), new Vector3(10, maxYPosition, 0));

        // Dibujar centro
        Gizmos.color = Color.yellow;
        float center = (minYPosition + maxYPosition) / 2f;
        Gizmos.DrawLine(new Vector3(-10, center, 0), new Vector3(10, center, 0));

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