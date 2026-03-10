using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Sistema de carretera con control de pausa.
/// Se puede pausar completamente antes de iniciar el juego.
/// </summary>
public class RoadPivotSystem: MonoBehaviour
{
    [Header("Scroll Settings")]
    [SerializeField] private float scrollSpeed = 3f;

    [Header("Lane Lines (Physical Boundaries)")]
    [SerializeField] private Transform roadContainer;
    [SerializeField] private Transform leftLaneLine;
    [SerializeField] private Transform rightLaneLine;
    [SerializeField] private SpriteRenderer roadSurface;
    [SerializeField] private float laneWidth = 3f;

    [Header("Y Position Limits")]
    [SerializeField] private float minYPosition = -3f;
    [SerializeField] private float maxYPosition = -1f;
    [SerializeField] private bool useAutomaticLimits = true;
    [Range(0.25f, 1f)]
    [SerializeField] private float screenCoverageBottom = 0.5f;

    [Header("Pivot System")]
    [SerializeField] private List<RoadPivot> pivots = new List<RoadPivot>();
    [SerializeField] private int pivotCount = 15;
    [SerializeField] private float pivotSpacing = 3f;

    [Header("Curve Settings")]
    [SerializeField] private float maxCurveOffset = 1f;
    [SerializeField] private float curveChangeInterval = 3f;
    [SerializeField] private float curveSpeed = 2f;

    [Header("Smooth Interpolation")]
    [SerializeField] private int interpolationPoints = 20;
    [Range(0f, 1f)]
    [SerializeField] private float smoothness = 0.5f;

    [Header("Control - NUEVO")]
    [Tooltip("Si está en false, la carretera no se actualiza")]
    [SerializeField] private bool isActive = false;

    // Estado interno
    private float nextCurveChangeTime;
    private float targetCurveOffset = 0f;
    private List<Vector2> smoothPath = new List<Vector2>();
    private float centerYPosition;

    // Propiedades públicas
    public bool IsActive => isActive;
    public float CurrentRoadY => roadContainer != null ? roadContainer.position.y : centerYPosition;
    public float LaneWidth => laneWidth;
    public float MinY => centerYPosition - (laneWidth / 2f);
    public float MaxY => centerYPosition + (laneWidth / 2f);

    private void Start()
    {
        CalculateYLimits();
        CreatePivots();
        nextCurveChangeTime = Time.time + curveChangeInterval;

        // Actualizar visuals una vez para posición inicial
        UpdateRoadVisuals();
    }

    private void Update()
    {
        // Solo actualizar si está activo
        if (!isActive) return;

        UpdateCurves();
        ScrollPivots();
        UpdateRoadVisuals();
    }

    /// <summary>
    /// Activa o pausa el sistema de carretera
    /// </summary>
    public void SetActive(bool active)
    {
        isActive = active;

        if (active)
        {
            // Reiniciar timer de curvas
            nextCurveChangeTime = Time.time + curveChangeInterval;
        }
    }

    /// <summary>
    /// Reinicia el sistema (útil para retry)
    /// </summary>
    public void ResetSystem()
    {
        // Resetear pivotes a posición inicial
        for (int i = 0; i < pivots.Count; i++)
        {
            if (pivots[i] == null) continue;

            float xPos = i * pivotSpacing;
            pivots[i].transform.position = new Vector3(xPos, 0, 0);
            pivots[i].offset = centerYPosition;
        }

        targetCurveOffset = 0f;
        UpdateRoadVisuals();
    }

    private void CalculateYLimits()
    {
        if (useAutomaticLimits)
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                float cameraHeight = cam.orthographicSize * 2f;
                float cameraBottom = -cam.orthographicSize;
                float rangeHeight = cameraHeight * screenCoverageBottom;

                minYPosition = cameraBottom;
                maxYPosition = cameraBottom + rangeHeight;
            }
        }

        centerYPosition = (minYPosition + maxYPosition) / 2f;
    }

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
            pivot.offset = centerYPosition;

            pivots.Add(pivot);
        }
    }

    private void UpdateCurves()
    {
        if (Time.time >= nextCurveChangeTime)
        {
            float rangeSize = maxYPosition - minYPosition;
            float halfRange = rangeSize / 2f;
            float maxOffset = Mathf.Min(maxCurveOffset, halfRange);
            targetCurveOffset = Random.Range(-maxOffset, maxOffset);

            nextCurveChangeTime = Time.time + curveChangeInterval;
        }

        for (int i = 0; i < pivots.Count; i++)
        {
            if (pivots[i] == null) continue;

            if (i == pivots.Count - 1)
            {
                float targetY = centerYPosition + targetCurveOffset;
                pivots[i].offset = Mathf.Lerp(
                    pivots[i].offset,
                    targetY,
                    Time.deltaTime * curveSpeed
                );
            }
            else
            {
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
            pivot.offset = Mathf.Clamp(pivot.offset, minYPosition, maxYPosition);
        }
    }

    private void UpdateRoadVisuals()
    {
        if (roadContainer == null) return;

        CalculateSmoothPath();

        float truckX = 0f;
        float roadOffset = GetOffsetAtX(truckX);
        roadOffset = Mathf.Clamp(roadOffset, minYPosition, maxYPosition);

        Vector3 containerPos = roadContainer.position;
        containerPos.y = roadOffset;
        roadContainer.position = containerPos;

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
            float rangeSize = maxYPosition - minYPosition;
            maxCurveOffset = Mathf.Min(maxCurveOffset * 1.5f, rangeSize / 2f);

            curveChangeInterval *= 0.7f;
            scrollSpeed *= 1.2f;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(-10, minYPosition, 0), new Vector3(10, minYPosition, 0));
        Gizmos.DrawLine(new Vector3(-10, maxYPosition, 0), new Vector3(10, maxYPosition, 0));

        Gizmos.color = Color.yellow;
        float center = (minYPosition + maxYPosition) / 2f;
        Gizmos.DrawLine(new Vector3(-10, center, 0), new Vector3(10, center, 0));

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