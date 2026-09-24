using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;

    public float minX;
    public float maxX;

    private float currentY;

    private void Start()
    {
        currentY = transform.position.y;
    }

    private void LateUpdate()
    {
        float x = Mathf.Clamp(player.position.x, minX, maxX);

        transform.position = new Vector3(
            x,
            currentY,
            transform.position.z
        );
    }

    public void SetLevelY(float y)
    {
        currentY = y;
    }
}