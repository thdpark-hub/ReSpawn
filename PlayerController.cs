using UnityEngine;
using FishNet.Object;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5.0f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 1.5f; // 점프 높이 설정
    [SerializeField] private float RuningForce = 3.0f;

    [Header("Look")]
    [Tooltip("Pitch(상하 시선)를 담당하는 자식 오브젝트. VCam은 이 밑에 자식으로 둡니다.")]
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private float mouseSensitivity = 0.1f; // 0.05~0.2 사이에서 취향껏 조절
    [SerializeField] private float minPitch = -80f;
    [SerializeField] private float maxPitch = 45f;

    private float _pitch;

    private CharacterController _characterController;
    private Vector2 _movementInput; // Vector2로 변경
    private Vector3 _velocity;
    private bool _isJumpPressed = false;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
    }
    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!base.IsOwner)
        {
            enabled = false;
            return;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // 이동 입력 (Vector2로 수정)
    private void OnMove(InputValue value)
    {
        if (!base.IsOwner) return;
    
        _movementInput = value.Get<Vector2>();
    }
    private void OnRuning(InputValue value)
    {
        if (value.isPressed)
        {
            Debug.Log(moveSpeed);
            moveSpeed = moveSpeed + RuningForce;
        }
        else
        {
            Debug.Log(moveSpeed);
            moveSpeed = 5f;
        }
    }

    // 점프 입력
    private void OnJump(InputValue value)
    {
        if (!base.IsOwner) return;

        if (value.isPressed)
        {
            Debug.Log("점프");
            _isJumpPressed = true;
        }
    }


    private void Update()
    {
        if (!base.IsOwner || _characterController == null) return;

        HandleMovement();
        HandleLook();
    }

    private void HandleLook()
    {
        if (Mouse.current == null || cameraTarget == null) return;

        Vector2 delta = Mouse.current.delta.ReadValue();
        float mouseX = delta.x * mouseSensitivity;
        float mouseY = delta.y * mouseSensitivity;

        // 몸통(Yaw) 회전: 루트를 회전시켜서 다른 클라이언트도 몸이 도는 걸 볼 수 있게 함
        // (NetworkTransform이 이 회전값을 다른 클라이언트에 동기화함)
        transform.Rotate(Vector3.up * mouseX);

        // 카메라(Pitch) 회전: 로컬 전용, CameraTarget만 회전
        _pitch -= mouseY;
        _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);
        cameraTarget.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
    }

    private void HandleMovement()
    {
        // 1. 바닥 체크 및 중력 초기화
        if (_characterController.isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f; // 바닥에 붙어있도록 살짝 음수 유지
        }

        // 2. 이동 방향 계산 (WASD 평면 이동)
        Vector3 moveDirection = transform.right * _movementInput.x + transform.forward * _movementInput.y;

        // 3. 점프 처리
        if (_isJumpPressed && _characterController.isGrounded)
        {
            // 물리 공식: v = sqrt(height * -2 * gravity)
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        _isJumpPressed = false; // 점프 입력 초기화

        // 4. 중력 누적 계산
        _velocity.y += gravity * Time.deltaTime;

        // 5. 수평 이동 + 수직 이동(중력/점프)을 합쳐서 최종 이동 처리
        Vector3 finalVelocity = (moveDirection * moveSpeed) + new Vector3(0, _velocity.y, 0);
        _characterController.Move(finalVelocity * Time.deltaTime);
    }
}