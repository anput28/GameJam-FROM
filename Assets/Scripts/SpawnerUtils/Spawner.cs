using UnityEngine;

public abstract class Spawner : MonoBehaviour
{ 
    [SerializeField, Min(1)] private int MaxRetries = 10;

    [Header("Position Validation")]
    [SerializeField, Min(0f)] protected float CheckRadius = 10.0f;
    [SerializeField] private LayerMask ObstacleMask;
    [Range(-2, 2)] [SerializeField] private float OffsetFromGround = 0f;

    protected float ObjectHalfHeight { get; set; }
    private BoxCollider SpawnArea;

    // Terrain cache variables
    private Terrain _terrain;
    private TerrainData _terrainData;
    private Vector3 _terrainSize;

    protected virtual void Awake()
    {
        SpawnArea = GameObject.FindGameObjectWithTag("SpawnArea").GetComponent<BoxCollider>();

        if (SpawnArea == null)
        {
            Debug.LogWarning("No 'SpawnArea' tag was found.");
        }

        SetTerrainInfo();
    }

    private void SetTerrainInfo()
    {
        _terrain = Terrain.activeTerrain;
        if (_terrain != null)
        {
            _terrainData = _terrain.terrainData;
            _terrainSize = _terrainData.size;
        }
        else
        {
            Debug.LogWarning("No active terrain found in the scene.");
        }
    }

    public bool TryGetValidPose(out Vector3 position, out Quaternion rotation)
    {
        for (int i = 0; i < MaxRetries; i++)
        {
            GetRandomPose(out position, out rotation);

            if (IsPositionValid(position))
            {
                return true;
            }
        }

        position = default;
        rotation = Quaternion.identity;
        return false;
    }

    // This method generates a random position and rotation within the spawn area, adjusted to the terrain height and normal.
    public void GetRandomPose(out Vector3 position, out Quaternion rotation)
    {
        if (SpawnArea == null)
        {
            Debug.LogError("SpawnArea is null.");
            position = Vector3.zero;
            rotation = Quaternion.identity;
            return;
        }

        Bounds bounds = SpawnArea.bounds;
        Vector3 center = bounds.center;
        Vector3 extents = bounds.extents;

        float x = center.x + Random.Range(-extents.x, extents.x);
        float z = center.z + Random.Range(-extents.z, extents.z);

        // Normalize x and z to terrain coordinates
        Vector3 origin = _terrain.transform.position;
        float nx = Mathf.Clamp01((x - origin.x) / _terrainSize.x);
        float nz = Mathf.Clamp01((z - origin.z) / _terrainSize.z);

        float groundY = _terrainData.GetInterpolatedHeight(nx, nz) + origin.y;
        Vector3 normal = _terrainData.GetInterpolatedNormal(nx, nz);

        // Adjust the position to be above the ground by the object's half height and any additional offset
        position = new Vector3(x, groundY, z) + normal * (ObjectHalfHeight + OffsetFromGround);

        // Set the rotation to align with the terrain normal
        rotation = Quaternion.FromToRotation(Vector3.up, normal);
    }

    public bool IsPositionValid(Vector3 position)
    {
        return !Physics.CheckSphere(position, CheckRadius, ObstacleMask, QueryTriggerInteraction.Collide);
    }

    public void SetSpawnArea(BoxCollider area)
    {
        SpawnArea = area;
    }

    public BoxCollider GetSpawnArea() => SpawnArea;

    public abstract void PerformSpawn(GameObject objectToSpawn = null);
}
