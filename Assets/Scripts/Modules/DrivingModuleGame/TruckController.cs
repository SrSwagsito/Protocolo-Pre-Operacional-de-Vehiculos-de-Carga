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

    [SerializeField] Color normalColor;
    [SerializeField] Color warningColor;

    private bool isOutOfLane = false;

    private float timeOutLane = 0f;
    private float lastPenaltyTime = 0f;

    private void Update()
    {
        HandleInput();
        CheckLanePosition();
        ClampPosition();
    }

    private void HandleInput()
    {
        float inputY = Input.GetAxis("Vertical");

#if UNITY_ANDROID || UNITY_IOS || UNITY_EDITOR
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector3 touchPosition = Camera.main.ScreenToWorldPoint(touch.position);
            float targetY = touchPosition.y;
            float currentY = transform.position.y;

            if (Mathf.Abs(targetY - currentY) > 0.2f)
            {
                inputY = Mathf.Sign(targetY - currentY);
            }
        }
#endif

        if(inputY != 0f)
        {
            Vector3 movement = new Vector3(0, inputY * moveSpeed * Time.deltaTime, 0);
            transform.Translate(movement);
        }
    }

    private void CheckLanePosition()
    {
        if (roadScroller != null) return;

        float truckY = transform.position.y;

        bool IsInLane = roadScroller.IsInLane(truckY);

        if (!IsInLane && !isOutOfLane)
        {
            OnExitLane();
            isOutOfLane = true;
            timeOutLane = 0f;
        }

        else if (IsInLane && isOutOfLane)
        {
            OnEnterLane();
            isOutOfLane = false;
            timeOutLane = 0f;
        }

        if (isOutOfLane)
        {
            timeOutLane += Time.deltaTime;
            if (Time.time - lastPenaltyTime >= penaltyInterval)
            {
                ApplyPenalty();
                lastPenaltyTime = Time.time;
            }
        }
    }

    private void OnExitLane()
    {
        if (truckSprite != null)
        {
            truckSprite.color = warningColor; // Cambia el color del camión a rojo
        }
    }

    private void OnEnterLane()
    {
        if(truckSprite != null)
        {
            truckSprite.color = normalColor; // Cambia el color del camión a blanco
        }
    }

    private void ApplyPenalty()
    {
        int penalty = Mathf.RoundToInt(outOfLanePenalty);
        OnOutOfLane?.Invoke(penalty);
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
