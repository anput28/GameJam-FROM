using System.Collections.Generic;
using UnityEngine;


public class ObjectsSpawner : Spawner
{
    [Header("Object Info")]
    [SerializeField] private GameObject ObjectPrefab;
    [SerializeField] private int ObjectCount = 10;

    private readonly List<Vector3> _spawnedPositions = new();

    private void Awake()
    {
        if (ObjectPrefab != null)
        {
            if (ObjectPrefab.TryGetComponent(out Renderer rend))
                ObjectHalfHeight = rend.bounds.extents.y;
            else
                ObjectHalfHeight = ObjectPrefab.transform.localScale.y / 2f;
        }
    }

    private void Start()
    {
        PerformSpawn();
    }

    public override void PerformSpawn(GameObject objectToSpawn = null)
    {
        if (ObjectPrefab == null)
        {
            Debug.LogError($"{name}: ObjectPrefab non assegnato.", this);
            return;
        }

        for (int i = 0; i < ObjectCount; i++)
        {
            if (!TryGetValidPosition(out Vector3 randomPosition))
            {
                Debug.LogWarning($"Spawn {i} failed.");
                continue;
            }

            GameObject instance = Instantiate(ObjectPrefab, randomPosition, Quaternion.identity);
            if (instance.TryGetComponent(out Spawner child))
            {
                child.SetSpawnArea(GetSpawnArea());
            }

            // Ensure physics updates after instantiation, without this, the next position check may not account for the newly spawned object.
            Physics.SyncTransforms();

            _spawnedPositions.Add(randomPosition);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        if (_spawnedPositions != null)
        {
            foreach (var pos in _spawnedPositions)
            {
                Gizmos.DrawWireSphere(pos, CheckRadius);
            }
        }
    }
}
