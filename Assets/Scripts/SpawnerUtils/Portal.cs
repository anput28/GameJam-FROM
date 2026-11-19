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
            // randomPos sits on the ground, but the object's pivot may be above its feet
            // (controller center/height). Lift it so the controller rests on the surface
            // instead of sinking into the terrain.
            float pivotToFeet = objectToSpawn.transform.position.y - cc.bounds.min.y;
            Vector3 targetPos = randomPos + Vector3.up * (pivotToFeet + cc.skinWidth);

            cc.enabled = false;
            objectToSpawn.transform.position = targetPos;
            cc.enabled = true;
        }
    }
}
