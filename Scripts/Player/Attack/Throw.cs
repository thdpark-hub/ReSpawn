using FishNet.Object;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.LowLevel;
using UnityEngine.Windows;

public class Throw : NetworkBehaviour
{
    [Header("Reference")]
    [SerializeField] private PlayerLook playerLook;
    [SerializeField]private PlayerController playerController;

    private Vector2 _movementInput;
    private Rigidbody rigid;
    public bool throwRead = false;
    private void Awake() 
    {
        rigid = GetComponent<Rigidbody>();
    }

    private void OnAction(InputValue value)
    {
        if (value.isPressed && throwRead == true)
        {
            //애니메이션
            //던지기 메소드
        }
    }

    private void OnActOption(InputValue value)
    { 
        if (value.isPressed)
        {
            throwRead = true;
        }
        else
        {
            throwRead = false;
        }
    }
    private void ThrowRead()
    {
        float yaw = playerLook.GetYaw();
        Quaternion camRot = Quaternion.Euler(0f, yaw, 0f);
        transform.rotation = camRot;
        //애니메이션 추가
        //궤적 추가
    }

    private void FixedUpdate() 
    {
        if (throwRead)
        {
            ThrowRead();
        }
    }
}
