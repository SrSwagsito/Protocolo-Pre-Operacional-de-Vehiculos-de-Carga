using UnityEngine;

/// <summary>
/// SpawnManager mejorado que sincroniza el spawn de objetos
/// con la posición de la carretera.
/// </summary>
public class SpawnManager : MonoBehaviour
{
    [Header("Spawn Objects")]
    [SerializeField] private GameObject collectibleGO;
    [SerializeField] private GameObject obstacleGO;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float spawnX = 10f;

    [Header("Road Synchronization - NUEVO")]
    [Tooltip("Referencia al sistema de carretera para sincronizar spawns")]
    [SerializeField] private RoadPivotSystem roadSystem;

    [Tooltip("Rango vertical dentro del carril (0.8 = 80% del ancho)")]
    [Range(0.5f, 1f)]
    [SerializeField] private float laneUsagePercentage = 0.8f;

    [Header("Fallback (si no hay roadSystem)")]
    [SerializeField] private float minY = -2f;
    [SerializeField] private float maxY = 2f;

    private bool spawning = false;

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

        // Calcular posición Y basada en la carretera
        float spawnY = CalculateSpawnY();

        Vector3 spawnPos = new Vector3(spawnX, spawnY, 0);

        // Decidir qué spawear (70% coleccionable, 30% obstáculo)
        GameObject prefabToSpawn = Random.value > 0.3f ? collectibleGO : obstacleGO;

        GameObject spawnedObject = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);

        // Configurar velocidad
        if (spawnedObject.TryGetComponent<MoveLeft>(out var mover))
        {
            mover.Initialize(moveSpeed);
        }
    }

    /// <summary>
    /// Calcula la posición Y de spawn basándose en la carretera
    /// </summary>
    private float CalculateSpawnY()
    {
        if (roadSystem != null)
        {
            // Obtener el centro de la carretera en la posición de spawn
            float roadCenterY = roadSystem.GetOffsetAtX(spawnX);

            // Calcular rango dentro del carril
            float laneWidth = roadSystem.LaneWidth;
            float halfLane = (laneWidth * laneUsagePercentage) / 2f;

            // Spawn aleatorio dentro del carril
            float randomOffset = Random.Range(-halfLane, halfLane);

            return roadCenterY + randomOffset;
        }
        else
        {
            // Fallback: usar rango fijo si no hay roadSystem
            return Random.Range(minY, maxY);
        }
    }

    public void SetDifficulty(bool challenge)
    {
        if (challenge)
        {
            spawnInterval = Mathf.Max(0.5f, spawnInterval - 1f);
            moveSpeed += 2f;
        }
    }

    /// <summary>
    /// Limpia todos los objetos spawneados (útil al reiniciar)
    /// </summary>
    public void ClearAllSpawnedObjects()
    {
        // Encontrar todos los coleccionables y obstáculos
        GameObject[] collectibles = GameObject.FindGameObjectsWithTag("Collectible");
        GameObject[] obstacles = GameObject.FindGameObjectsWithTag("Obstacle");

        foreach (var obj in collectibles)
        {
            Destroy(obj);
        }

        foreach (var obj in obstacles)
        {
            Destroy(obj);
        }
    }

    /// <summary>
    /// Debug: Dibuja el área de spawn
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (roadSystem != null)
        {
            // Dibujar área de spawn sincronizada con carretera
            float roadY = roadSystem.CurrentRoadY;
            float laneWidth = roadSystem.LaneWidth;
            float halfLane = (laneWidth * laneUsagePercentage) / 2f;

            Gizmos.color = Color.green;

            // Línea superior del área de spawn
            Gizmos.DrawLine(
                new Vector3(spawnX - 0.5f, roadY + halfLane, 0),
                new Vector3(spawnX + 0.5f, roadY + halfLane, 0)
            );

            // Línea inferior
            Gizmos.DrawLine(
                new Vector3(spawnX - 0.5f, roadY - halfLane, 0),
                new Vector3(spawnX + 0.5f, roadY - halfLane, 0)
            );

            // Líneas laterales
            Gizmos.DrawLine(
                new Vector3(spawnX - 0.5f, roadY - halfLane, 0),
                new Vector3(spawnX - 0.5f, roadY + halfLane, 0)
            );

            Gizmos.DrawLine(
                new Vector3(spawnX + 0.5f, roadY - halfLane, 0),
                new Vector3(spawnX + 0.5f, roadY + halfLane, 0)
            );
        }
        else
        {
            // Dibujar área de spawn con fallback
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(
                new Vector3(spawnX, (minY + maxY) / 2f, 0),
                new Vector3(1f, maxY - minY, 0.1f)
            );
        }
    }
}