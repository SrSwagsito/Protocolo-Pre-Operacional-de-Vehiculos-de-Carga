using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private PlayerControl controls;
    private PlayerInput playerInput;
    private InputAction moveAction;

    private Vector2 movement;
    private Rigidbody2D rb;

    public float speed = 5f;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Move"];
    }

    // Update is called once per frame
    void Update()
    {
        movement = moveAction.ReadValue<Vector2>();

    }

    private void FixedUpdate()
    {
        rb.linearVelocity = movement * speed;
    }

    public void MoveRight()
    {
        movement = Vector2.right;
    }

    public void StopMove()
    {
        movement = Vector2.zero;
    }

}
