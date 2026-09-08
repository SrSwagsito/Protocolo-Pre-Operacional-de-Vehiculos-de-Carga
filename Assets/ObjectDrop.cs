using System.Collections;
using UnityEngine;

public class ObjectDrop : MonoBehaviour
{
    [Header("Prefabs a generar")]
    [SerializeField] private GameObject[] objectPrefabs;

    [Header("Configuración de spawn")]
    [SerializeField] private float spawnY = 10f;
    [SerializeField] private float spawnRate = 0.5f;
    [SerializeField] private int maxObjects = 20;

    [Header("Tutorial")]
    [SerializeField] private GameObject tutorialPop;

    private int currentObjects = 0;
    private bool tutorialFinished = false;

    private void Start()
    {
        StartCoroutine(WaitForTutorial());
    }

    private IEnumerator WaitForTutorial()
    {
        // Esperar mientras el TutorialPop esté activo
        while (tutorialPop != null && tutorialPop.activeInHierarchy)
        {
            yield return null;
        }

        // El tutorial terminó
        tutorialFinished = true;

        // Comenzar a generar objetos
        StartCoroutine(SpawnRain());
    }

    private IEnumerator SpawnRain()
    {
        while (tutorialFinished)
        {
            if (currentObjects < maxObjects)
            {
                float randomX = Random.Range(-19f, 19f);

                // Seleccionar un prefab aleatorio
                int randomIndex = Random.Range(0, objectPrefabs.Length);

                GameObject obj = Instantiate(
                    objectPrefabs[randomIndex],
                    new Vector3(randomX, spawnY, 0f),
                    Quaternion.identity
                );

                currentObjects++;
            }

            yield return new WaitForSeconds(spawnRate);
        }
    }

    public void RemoveObject()
    {
        currentObjects--;

        // Evitar valores negativos
        if (currentObjects < 0)
        {
            currentObjects = 0;
        }
    }
}

