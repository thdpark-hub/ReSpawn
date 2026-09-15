using UnityEngine;
using UnityEngine.InputSystem;

public class Jump : MonoBehaviour
{
    private Rigidbody rigid;
    [SerializeField]private float JumpFroce = 5f;
    [SerializeField]private Transform groundcheck;
    [SerializeField]private float checkRadius = 0.3f;
    [SerializeField]private LayerMask groundLayer;
    private bool isGround = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rigid = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void OnJump(InputValue value)
    {
        if (value.isPressed && isGround)
        {
            rigid.AddForce(Vector3.up * JumpFroce, ForceMode.Impulse);
        }
    }
    void Update()
    {
        
    }
    void FixedUpdate()
    {
        isGround = Physics.Raycast(groundcheck.position, Vector3.down,checkRadius,groundLayer); //layermask를 통해서 ground를 체크하고 2단점프를 방지함 raycast방식을 이용하여 바닥 레이어를 체크함
    }
}
