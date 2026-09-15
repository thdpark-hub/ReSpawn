using UnityEngine;
using UnityEngine.InputSystem;

public class Move : MonoBehaviour
{
    private Rigidbody rigid;
    private Vector2 move;
    [SerializeField]private float movespeed = 5f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rigid = GetComponent<Rigidbody>();
    }

    void OnMove(InputValue value)
    {
        move = value.Get<Vector2>();
    }
    // Update is called once per frame
    void Update()
    {
        rigid.linearVelocity = new Vector3(move.x * movespeed, rigid.linearVelocity.y, move.y * movespeed);
    }
}
