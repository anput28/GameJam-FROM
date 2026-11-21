using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Info")]
    [SerializeField] private GameObject EnemyPrefab;
    [SerializeField] private Transform[] SpawnPoints;

    [Header("Spawn Settings")]
    [Tooltip("Max enemies alive at the same time. Spawning pauses when this is reached.")]
    [SerializeField, Min(1)] private int MaxAlive = 10;
    [SerializeField, Min(0f)] private float MinDelay = 1f;
    [SerializeField, Min(0f)] private float MaxDelay = 3f;
    [Tooltip("How often to recheck for a free slot while at the cap.")]
    [SerializeField, Min(0.05f)] private float CheckInterval = 0.5f;
    [Tooltip("Search radius used to snap the spawn point onto the NavMesh.")]
    [SerializeField, Min(0.1f)] private float NavMeshSampleRadius = 20f;

    private readonly List<GameObject> _alive = new();
    private WaitForSeconds _checkWait;

    private void Start()
    {
        if (EnemyPrefab == null || SpawnPoints == null || SpawnPoints.Length == 0)
        {
            Debug.LogError($"{name}: EnemyPrefab or SpawnPoints not assigned.", this);
            return;
        }

        _checkWait = new WaitForSeconds(CheckInterval);
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            // Wait for a free slot (an enemy is destroyed -> its reference becomes null).
            if (CountAlive() >= MaxAlive)
            {
                yield return _checkWait;
                continue;
            }

            SpawnOne();

            yield return new WaitForSeconds(Random.Range(MinDelay, MaxDelay));
        }
    }

    private void SpawnOne()
    {
        Transform point = SpawnPoints[Random.Range(0, SpawnPoints.Length)];

        // Snap the spawn point onto the NavMesh so the enemy starts on a walkable surface.
        if (!NavMesh.SamplePosition(point.position, out NavMeshHit hit, NavMeshSampleRadius, NavMesh.AllAreas))
        {
            Debug.LogWarning($"{name}: no NavMesh within {NavMeshSampleRadius}m of spawn point '{point.name}'. Skipped.", this);
            return;
        }

        GameObject enemy = Instantiate(EnemyPrefab, hit.position, point.rotation);
        _alive.Add(enemy);
    }

    // Counts living enemies and removes destroyed (null) ones from the list.
    private int CountAlive()
    {
        for (int i = _alive.Count - 1; i >= 0; i--)
        {
            if (_alive[i] == null)
            {
                _alive.RemoveAt(i);
            }
        }
        return _alive.Count;
    }
}
