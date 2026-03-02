using UnityEngine;

public class MoveLeft : MonoBehaviour
{
    private float speed;

    public void Initialize(float moveSpeed)
    {
        speed = moveSpeed;
    }

    private void Update()
    {
     //transform.Translate(Vector2.left * speed * Time.deltaTime);
        transform.LeanMoveX(Vector2.left.x * speed * Time.deltaTime, 4f).setEaseInOutSine();

        if (transform.position.x < -15f) // Ajusta este valor según el límite de tu escena
        {
            Destroy(gameObject);
        }
    }
}
