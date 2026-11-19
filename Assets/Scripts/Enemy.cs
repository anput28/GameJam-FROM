using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(Rigidbody))]
public class Enemy : MonoBehaviour
{
    private enum State { WaitingForNavMesh, Chasing, Grabbed, Flying }

    [Header("Enemy Movement")]
    [SerializeField] private Transform Target;
    [SerializeField, Min(1)] private int MinSpeed;
    [SerializeField, Min(1)] private int MaxSpeed;
    [SerializeField] private float RepathInterval = 0.2f;

    [Header("Enemy Damage")]
    [SerializeField] private float AttackRange = 1.5f;
    [SerializeField, Min(1)] private int Damage = 30;
    [SerializeField] private float DamageInterval = 1f;

    [Header("NavMesh")]
    [SerializeField] private float NavMeshSearchRadius = 5f;
    [SerializeField] private float NavMeshPollInterval = 0.25f;
    [SerializeField] private float NavMeshWaitTimeout = 10f;

    [Header("Throw")]
    [SerializeField] private float MaxFlightTime = 5f;
    [Tooltip("After the launch, collisions are ignored for this duration to prevent the enemy from destroying itself against the player or the hand holding it.")]
    [SerializeField] private float ImpactGracePeriod = 0.1f;

    [Header("Shockwave")]
    [SerializeField] private float ShockwaveRadius = 6f;
    [SerializeField] private LayerMask ShockwaveMask = ~0;

    public float AttackReach => AttackRange;

    private bool _hasImpacted; // Flag to avoid multiple shockwave triggers on the same impact
    private float _flightStartTime;
    private float _nextDamageTime;
    private float _nextRepathTime;

    private State _state = State.WaitingForNavMesh;
    private NavMeshAgent _navMeshAgent;
    private Rigidbody _rb;
    private PlayerController _player;

    #region  Unity Lifecycle Hooks

    private void Awake()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _navMeshAgent.enabled = false;

        _rb = GetComponent<Rigidbody>();
        _rb.isKinematic = true;

        if (MinSpeed > MaxSpeed)
        {
            Debug.LogWarning($"Enemy {name}: MinSpeed is greater than MaxSpeed. Adjusting MinSpeed to be equal to MaxSpeed.");
            MinSpeed = MaxSpeed;
        }

        // Random.Range with int parameters excludes the max value
        _navMeshAgent.speed = Random.Range((float)MinSpeed, MaxSpeed);
    }

    private void Start()
    {
        ResolveTarget();
        StartCoroutine(WaitForNavMeshRoutine());
    }

    private void Update()
    {
        switch (_state)
        {
            case State.Chasing:
                UpdateChasing();
                break;

            case State.Grabbed:
                TryAttack();
                break;

            case State.Flying:
                UpdateFlying();
                break;
            
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandleFlyingCollision();
    }
    private void OnCollisionStay(Collision collision) => HandleFlyingCollision();

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, ShockwaveRadius);
    }
    #endregion

    #region Initialization
    private void ResolveTarget()
    {
        if (Target == null)
        {
            _player = FindAnyObjectByType<PlayerController>();
            if (_player != null)
            {
                Target = _player.transform;
            }
        }
        else
        {
            Target.TryGetComponent(out _player);
        }

        if (Target == null)
        {
            Debug.LogError($"Enemy {name}: No target assigned and no GameObject with tag 'Player' found.");
        }

    }

    private IEnumerator WaitForNavMeshRoutine()
    {
        var wait = new WaitForSeconds(NavMeshPollInterval);
        float startTime = Time.time;

        while (true)
        {
            if (TrySnapToNavMesh())
            {
                _state = State.Chasing;
                yield break;
            }

            if (Time.time - startTime > NavMeshWaitTimeout)
            {
                Debug.LogError($"Enemy {name}: Failed to find a valid NavMesh position within the timeout period.");
                Destroy(gameObject);
                yield break;
            }

            yield return wait;
        }
    }

    #endregion

    #region Attack Logic
    private void UpdateChasing()
    {
        if (Target == null || !_navMeshAgent.enabled || !_navMeshAgent.isOnNavMesh) return;

        // Repath to the target at intervals
        if (Time.time >= _nextRepathTime)
        {
            _navMeshAgent.SetDestination(Target.position);
            _nextRepathTime = Time.time + RepathInterval;
        }

        TryAttack();
    }

    private void TryAttack()
    {
        if (_player == null || Time.time < _nextDamageTime) return;

        Vector3 delta = Target.position - transform.position;
        delta.y = 0f;

        if (delta.sqrMagnitude <= AttackRange * AttackRange)
        {
            _player.TakeDamage(Damage);
            _nextDamageTime = Time.time + DamageInterval;
        }
    }
    #endregion

    #region Flying Logic
    private void UpdateFlying()
    {
        if (Time.time - _flightStartTime > MaxFlightTime)
        {
            Destroy(gameObject);
        }
    }

    private void HandleFlyingCollision()
    {
        if (_state != State.Flying) return;
        if (Time.time - _flightStartTime < ImpactGracePeriod) return;

        if (_hasImpacted) return;
        _hasImpacted = true;

        Sockwave();
        Destroy(gameObject);
    }

    private void Sockwave()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, ShockwaveRadius, ShockwaveMask, QueryTriggerInteraction.Ignore);
        foreach (var collider in colliders)
        {
            Enemy other = collider.GetComponentInParent<Enemy>();
            if (other != null && other != this)
            {
                Destroy(other.gameObject);
            }
        }
    }
    #endregion

    #region NavMesh Helpers
    private bool TrySnapToNavMesh()
    {
        if (!NavMesh.SamplePosition(transform.position, out NavMeshHit hit, NavMeshSearchRadius, NavMesh.AllAreas))
        {
            return false;
        }

        // Snap the enemy to the NavMesh position
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _rb.isKinematic = true;

        transform.SetPositionAndRotation(hit.position, Quaternion.Euler(0f, transform.eulerAngles.y, 0f));

        // Enable the NavMeshAgent and warp it to the snapped position
        _navMeshAgent.enabled = true;
        _navMeshAgent.Warp(hit.position);
        return true;

    }
    #endregion

    #region Grab & Throw (API for PlayerController)

    public bool Grab()
    {
        if (_state != State.Chasing) return false;

        _state = State.Grabbed;

        if (_navMeshAgent.enabled) _navMeshAgent.ResetPath();
        _navMeshAgent.enabled = false;
        _rb.isKinematic = true;
        return true;
    }

    public void Throw(Vector3 velocity)
    {
        _state = State.Flying;
        _flightStartTime = Time.time;

        _navMeshAgent.enabled = false;
        _rb.isKinematic = false;
        _rb.linearVelocity = velocity;
    }

    #endregion
}
