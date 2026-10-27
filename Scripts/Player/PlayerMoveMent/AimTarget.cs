using FishNet.Object;
using UnityEngine;

public class InteractionPoint : NetworkBehaviour
{
    [SerializeField] private Camera mainCamera;

    // FishNet 환경에서는 Start 대신 OnStartClient()를 사용하는 것이 좋습니다.
    public override void OnStartClient()
    {
        base.OnStartClient();

        // 메인 권한(내 캐릭터)이 있는 클라이언트만 카메라를 로드
        if (IsOwner)
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main; // Scene의 MainCamera 태그가 붙은 카메라를 가져옴
            }
        }
    }

    void Update()
    {
        // 로컬 플레이어(내 캐릭터)가 아니거나 카메라가 없으면 레이캐스트를 실행하지 않음
        if (!IsOwner || mainCamera == null) return;

        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f))
        {
            Debug.Log("Hit object: " + hit.collider.name);
            // Target = hit.collider.GetComponent<TestInvetoryObject>();
        }
    }
}