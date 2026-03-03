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

     gameObject.transform.Translate(Vector3.left * speed * Time.deltaTime, Space.World);
        

        if (transform.position.x < -15f) // Ajusta este valor según el límite de tu escena
        {
            Destroy(gameObject);
        }
    }
}
