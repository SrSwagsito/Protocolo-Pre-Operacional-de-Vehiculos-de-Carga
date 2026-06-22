using UnityEngine;

public class HookSwing : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform pivot;

    [Header("Swing")]
    [SerializeField] private float ropeLength = 4f;
    [SerializeField] private float swingAngle = 40f;
    [SerializeField] private float swingSpeed = 1.8f;

    private float timer;
    private bool isSwinging = true;

    public Transform Hook => transform;

    public void EnableSwing()
    {
        isSwinging = true;
    }

    public void DisableSwing()
    {
        isSwinging = false;
    }

    private void Update()
    {
        if (!isSwinging)
            return;

        timer += Time.deltaTime * swingSpeed;

        float angle =
            Mathf.Sin(timer) * swingAngle;

        Vector2 direction =
            Quaternion.Euler(0, 0, angle) *
            Vector2.down;

        transform.position =
            pivot.position +
            (Vector3)(direction * ropeLength);
    }
}