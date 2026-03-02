using UnityEngine;

public class RoadScroller : MonoBehaviour
{
    [SerializeField] private float scrollSpeed = 2f;
    [SerializeField] private float resetPosition;
    [SerializeField] private float startPosition;
    void Update()
    {
        transform.Translate(Vector2.left * scrollSpeed * Time.deltaTime);

        if (transform.position.x <= resetPosition)
        {
            transform.position = new Vector3(startPosition, transform.position.y, transform.position.z);
        }
    }
}
