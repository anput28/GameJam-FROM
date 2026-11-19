using System.Collections.Generic;
using UnityEngine;


public class ObjectsSpawner : Spawner
{
    [Header("Object Info")]
    [SerializeField] private GameObject ObjectPrefab;
    [SerializeField] private int ObjectCount = 10;

    private readonly List<Vector3> _spawnedPositions = new();

    // Adds a minimum-distance check against already-spawned objects, on top of the base obstacle/player checks.
    public override bool IsPositionValid(Vector3 position)
    {
        if (!base.IsPositionValid(position))
        {
            return false;
        }

        for (int i = 0; i < _spawnedPositions.Count; i++)
        {
            if ((position - _spawnedPositions[i]).sqrMagnitude < CheckRadius * CheckRadius)
            {
                return false;
            }
        }

        return true;
    }

    protected override void Awake()
    {
        base.Awake();
        if (ObjectPrefab != null)
        {
            if (ObjectPrefab.TryGetComponent(out Renderer rend))
                ObjectHalfHeight = rend.bounds.extents.y;
            else
                ObjectHalfHeight = ObjectPrefab.transform.localScale.y / 2f;
        }

        PerformSpawn();
    }

    public override void PerformSpawn(GameObject objectToSpawn = null)
    {
        if (ObjectPrefab == null)
        {
            Debug.LogError($"{name}: ObjectPrefab not assigned.", this);
            return;
        }

        for (int i = 0; i < ObjectCount; i++)
        {
            if (!TryGetValidPose(out Vector3 randomPos, out Quaternion randomRot))
            {
                Debug.LogWarning($"Spwan {i} of object {ObjectPrefab} failed");
                continue;
            }

            GameObject instance = Instantiate(ObjectPrefab, randomPos, randomRot);
            if (instance.TryGetComponent(out Spawner child))
            {
                child.SetSpawnArea(GetSpawnArea());
            }

            // Ensure physics updates after instantiation, without this, the next position check may not account for the newly spawned object.
            Physics.SyncTransforms();

            _spawnedPositions.Add(randomPos);
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
