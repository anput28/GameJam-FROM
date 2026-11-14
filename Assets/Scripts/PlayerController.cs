using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(InputReader))]
public class PlayerController : MonoBehaviour
{
    [Header("Look")]
    [SerializeField] private Transform CameraTransform;
    [SerializeField] private float MouseSensitivity = 0.1f;
    [SerializeField] private float GamePadSensitivity = 150f;
    [SerializeField] private float MaxVerticalAngle = 85f;

    [Header("Movement")]
    [SerializeField] private float Speed = 5f;
    [SerializeField] private float SprintSpeed = 15f;
    [SerializeField] private float SprintDuration = 5f;
    [SerializeField] private float Gravity = -15f; // The real gravity is 9.81 m/s², but we can exaggerate it for gameplay purposes

    [Header("UI")]
    [SerializeField] private Image SprintImage;

    // Movement variables
    private float _lookRotation = 0f;
    private float _sprintTimeElapsed = 0f;
    private Vector3 _velocity = Vector3.zero;
    private bool _isExhausted = false;
    private bool _isGrounded = false;

    // Amulet collection variables
    private Amulet _collectibleAmulet = null;
    private int _amuletCount = 0;

    private CharacterController _cc;
    private InputReader _inputReader;
    
    public static event Action<Amulet, int> OnAmuletCollected;

    #region Unity Lifecycle Hooks
    private void Awake()
    {
        _cc = GetComponent<CharacterController>();
        _inputReader = GetComponent<InputReader>();

        LockCursor();

        if (SprintImage != null)
        {
            SprintImage.fillAmount = 0f;
        }
    }

    private void Update()
    {
        Look();
        Move();
        CollectAmulet();
    }

    private void OnApplicationFocus(bool focus)
    {
        if (focus && enabled)
        {
            LockCursor();
        }
    }

    private void OnEnable()
    {
        Amulet.OnPlayerEntered += SetCollectibleAmulet;
        Amulet.OnPlayerExited += ForgetCollectibleAmulet;
    }

    private void OnDisable()
    {
        Amulet.OnPlayerEntered -= SetCollectibleAmulet;
        Amulet.OnPlayerExited -= ForgetCollectibleAmulet;
    }

    #endregion

    private void Look()
    {
        Vector2 lookInput = _inputReader.IsLookFromGamepad
            ? GamePadSensitivity * Time.deltaTime * _inputReader.LookInput
            : _inputReader.LookInput * MouseSensitivity;

        // Update the vertical rotation (x-axis)
        _lookRotation -= lookInput.y;
        _lookRotation = Mathf.Clamp(_lookRotation, -MaxVerticalAngle, MaxVerticalAngle);
        CameraTransform.localRotation = Quaternion.Euler(_lookRotation, 0f, 0f);

        // Update the horizontal rotation (y-axis)
        transform.Rotate(Vector3.up * lookInput.x);
    }

    #region Movement

    private void Move()
    {
        // Calculate movement direction
        Vector2 moveInput = _inputReader.MoveInput;
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        move = Vector3.ClampMagnitude(move, 1f); // Normalize diagonal movement

        UpdateStamina();
        float speed = CanSprint() ? SprintSpeed : Speed;

        // Apply gravity only to the vertical component of the velocity
        Vector3 newVelocity = move * speed;
        newVelocity.y = ApplyGravity();

        // Move the character controller
        CollisionFlags flags = _cc.Move(newVelocity * Time.deltaTime);

        // Update grounded state based on collision flags (more reliable than _cc.isGrounded)
        _isGrounded = (flags & CollisionFlags.Below) != 0;

        // Reset vertical velocity if hitting the ceiling
        if ((flags & CollisionFlags.Above) != 0 && _velocity.y > 0f)
        {
            _velocity.y = 0f; 
        }
    }

    private bool CanSprint()
    {
        bool isMoving = _inputReader.MoveInput.magnitude > 0.1f; // Check if the player is moving
        return _inputReader.IsSprinting && isMoving && !_isExhausted;
    }

    private void UpdateStamina()
    {
        float dt = Time.deltaTime;

        if (CanSprint())
        {
            _sprintTimeElapsed += dt;

            // Handle exhaustion
            if (_sprintTimeElapsed >= SprintDuration)
            {
                _sprintTimeElapsed = SprintDuration;
                _isExhausted = true;
            }
        }
        else if (_sprintTimeElapsed > 0)
        {
            // Decrease the sprint time elapsed while recharging
            _sprintTimeElapsed = Mathf.Max(0f, _sprintTimeElapsed - dt);
            _isExhausted = false;
        }

        UpdateSprintUI();
    }

    private float ApplyGravity()
    {
        if (_isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f; // Small negative value to keep the player grounded
        }
        _velocity.y += Gravity * Time.deltaTime;
        return _velocity.y;
    }

    #endregion
    
    private void CollectAmulet()
    {
        if (_inputReader.IsInteracting && _collectibleAmulet != null)
        {          
            _amuletCount++;
            OnAmuletCollected?.Invoke(_collectibleAmulet, _amuletCount);
            _collectibleAmulet = null;
        }
    }

    #region Utils

    private void LockCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    #endregion

    #region UI methods
    private void UpdateSprintUI()
    {
        if (SprintImage != null)
        {
            float fillAmount = _sprintTimeElapsed / SprintDuration;
            SprintImage.fillAmount = Mathf.Clamp01(fillAmount);
        }
    }
    #endregion

    #region Amulet Event Handlers
    private void SetCollectibleAmulet(Amulet amulet)
    {
        _collectibleAmulet = amulet;
    }

    private void ForgetCollectibleAmulet(Amulet amulet)
    {
        if (_collectibleAmulet == amulet)
        {
            _collectibleAmulet = null;
        }
    }
    #endregion
}
