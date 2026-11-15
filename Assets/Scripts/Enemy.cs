using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Movement")]
    [SerializeField] private Transform Target;
    [SerializeField, Min(1)] private int MinSpeed;
    [SerializeField, Min(1)] private int MaxSpeed;

    [Header("Enemy Damage")]
    [SerializeField, Min(1)] private int Damage = 30;
    [SerializeField] private float damageInterval = 1f;

    private float _currentSpeed;
    private float _nextDamageTime;
    private Rigidbody _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();

        if (MinSpeed > MaxSpeed)
        {
            Debug.LogWarning($"Enemy {name}: MinSpeed is greater than MaxSpeed. Adjusting MinSpeed to be equal to MaxSpeed.");
            MinSpeed = MaxSpeed;
        }

        // Random.Range with int parameters excludes the max value
        _currentSpeed = Random.Range((float)MinSpeed, MaxSpeed);
    }

    private void FixedUpdate()
    {
        if (Target == null) return;
        
        Vector3 targetPosition = new(Target.position.x, _rb.position.y, Target.position.z);
        Vector3 newPos = Vector3.MoveTowards(_rb.position, targetPosition, _currentSpeed * Time.fixedDeltaTime);
        _rb.MovePosition(newPos);
    }

    private void OnCollisionStay(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        if (Time.time >= _nextDamageTime)
        {
            if (collision.gameObject.TryGetComponent(out PlayerController player))
            {
                player.TakeDamage(Damage);
                _nextDamageTime = Time.time + damageInterval;
            }
        }

    }
}
