using System.Collections;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [SerializeField] private FallingObstacle obstaclePrefab;

    [SerializeField] private Transform leftLimit;
    [SerializeField] private Transform rightLimit;

    [SerializeField] private float spawnDelay = 2f;

    private bool spawning;

    public void StartSpawning()
    {
        if (spawning)
            return;

        spawning = true;

        StartCoroutine(SpawnRoutine());
    }

    public void StopSpawning()
    {
        spawning = false;
    }

    private IEnumerator SpawnRoutine()
    {
        while (spawning)
        {
            float randomX =
                Random.Range(
                    leftLimit.position.x,
                    rightLimit.position.x);

            Vector3 spawnPos =
                new Vector3(
                    randomX,
                    transform.position.y,
                    0);

            Instantiate(
                obstaclePrefab,
                spawnPos,
                Quaternion.identity);

            yield return new WaitForSeconds(spawnDelay);
        }
    }
}