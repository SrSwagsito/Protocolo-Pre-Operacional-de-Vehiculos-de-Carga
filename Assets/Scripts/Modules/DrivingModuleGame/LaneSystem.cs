using UnityEngine;

public class LaneSystem : MonoBehaviour
{
    [Header("Choose side")]
    [SerializeField] private bool isLeftBoundary = true;

    [Header("Visual Options")]
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] LineRenderer lineRenderer;

    public void Start()
    {
        if (lineRenderer != null)
        {
            ConfigureBoundaries();
        }

        if(spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }
    }

    public void ConfigureBoundaries()
    {
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.positionCount = 2;

        lineRenderer.SetPosition(0, new Vector3(-15, 0, 0));
        lineRenderer.SetPosition(0, new Vector3(15, 0, 0));

        lineRenderer.startColor = Color.white;
        lineRenderer.endColor = Color.white;
    }

    public void SetPosition(float yPosition)
    {
        Vector3 pos = transform.position;
        pos.y = yPosition;
        transform.position = pos;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = isLeftBoundary ? Color.red : Color.blue;
        Gizmos.DrawSphere(transform.position, 0.2f);

        Gizmos.DrawLine(transform.position + Vector3.left * 10, transform.position + Vector3.right * 10);
    }
}
