using System.Collections;
using UnityEngine;

public class ObjectDrop : MonoBehaviour
{
    [Header("Prefabs a generar")]
    [SerializeField] private GameObject[] objectPrefabs;

    [Header("Configuración de spawn")]
    [SerializeField] private float spawnY = 10f;
    [SerializeField] private float spawnRate = 0.5f;

    [Tooltip("Máximo de objetos vivos al mismo tiempo")]
    [SerializeField] private int maxObjects = 20;

    [Tooltip("Cantidad total de objetos que se van a generar")]
    [SerializeField] private int totalObjectsToSpawn = 50;

    [Header("Tutorial")]
    [SerializeField] private GameObject tutorialPop;

    private int currentObjects = 0;
    private int totalSpawned = 0;
    private bool tutorialFinished = false;

    private void Start()
    {
        StartCoroutine(WaitForTutorial());
    }

    private IEnumerator WaitForTutorial()
    {
        while (tutorialPop != null && tutorialPop.activeInHierarchy)
        {
            yield return null;
        }

        tutorialFinished = true;

        Debug.Log("Tutorial terminado. Iniciando spawn.");

        StartCoroutine(SpawnRain());
    }

    private IEnumerator SpawnRain()
    {
        while (tutorialFinished)
        {
            if (currentObjects < maxObjects &&
                totalSpawned < totalObjectsToSpawn)
            {
                float randomX = Random.Range(-5.22f, 10.21f);

                int randomIndex = Random.Range(0, objectPrefabs.Length);

                Instantiate(
                    objectPrefabs[randomIndex],
                    new Vector3(randomX, spawnY, 0f),
                    Quaternion.identity
                );

                currentObjects++;
                totalSpawned++;

                Debug.Log(
                    $"Spawn: {totalSpawned}/{totalObjectsToSpawn} | Vivos: {currentObjects}/{maxObjects}"
                );
            }

            yield return new WaitForSeconds(spawnRate);
        }

        Debug.Log("Se alcanzó el límite total de objetos.");
    }

    public void RemoveObject()
    {
        currentObjects--;

        if (currentObjects < 0)
            currentObjects = 0;

        if (totalSpawned >= totalObjectsToSpawn &&
            currentObjects <= 0)
        {
            ManagerRecolection.Instance.FinishInspection();
        }
    }
}