using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class StackBlock : MonoBehaviour
{
    private Rigidbody2D _rb;

    private void Awake() => _rb = GetComponent<Rigidbody2D>();

    public void AttachToHook(Transform hook)
    {
        _rb.bodyType = RigidbodyType2D.Kinematic;
        transform.SetParent(hook);
        transform.localPosition = Vector2.zero;
    }

    public void Release()
    {
        transform.SetParent(null);
        _rb.bodyType = RigidbodyType2D.Dynamic;
    }

    public void Freeze()
    {
        _rb.linearVelocity = Vector2.zero;
        _rb.angularVelocity = 0f;
        _rb.bodyType = RigidbodyType2D.Kinematic;
    }
}