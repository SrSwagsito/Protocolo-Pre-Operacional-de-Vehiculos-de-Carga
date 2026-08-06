using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Entrada configurable en el Inspector para cada tipo de obstáculo.
/// </summary>
[Serializable]
public class ObstaclePrefabEntry
{
    public GameObject prefab;
    [Min(1), Tooltip("Cantidad máxima de este prefab activa simultáneamente.")]
    public int maxInPool = 5;
}

/// <summary>
/// Genera obstáculos durante la Fase 2 utilizando Object Pooling.
///
/// Cambios respecto a la versión anterior:
/// - Reemplaza Instantiate/Destroy por un pool por prefab.
/// - La lista obstacleEntries es extensible desde el Inspector.
/// - Cuando un obstáculo sale del recycleY, se devuelve al pool en lugar
///   de destruirse.
/// - StopSpawning() también devuelve todos los objetos activos al pool.
/// </summary>
public class ObstacleSpawner : MonoBehaviour
{
    [Header("Prefabs y Pool")]
    public List<ObstaclePrefabEntry> obstacleEntries = new();

    [Header("Puntos de Spawn")]
    public Transform[] spawnPoints;

    [Header("Intervalos")]
    public float minInterval = 1.5f;
    public float maxInterval = 3.5f;

    [Header("Reciclaje")]
    [Tooltip("Coordenada Y a la que el obstáculo vuelve al pool.")]
    public float recycleY = -10f;

    // ─── Estado interno del pool ──────────────────────────────────────────────
    private Dictionary<GameObject, Queue<GameObject>> _pools        = new();
    private Dictionary<GameObject, GameObject>        _instanceToP  = new(); // instancia → prefab
    private List<GameObject>                          _activeObjects = new();
    private bool _spawning;

    // ─── Unity ───────────────────────────────────────────────────────────────

    private void Awake() => InitializePools();

    // ─── Inicialización del pool ──────────────────────────────────────────────

    private void InitializePools()
    {
        foreach (var entry in obstacleEntries)
        {
            if (entry.prefab == null) continue;

            var pool = new Queue<GameObject>();
            for (int i = 0; i < entry.maxInPool; i++)
            {
                var go = Instantiate(entry.prefab);
                go.SetActive(false);
                pool.Enqueue(go);
                _instanceToP[go] = entry.prefab;
            }
            _pools[entry.prefab] = pool;
        }
    }

    // ─── API pública ──────────────────────────────────────────────────────────

    public void StartSpawning()
    {
        _spawning = true;
        StartCoroutine(SpawnLoop());
    }

    public void StopSpawning()
    {
        _spawning = false;
        ReturnAllActive();
    }

    // ─── Bucle de spawning ────────────────────────────────────────────────────

    private IEnumerator SpawnLoop()
    {
        while (_spawning)
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(minInterval, maxInterval));
            if (!_spawning) break;
            SpawnOne();
        }
    }

    private void SpawnOne()
    {
        if (obstacleEntries.Count == 0 || spawnPoints.Length == 0) return;

        // Elegir prefab y punto aleatorios
        var entry = obstacleEntries[UnityEngine.Random.Range(0, obstacleEntries.Count)];
        if (entry.prefab == null) return;
        if (!_pools.TryGetValue(entry.prefab, out var pool) || pool.Count == 0) return; // pool agotado

        var point = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];

        // Sacar del pool
        var go = pool.Dequeue();
        go.transform.position = point.position;
        go.SetActive(true);
        _activeObjects.Add(go);

        // Inicializar el obstáculo con el callback de reciclaje
        var obstacle = go.GetComponent<FallingObstacle>();
        obstacle?.Initialize(recycleY, ReturnToPool);
    }

    // ─── Gestión del pool ─────────────────────────────────────────────────────

    private void ReturnToPool(GameObject go)
    {
        if (!go.activeSelf) return; // ya devuelto (guardia contra dobles llamadas)

        go.SetActive(false);
        _activeObjects.Remove(go);

        if (_instanceToP.TryGetValue(go, out var prefab) &&
            _pools.TryGetValue(prefab, out var pool))
        {
            pool.Enqueue(go);
        }
    }

    private void ReturnAllActive()
    {
        // Iterar sobre copia para evitar modificar la lista mientras se itera
        for (int i = _activeObjects.Count - 1; i >= 0; i--)
            ReturnToPool(_activeObjects[i]);
    }
}
