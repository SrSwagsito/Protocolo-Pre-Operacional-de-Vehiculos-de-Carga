using UnityEngine;

public class LaneSystem : MonoBehaviour
{
    [Header("Choose side")]
    [SerializeField] private bool isLeftBoundary = true;

    [Header("Visual Options")]
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] LineRenderer lineRenderer;

    public void SetPosition(float yPosition)
    {
        Vector3 pos = transform.position;
        pos.y = yPosition;
        transform.position = pos;
    }

    public void Start()
    {
        if (lineRenderer != null)
        {
            lineRenderer.startWidth = 0.1f;
            lineRenderer.endWidth = 0.1f;
            lineRenderer.SetPosition(0,new Vector3(-10, 0, 0));
            lineRenderer.SetPosition(0, new Vector3(10, 0, 0));
        }
    }
}
