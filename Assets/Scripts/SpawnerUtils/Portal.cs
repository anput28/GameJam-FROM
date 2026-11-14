using UnityEngine;

public class Portal : Spawner
{
    private void OnTriggerEnter(Collider other)
    {
        PerformSpawn(other.gameObject);
    }

    public override void PerformSpawn(GameObject objectToSpawn)
    {
        if (!TryGetValidPosition(out Vector3 randomPosition))
        {
            Debug.LogWarning($"teleportation of object {objectToSpawn} failed.");
            return;
        }

        if (objectToSpawn.TryGetComponent(out CharacterController cc))
        {
            cc.enabled = false;
            objectToSpawn.transform.position = randomPosition;
            cc.enabled = true;
        }
    }
}
