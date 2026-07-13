using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BackgroundWalker : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 1.5f;

    [SerializeField] private Transform leftLimit;
    [SerializeField] private Transform rightLimit;

    [Header("Settings")]
    [SerializeField] private bool startMovingRight = true;

    private SpriteRenderer spriteRenderer;

    private int direction;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        direction = startMovingRight ? 1 : -1;

        spriteRenderer.flipX = direction < 0;
    }

    private void Update()
    {
        transform.Translate(Vector3.right * direction * speed * Time.deltaTime);

        CheckLimits();
    }

    private void CheckLimits()
    {
        if (direction > 0 &&
            transform.position.x >= rightLimit.position.x)
        {
            direction = -1;
            spriteRenderer.flipX = true;
        }

        if (direction < 0 &&
            transform.position.x <= leftLimit.position.x)
        {
            direction = 1;
            spriteRenderer.flipX = false;
        }
    }
}