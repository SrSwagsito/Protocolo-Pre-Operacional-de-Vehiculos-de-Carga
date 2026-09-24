using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private PlayerInput playerInput;
    private InputAction moveAction;

    private Vector2 movement;
    private Vector2 buttonMovement;

    private Rigidbody2D rb;

    public float speed = 5f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Move"];
    }

    void Update()
    {
        Vector2 inputMovement = moveAction.ReadValue<Vector2>();

        // Si hay input físico úsalo, si no usa botones
        movement = inputMovement != Vector2.zero
            ? inputMovement
            : buttonMovement;
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = movement * speed;
    }

    public void MoveRight()
    {
        buttonMovement = Vector2.right;
    }

    public void MoveLeft()
    {
        buttonMovement = Vector2.left;
    }

    public void StopMove()
    {
        buttonMovement = Vector2.zero;
    }
}