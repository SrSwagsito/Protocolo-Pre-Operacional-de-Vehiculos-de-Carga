using UnityEngine;

public class RoadScroller : MonoBehaviour
{

    [Header("Image Scroll")]
    [SerializeField] private float scrollSpeed = 2f;
    [SerializeField] private float resetPosition;
    [SerializeField] private float startPosition;

    [Header("CurveSettings")]
    [SerializeField] private bool curvesActive = false;
    [SerializeField] private float curveChanceInterval = 1f;
    [SerializeField] private float maxCurveOffset;
    [SerializeField] private float curveSpeed;

    [Header("Lane Boundaries")]
    [SerializeField] private Transform leftBoundary;
    [SerializeField] private Transform rightBoundary;
    [SerializeField] private float laneWitdth = 3f;

    float currentCurveOffset = 0f;
    float targetCurveOffset = 0f;
    float nextCuurveChangeTime;

    float CurrentCurveOffset => currentCurveOffset;

    private void Start()
    {
        nextCuurveChangeTime = Time.time * curveChanceInterval;
        //UpdateBoundaryPositions();
    }
    private void Update()
    {
        transform.Translate(Vector2.left * scrollSpeed * Time.deltaTime);

        if (transform.position.x <= resetPosition)
        {
            transform.position = new Vector3(startPosition, transform.position.y, transform.position.z);
        }
    }

    private void UpdateCurves()
    {
        if (!curvesActive) return;
        if (Time.time >= nextCuurveChangeTime)
        {
            targetCurveOffset = Random.Range(-maxCurveOffset, maxCurveOffset);
            nextCuurveChangeTime = Time.time + curveChanceInterval;
        }
        currentCurveOffset = Mathf.Lerp(currentCurveOffset, targetCurveOffset, Time.deltaTime * curveSpeed);
        UpdateBoundaryPositions();
    }

    private void UpdateBoundaryPositions()
    {
        if (leftBoundary != null)
        {
            Vector3 pos = leftBoundary.position;
            pos.y = currentCurveOffset - (laneWitdth / 2f);
            leftBoundary.position = pos;
        }

        if (rightBoundary != null)
        {
            Vector3 pos = rightBoundary.position;
            pos.x = currentCurveOffset + (laneWitdth / 2f);
            leftBoundary.position = pos;
        }
    }

    public bool IsInLane(float truckY)
    {
        float laneMin = currentCurveOffset - (laneWitdth / 2f);
        float laneMax = currentCurveOffset + (laneWitdth / 2f);
        return truckY >= laneMin && truckY <= laneMax;
    }

    public float GetLaneCenter()
    {
               return currentCurveOffset;

    }

    public void SetDifficult(bool challengeMode)
    {
        if (challengeMode)
        {
            maxCurveOffset = 1.5f;
            curveChanceInterval = 0.7f;
            scrollSpeed *= 1.2f;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        if (leftBoundary != null)
        {
            Gizmos.DrawLine(new Vector3(transform.position.x, leftBoundary.position.y, 0), new Vector3(transform.position.x + 20f, leftBoundary.position.y, 0));
        }
        if (rightBoundary != null)
        {
            Gizmos.DrawLine(new Vector3(transform.position.x, rightBoundary.position.y, 0), new Vector3(transform.position.x + 20f, rightBoundary.position.y, 0));
        }
    }
}
