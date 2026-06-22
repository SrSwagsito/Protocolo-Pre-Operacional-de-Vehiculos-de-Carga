using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class StackBlock : MonoBehaviour
{
    private Rigidbody2D rb;

    private Transform hook;

    private bool followingHook;

    public bool IsLocked { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (!followingHook)
            return;

        transform.position = hook.position;
    }

    public void Hold(Transform hookTransform)
    {
        hook = hookTransform;

        followingHook = true;

        IsLocked = false;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0;

        rb.isKinematic = true;
    }

    public void Release()
    {
        if (!followingHook)
            return;

        followingHook = false;

        rb.bodyType = RigidbodyType2D.Dynamic;

        rb.simulated = true;
    }

    public void Lock()
    {
        followingHook = false;

        IsLocked = true;

        rb.linearVelocity = Vector2.zero;

        rb.angularVelocity = 0;

        rb.bodyType = RigidbodyType2D.Kinematic;
    }
}