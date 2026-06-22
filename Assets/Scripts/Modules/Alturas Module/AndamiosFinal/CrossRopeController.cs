using System;
using UnityEngine;

public class CrossRopeController : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;

    [SerializeField] private RopePlayerController player;

    [SerializeField] private ObstacleSpawner obstacleSpawner;

    [SerializeField] Module4Manager module4;

    [SerializeField] private int maxhits;

    int currentHits;

    private Vector3 checkPointPosition;

    private bool isSwinging;

    public event Action OnSwingStarted;
    public event Action OnSwingEnded;

    private void OnEnable()
    {
        player.OnReachedGoal += CompleteSwing;
        player.OnHitObstacle += RespawnPoint;
        
    }

    private void OnDisable()
    {
        player.OnReachedGoal -= CompleteSwing;
        player.OnHitObstacle -= RespawnPoint;
    }

    public void StartSwinging()
    {
        if (isSwinging)
            return;

        gameObject.SetActive(true);

        isSwinging = true;

        checkPointPosition =
            spawnPoint.position;

        player.transform.position =
            checkPointPosition;

        player.EnableMovement();

        obstacleSpawner.StartSpawning();

        OnSwingStarted?.Invoke();
    }

    public void RespawnPoint()
    {
        currentHits++;

        if (currentHits >= maxhits)
        {
            module4.HandleFail();
            return;
        }

        player.transform.position =
            checkPointPosition;
    }

    private void CompleteSwing()
    {
        player.DisableMovement();

        obstacleSpawner.StopSpawning();

        isSwinging = false;

        gameObject.SetActive(false);

        OnSwingEnded?.Invoke();
    }
}