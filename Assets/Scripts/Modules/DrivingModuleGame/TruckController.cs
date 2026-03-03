using System;
using UnityEngine;

public class TruckContorller : MonoBehaviour
{
    public Action<int> OnCollectibleHit;
    public Action<int> OnObstacleHit;
    public Action<int> OnOutOfLane;

    [SerializeField] private float moveSpeed;
    [SerializeField] private float verticalLimit;
    [SerializeField] private RoadScroller roadScroller;
    [SerializeField] private float outOfLanePenalty;
    [SerializeField] private float penaltyInterval;
    [SerializeField] private SpriteRenderer truckSprite;

    private bool isOutOfLane = false;
    private float lastPenaltyTime = 0f;

    private void Update()
    {
        float inputY = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(0, inputY * moveSpeed * Time.deltaTime, 0);
        transform.Translate(movement);

        Vector3 pos = transform.position;
        pos.y = Mathf.Clamp(pos.y, -verticalLimit, verticalLimit);
        transform.position = pos;
        
        if(roadScroller != null)
        {
            float truckY = transform.position.y;
            bool inLane = roadScroller.IsInLane(truckY);

            if (!inLane && Time.time - lastPenaltyTime >= penaltyInterval)
            {
                OnOutOfLane?.Invoke(Mathf.RoundToInt(outOfLanePenalty));
                lastPenaltyTime = Time.time;
                isOutOfLane = true;
                if(truckSprite != null)
                {
                    truckSprite.color = Color.red; // Cambia el color del camión a rojo
                }
                else if (inLane && isOutOfLane)
                {
                    isOutOfLane = false;
                    if(truckSprite != null)
                    {
                        truckSprite.color = Color.white; // Cambia el color del camión a blanco
                    }
                }
            }
        }

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
