using UnityEngine;
using FishNet.Object;
using UnityEngine.InputSystem;
[RequireComponent(typeof(CharacterController))]
public class PlayerDash : NetworkBehaviour
{
    [Header("Dash Settings")]
    [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
    [SerializeField] private float dashDuration = 0.4f;
    [SerializeField] private float dashSpeed = 12f;

    [Header("Reference")]
    [SerializeField] private PlayerLook playerLook;

    private CharacterController _characterController;
    private Vector2 _movementInput;

    private bool _isRolling;
    private float _dashTimer;
    private Vector3 _dashDir;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
    }

    // Player Input(Send Messages)에서 호출됨
    private void OnMove(InputValue value)
    {
        _movementInput = value.Get<Vector2>();
    }

    private void OnDash(InputValue value)
    {
        if (!value.isPressed  || _isRolling) return;

        _dashDir = GetDashDirection();
        _dashTimer = 0f;
        _isRolling = true;
    }

    private Vector3 GetDashDirection()
    {
        // 입력이 없으면 캐릭터가 보는 방향으로
        if (_movementInput.sqrMagnitude < 0.01f)
            return transform.forward * -1;

        // 카메라 yaw 기준으로 입력 방향 변환
        Quaternion camRot = Quaternion.Euler(0f, playerLook.GetYaw(), 0f);
        Vector3 input = new Vector3(_movementInput.x, 0f, _movementInput.y).normalized;

        return camRot * input;
    }

    private void HandleRolling()
    {
        if (!_isRolling) return;

        _dashTimer += Time.deltaTime;
        float t = Mathf.Clamp01(_dashTimer / dashDuration);

        float speed = dashSpeed * animationCurve.Evaluate(t);
        _characterController.Move(_dashDir * speed * Time.deltaTime);

        if (_dashTimer >= dashDuration)
            _isRolling = false;
    }

    private void Update()
    {
        if (!IsOwner || _characterController == null) return;

        HandleRolling();
    }
}
