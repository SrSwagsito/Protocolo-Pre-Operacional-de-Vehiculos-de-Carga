
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class TrashFall : MonoBehaviour
{
    [SerializeField] private float fallSpeed = 4f;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.down * fallSpeed;
    }
}
