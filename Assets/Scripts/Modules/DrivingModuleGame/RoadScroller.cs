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
    //[SerializeField] private Transform leftBoundary;
    //[SerializeField] private Transform rightBoundary;

    [SerializeField] private float laneWitdth = 3f;
    [SerializeField] float toleranceHB;

    float currentCurveOffset = 0f;
    float targetCurveOffset = 0f;
    float nextCuurveChangeTime;

    float CurrentCurveOffset => currentCurveOffset;

    private void Start()
    {
        nextCuurveChangeTime = Time.time * curveChanceInterval;
    }
    private void Update()
    {
        transform.Translate(Vector2.left * scrollSpeed * Time.deltaTime);

        if (transform.position.x <= resetPosition)
        {
            transform.position = new Vector3(startPosition, transform.position.y, transform.position.z);
        }

        if (curvesActive)
        {
            UpdateCurves();
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
    }

    public bool IsInLane(float truckY)
    {
        float laneMin = currentCurveOffset - (laneWitdth / 2f);
        float laneMax = currentCurveOffset + (laneWitdth / 2f);

        float tolerance = toleranceHB;

        return truckY >= (laneMin + tolerance) && truckY <= (laneMax - tolerance);
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
        Gizmos.DrawLine( new Vector3(-10, currentCurveOffset,0), new Vector3(10, currentCurveOffset, 0));

        Gizmos.color = Color.red;
        float halfWidth = laneWitdth / 2f;
        Gizmos.DrawLine(new Vector3(-10, currentCurveOffset - halfWidth, 0), new Vector3(10, currentCurveOffset - halfWidth, 0));
        Gizmos.DrawLine(new Vector3(-10, currentCurveOffset + halfWidth, 0), new Vector3(10, currentCurveOffset + halfWidth, 0));


    }
}
