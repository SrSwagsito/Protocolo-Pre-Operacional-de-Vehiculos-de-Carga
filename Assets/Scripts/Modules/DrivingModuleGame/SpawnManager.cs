using Newtonsoft.Json.Serialization;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private GameObject collectibleGO;
    [SerializeField] private GameObject obstacleGO;
    [SerializeField] float spawnInterval;
    [SerializeField] float moveSpeed;
    [SerializeField] float spawnX;
    [SerializeField] float minY;
    [SerializeField] float maxY;

    private bool spawning;

    

    public void StartSpawning()
    {
               spawning = true;
        InvokeRepeating(nameof(SpawnObject), 1f, spawnInterval);
    }

    public void StopSpawning()
    {
        spawning = false;
        CancelInvoke();
    }

    public void SpawnObject()
    {
        if (!spawning) return;

        float ramdomY = Random.Range(minY, maxY);
        Vector3 spawnPos = new Vector3(spawnX, ramdomY, 0);

        GameObject prefabToSpawn = Random.value > 0.3f ? collectibleGO : obstacleGO;

        GameObject spawnedObject = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
    }

    public void SetDifficulty(bool challenge)
    {
        if (challenge)
        {
            spawnInterval = spawnInterval - 1;
            moveSpeed = moveSpeed + 2;
        }
    }
}
