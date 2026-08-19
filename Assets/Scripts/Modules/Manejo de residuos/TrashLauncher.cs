using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class TrashLauncher : MonoBehaviour
{
    private Rigidbody2D rb;

    private Vector2 startPosition;
    private bool isDragging;

    [Header("Configuración")]
    public float maxDragDistance = 3f;
    public float launchForce = 10f;

    [Header("Trayectoria")]
    public LineRenderer lineRenderer;
    public int trajectoryPoints = 30;
    public float timeStep = 0.1f;

    private Camera mainCamera;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;

        startPosition = transform.position;

        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    private void Update()
    {
        Debug.Log("Update corriendo");

#if UNITY_ANDROID || UNITY_IOS
        HandleTouch();
#else
    HandleMouse();
#endif
    }

    private void Launch()
    {
        isDragging = false;

        lineRenderer.positionCount = 0;

        rb.bodyType = RigidbodyType2D.Dynamic;

        Vector2 releaseDirection =
            startPosition - (Vector2)transform.position;

        rb.linearVelocity = Vector2.zero;

        rb.AddForce(
            releaseDirection * launchForce,
            ForceMode2D.Impulse
        );
    }

    private void DrawTrajectory(Vector2 launchDirection)
    {
        lineRenderer.positionCount = trajectoryPoints;

        Vector2 velocity = launchDirection * launchForce;

        for (int i = 0; i < trajectoryPoints; i++)
        {
            float t = i * timeStep;

            Vector2 point =
                startPosition +
                velocity * t +
                0.5f * Physics2D.gravity * t * t;

            lineRenderer.SetPosition(i, point);
        }
    }
    private void HandleTouch()
    {
        TouchControl touch = Touchscreen.current.primaryTouch;

        Vector2 touchPos = touch.position.ReadValue();

        Vector3 worldPos = mainCamera.ScreenToWorldPoint(touchPos);
        worldPos.z = 0;

        if (touch.press.wasPressedThisFrame)
        {
            Collider2D hit = Physics2D.OverlapPoint(worldPos);

            if (hit != null && hit.gameObject == gameObject)
            {
                isDragging = true;
            }
        }

        if (isDragging && touch.press.isPressed)
        {
            DragObject(worldPos);
        }

        if (isDragging && touch.press.wasReleasedThisFrame)
        {
            Launch();
        }
    }
    private void HandleMouse()
{
    if (Mouse.current == null)
        return;

    Debug.Log("Mouse Detectado");

    Vector2 mousePos = Mouse.current.position.ReadValue();

    Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);
    worldPos.z = 0;

    if (Mouse.current.leftButton.wasPressedThisFrame)
    {
        Collider2D hit = Physics2D.OverlapPoint(worldPos);

        if (hit != null && hit.gameObject == gameObject)
        {
            isDragging = true;
        }
    }

    if (isDragging && Mouse.current.leftButton.isPressed)
    {
        DragObject(worldPos);
    }

    if (isDragging && Mouse.current.leftButton.wasReleasedThisFrame)
    {
        Launch();
    }
}
    private void DragObject(Vector3 worldPos)
    {
        Vector2 direction = (Vector2)worldPos - startPosition;

        if (direction.magnitude > maxDragDistance)
        {
            direction = direction.normalized * maxDragDistance;
        }

        transform.position = startPosition + direction;

        Vector2 releaseDirection =
            startPosition - (Vector2)transform.position;

        DrawTrajectory(releaseDirection);
    }
}