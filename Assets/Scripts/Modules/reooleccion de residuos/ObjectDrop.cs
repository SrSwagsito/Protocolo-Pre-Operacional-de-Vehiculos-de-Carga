
using System.Collections;
using UnityEngine;

public class ObjectDrop : MonoBehaviour
{
    [Header("Prefabs a generar")]
    [SerializeField] private GameObject[] objectPrefabs;

    [Header("Configuración de spawn")]
    [SerializeField] private float spawnRate = 0.5f;
    [SerializeField] private int maxObjects = 20;
    [SerializeField] private int totalObjectsToSpawn = 50;

    [Header("Tutorial")]
    [SerializeField] private GameObject tutorialPop;

    private int currentObjects;
    private int totalSpawned;

    private bool spawningFinished;
    private bool inspectionFinished;

    private void Start()
    {
        StartCoroutine(WaitForTutorial());

    }

    private IEnumerator WaitForTutorial()
    {
        yield return null;

        while (tutorialPop != null &&
               tutorialPop.activeInHierarchy)
        {
            yield return null;
        }

        if (objectPrefabs == null || objectPrefabs.Length == 0)
        {
            Debug.LogError(
                "ObjectDrop: No hay prefabs asignados.",
                this
            );
            yield break;
        }

        if (totalObjectsToSpawn <= 0)
        {
            spawningFinished = true;
            TryFinishInspection();
            yield break;
        }

        yield return StartCoroutine(SpawnRain());
    }

    private IEnumerator SpawnRain()
    {
        while (totalSpawned < totalObjectsToSpawn)
        {
            if (currentObjects < maxObjects)
            {
                float randomX = Random.Range(-5.22f, 10.21f);

                int randomIndex = Random.Range(
                    0,
                    objectPrefabs.Length
                );

                Vector3 spawnPosition = new Vector3(
                    randomX,
                    transform.position.y,
                    transform.position.z
                );

                Instantiate(
                    objectPrefabs[randomIndex],
                    spawnPosition,
                    Quaternion.identity
                );

                currentObjects++;
                totalSpawned++;

                Debug.Log(
                    $"Generados: {totalSpawned}/{totalObjectsToSpawn} | " +
                    $"Objetos vivos: {currentObjects}"
                );
            }

            yield return new WaitForSeconds(spawnRate);
        }

        spawningFinished = true;

        Debug.Log("ObjectDrop: terminó la generación.");

        TryFinishInspection();
    }

    // Este método debe llamarse una sola vez por cada residuo eliminado.
    public void RemoveObject()
    {
        if (inspectionFinished)
            return;

        if (currentObjects <= 0)
        {
            Debug.LogWarning(
                "ObjectDrop: se intentó eliminar un objeto " +
                "cuando no hay objetos registrados.",
                this
            );
            return;
        }

        currentObjects--;

        Debug.Log(
            $"Residuo eliminado. Quedan {currentObjects} | " +
            $"Generados: {totalSpawned}/{totalObjectsToSpawn}"
        );

        TryFinishInspection();
    }

    private void TryFinishInspection()
    {
        if (inspectionFinished)
            return;

        // No termina hasta generar todos los residuos
        // y registrar que ya no queda ninguno.
        if (!spawningFinished || currentObjects != 0)
            return;

        inspectionFinished = true;

        Debug.Log(
            "ObjectDrop: todos los residuos fueron procesados."
        );

        if (ManagerRecolection.Instance != null)
        {
            ManagerRecolection.Instance.FinishInspection();
        }
        else
        {
            Debug.LogError(
                "ObjectDrop: no existe ManagerRecolection en la escena."
            );
        }
    }
}
