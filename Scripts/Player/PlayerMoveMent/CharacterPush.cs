using FishNet.Object;
using UnityEngine;

public class CharacterPush : NetworkBehaviour
{
    [Header("Push Settings")]
    [SerializeField] private float pushPower = 2.0f; // 밀어내는 힘의 크기

    // CharacterController가 다른 Collider와 충돌할 때 자동으로 호출됨
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;

        if (body == null || body.isKinematic)
            return;

        if (hit.moveDirection.y < -0.7f)
            return;

        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0f, hit.moveDirection.z);

        if (pushDir.sqrMagnitude < 0.001f)
            return;

        // 작은 물체 → 안정적인 밀기
        if (body.mass < 2f)
        {
            Vector3 targetVel = pushDir * pushPower;

            body.linearVelocity = new Vector3(
                targetVel.x,
                body.linearVelocity.y,
                targetVel.z
            );
        }
        else
        {
            // 큰 물체 → 자연스러운 물리
            body.AddForce(pushDir * pushPower, ForceMode.Force);
        }

        // 안정화
        body.linearVelocity = Vector3.ClampMagnitude(body.linearVelocity, 4f);
        body.angularVelocity = Vector3.ClampMagnitude(body.angularVelocity, 3f);
    }

}