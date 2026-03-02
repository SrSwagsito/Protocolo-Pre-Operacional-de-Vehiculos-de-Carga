using System;
using UnityEngine;

public class TruckContorller : MonoBehaviour
{
    public Action<int> OnCollectibleHit;
    public Action<int> OnObstacleHit;

    [SerializeField] private float moveSpeed;
    [SerializeField] private float verticalLimit;

    private void Update()
    {
        float inputY = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(0, inputY * moveSpeed * Time.deltaTime, 0);
        transform.Translate(movement);

        ClampPosition();
    }

    private void ClampPosition()
    {
        Vector3 pos = transform.position;
        pos.y = Mathf.Clamp(pos.y, -verticalLimit, verticalLimit);
        transform.position = pos;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Collectible"))
        {
            OnCollectibleHit?.Invoke(10); // Incrementa el contador de coleccionables
            Destroy(collision.gameObject);
        }
        else if (collision.CompareTag("Obstacle"))
        {
            OnObstacleHit?.Invoke(5); // Incrementa el contador de obstáculos
            Destroy(collision.gameObject);
        }
    }
}
