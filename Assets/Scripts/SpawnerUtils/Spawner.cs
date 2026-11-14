using UnityEngine;

public abstract class Spawner : MonoBehaviour
{ 
    [SerializeField, Min(1)] private int MaxRetries = 5;

    [Header("Position Validation")]
    [SerializeField, Min(0f)] protected float CheckRadius = 100.0f;
    [SerializeField] private LayerMask ObstacleMask;
    [Range(-1, 0)] [SerializeField] private float OffsetFromGround = -0.5f;

    protected float ObjectHalfHeight { get; set; }
    private BoxCollider SpawnArea;

    private void OnValidate()
    {
        SpawnArea = GameObject.FindGameObjectWithTag("SpawnArea").GetComponent<BoxCollider>();

        if (SpawnArea == null)
        {
            Debug.LogWarning("No 'SpawnArea' tag was found.");
        }
    }

    public bool TryGetValidPosition(out Vector3 position)
    {
        for (int i = 0; i < MaxRetries; i++)
        {
            position = GetRandomPosition();
            if (IsPositionValid(position))
            {
                return true;
            }
        }

        position = default;
        return false;
    }

    public bool IsPositionValid(Vector3 position)
    {
        return !Physics.CheckSphere(position, CheckRadius, ObstacleMask, QueryTriggerInteraction.Collide);
    }

    public Vector3 GetRandomPosition()
    {
        Bounds bounds = SpawnArea.bounds;
        float x = Random.Range(bounds.min.x, bounds.max.x);
        float z = Random.Range(bounds.min.z, bounds.max.z);

        // Adjust y based on terrain and object's height  
        float y = 0;
  
        Terrain terrain = Terrain.activeTerrain;
        y = terrain.SampleHeight(new Vector3(x, 0, z)) + terrain.transform.position.y;
        y += ObjectHalfHeight + OffsetFromGround;

        return new Vector3(x, y, z);
    }

    public void SetSpawnArea(BoxCollider area)
    {
        SpawnArea = area;
    }

    public BoxCollider GetSpawnArea() => SpawnArea;

    public abstract void PerformSpawn(GameObject objectToSpawn = null);
}
