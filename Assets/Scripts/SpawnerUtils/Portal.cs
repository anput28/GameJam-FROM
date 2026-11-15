using UnityEngine;

public class Portal : Spawner
{
    private void OnTriggerEnter(Collider other)
    {
        PerformSpawn(other.gameObject);
    }

    public override void PerformSpawn(GameObject objectToSpawn)
    {
        if (!TryGetValidPose(out Vector3 randomPos, out _))
        {
            Debug.LogWarning($"teleportation of object {objectToSpawn} failed.");
            return;
        }

        if (objectToSpawn.TryGetComponent(out CharacterController cc))
        {
            cc.enabled = false;
            objectToSpawn.transform.position = randomPos;
            cc.enabled = true;
        }
    }
}
