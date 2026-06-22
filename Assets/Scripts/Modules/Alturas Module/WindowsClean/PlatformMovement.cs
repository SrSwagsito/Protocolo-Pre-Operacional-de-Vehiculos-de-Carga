using UnityEngine;

public class PlatformMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;

    [Header("Bounds")]
    [SerializeField] private Transform topLimit;
    [SerializeField] private Transform bottomLimit;
    [SerializeField] private Transform leftLimit;
    [SerializeField] private Transform rightLimit;

    private Vector2 direction;

    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection;
    }

    private void Update()
    {
        Vector3 newPosition =
            transform.position +
            (Vector3)(direction * moveSpeed * Time.deltaTime);

        newPosition.x = Mathf.Clamp(
            newPosition.x,
            leftLimit.position.x,
            rightLimit.position.x);

        newPosition.y = Mathf.Clamp(
            newPosition.y,
            bottomLimit.position.y,
            topLimit.position.y);

        transform.position = newPosition;
    }
}