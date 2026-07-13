using UnityEngine;
using System.Collections;

public class ObstacleSpawner : MonoBehaviour
{
    public GameObject[] obstaclePrefabs;
    public Transform[] spawnPoints;
    public float minInterval = 1.5f;
    public float maxInterval = 3.5f;

    private bool _spawning;

    public void StartSpawning()
    {
        _spawning = true;
        StartCoroutine(SpawnLoop());
    }

    public void StopSpawning() => _spawning = false;

    private IEnumerator SpawnLoop()
    {
        while (_spawning)
        {
            yield return new WaitForSeconds(Random.Range(minInterval, maxInterval));
            if (!_spawning) break;
            var prefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
            var point = spawnPoints[Random.Range(0, spawnPoints.Length)];
            Instantiate(prefab, point.position, Quaternion.identity);
        }
    }
}