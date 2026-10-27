using UnityEngine;
using FishNet.Object;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float baseSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 1.5f;

    [Header("Reference")]
    [SerializeField] private PlayerLook playerLook;
    [SerializeField]private Throw Throw;

    private CharacterController _characterController;
    private Vector2 _movementInput;
    private Vector3 _velocity;

    private bool _isJumpPressed;
    private float _currentSpeed;

    private void Awake()
    {
        Throw = GetComponent<Throw>();
        _characterController = GetComponent<CharacterController>();
        _currentSpeed = baseSpeed;
    }

    private void Update()
    {
        if (!IsOwner || _characterController == null) return;

        HandleGround();
        HandleGravity();
        HandleMovement();
        ApplyMovement();
    }
    private void OnMove(InputValue value)
    {
        if (!IsOwner) return;
        _movementInput = value.Get<Vector2>();
        Debug.Log("움직임");
    }

    private void OnRuning(InputValue value)
    {
        _currentSpeed = value.isPressed ? runSpeed : baseSpeed;
    }

    private void OnJump(InputValue value)
    {
        if (!IsOwner) return;

        if (value.isPressed)
            _isJumpPressed = true;
    }

    // ---------------- 물리 ----------------

    private void HandleGround()
    {
        if (_characterController.isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f;
        }
    }

    private void HandleGravity()
    {
        _velocity.y += gravity * Time.deltaTime;
    }

    // ---------------- 이동 ----------------

    private void HandleMovement()
    {
        // 카메라 기준 입력
        Vector3 input = new Vector3(_movementInput.x, 0, _movementInput.y);

        if (input.magnitude > 1)
            input.Normalize();

        // 카메라 yaw 가져오기
        float yaw = playerLook.GetYaw();
        Quaternion camRot = Quaternion.Euler(0f, yaw, 0f);

        Vector3 moveDir = camRot * input;

        // 캐릭터를 이동 방향으로 회전
        if (moveDir != Vector3.zero && Throw.throwRead == false)
        {
            transform.forward = moveDir;
        }

        // 점프
        if (_isJumpPressed && _characterController.isGrounded)
        {
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        _isJumpPressed = false;

        // 수평 이동
        _velocity.x = moveDir.x * _currentSpeed;
        _velocity.z = moveDir.z * _currentSpeed;
    }

    // ---------------- 적용 ----------------

    private void ApplyMovement()
    {
        _characterController.Move(_velocity * Time.deltaTime);
    }
}