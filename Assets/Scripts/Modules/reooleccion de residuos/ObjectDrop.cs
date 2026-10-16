
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
        // Esperar a que Unity actualice el estado de la UI.
        yield return null;

        while (tutorialPop != null &&
               tutorialPop.activeInHierarchy)
        {
            yield return null;
        }

        if (objectPrefabs == null || objectPrefabs.Length == 0)
        {
            Debug.LogError(
                "ObjectDrop: No hay prefabs asignados.", this);
            yield break;
        }

        if (maxObjects <= 0)
        {
            Debug.LogError(
                "ObjectDrop: maxObjects debe ser mayor que cero.", this);
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
                int randomIndex = Random.Range(
                    0, objectPrefabs.Length);

                float randomX = Random.Range(-5.22f, 10.21f);

                Vector3 spawnPosition = new Vector3(
                    randomX,
                    transform.position.y,
                    transform.position.z
                );

                GameObject spawnedObject = Instantiate(
                    objectPrefabs[randomIndex],
                    spawnPosition,
                    Quaternion.identity
                );

                if (spawnedObject != null)
                {
                    currentObjects++;
                    totalSpawned++;

                    Debug.Log(
                        $"Generados: {totalSpawned}/{totalObjectsToSpawn} | " +
                        $"Basuras activas registradas: {currentObjects}"
                    );
                }
            }

            yield return new WaitForSeconds(
                Mathf.Max(0.01f, spawnRate));
        }

        spawningFinished = true;

        Debug.Log(
            "ObjectDrop: terminó la generación de residuos.");

        TryFinishInspection();
    }

    // Llamar una sola vez por cada residuo eliminado.
    public void RemoveObject()
    {
        if (inspectionFinished)
            return;

        if (currentObjects <= 0)
        {
            Debug.LogWarning(
                "ObjectDrop: el contador de residuos ya está en cero.",
                this);
            return;
        }

        currentObjects--;

        Debug.Log(
            $"Residuo eliminado. Quedan: {currentObjects} | " +
            $"Generados: {totalSpawned}/{totalObjectsToSpawn}");

        TryFinishInspection();
    }

    private void TryFinishInspection()
    {
        if (inspectionFinished)
            return;

        // Deben haberse generado todos los residuos
        // y no debe quedar ninguno registrado como activo.
        if (!spawningFinished || currentObjects > 0)
            return;

        inspectionFinished = true;

        Debug.Log(
            "ObjectDrop: todos los residuos fueron procesados.");

        if (ManagerRecolection.Instance != null)
        {
            ManagerRecolection.Instance.FinishInspection();
        }
        else
        {
            Debug.LogError(
                "ObjectDrop: no existe ManagerRecolection en la escena.");
        }
    }
}
