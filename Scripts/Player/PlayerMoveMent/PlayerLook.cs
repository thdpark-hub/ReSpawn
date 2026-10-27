using FishNet.Object;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLook : NetworkBehaviour
{
    [Header("Camera")]
    [SerializeField] private Transform cameraPivot;   // Yaw
    [SerializeField] private Transform cameraTarget;  // Pitch

    [SerializeField] private float mouseSensitivity = 0.1f;
    [SerializeField] private float minPitch = -80f;
    [SerializeField] private float maxPitch = 45f;

    private float _yaw;
    private float _pitch;

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!IsOwner)
        {
            enabled = false;
            return;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (!IsOwner) return;

        HandleLook();
    }

    private void HandleLook()
    {
        if (Mouse.current == null || cameraPivot == null || cameraTarget == null)
            return;

        Vector2 delta = Mouse.current.delta.ReadValue();

        float mouseX = delta.x * mouseSensitivity;
        float mouseY = delta.y * mouseSensitivity;

        _yaw += mouseX;
        _pitch -= mouseY;
        _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);

        // 좌우 회전 (플레이어 중심 공전)
        cameraPivot.rotation = Quaternion.Euler(0f, _yaw, 0f);

        // 상하 회전
        cameraTarget.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
    }

    public float GetYaw()
    {
        return _yaw;
    }
}