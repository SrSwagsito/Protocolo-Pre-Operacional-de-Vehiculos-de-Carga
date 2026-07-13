using UnityEngine;

public class PlatformMovement : MonoBehaviour
{
    [Header("Objects")]
    [SerializeField] private Transform baseObject;
    [SerializeField] private Transform platformObject;


    [Header("Movement")]
    [SerializeField] private float baseSpeed = 2f;
    [SerializeField] private float platformSpeed = 2f;


    [Header("Base X Limits")]
    [SerializeField] private Transform leftLimit;
    [SerializeField] private Transform rightLimit;


    [Header("Platform Limits")]
    [SerializeField] private Transform topLimit;
    [SerializeField] private Transform bottomLimit;


    private Vector2 inputDirection;


    public void SetDirection(Vector2 direction)
    {
        inputDirection = direction;
    }


    private void Update()
    {
        MoveBase();
        MovePlatform();
    }


    private void MoveBase()
    {
        Vector3 position = baseObject.position;


        // Solo movimiento horizontal
        position.x += inputDirection.x * baseSpeed * Time.deltaTime;


        position.x = Mathf.Clamp(
            position.x,
            leftLimit.position.x,
            rightLimit.position.x
        );


        baseObject.position = position;
    }



    private void MovePlatform()
    {
        Vector3 position = platformObject.position;


        // Movimiento libre de plataforma
        position.x += inputDirection.x * platformSpeed * Time.deltaTime;
        position.y += inputDirection.y * platformSpeed * Time.deltaTime;


        position.x = Mathf.Clamp(
            position.x,
            leftLimit.position.x,
            rightLimit.position.x
        );


        position.y = Mathf.Clamp(
            position.y,
            bottomLimit.position.y,
            topLimit.position.y
        );


        platformObject.position = position;
    }
}