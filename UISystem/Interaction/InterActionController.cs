using System.Globalization;
using TMPro;
using UnityEngine;
using FishNet.Object;

public class InterActionController : NetworkBehaviour
{
    [Header("Detection")]
    [SerializeField] private float maxCheckDistance = 3f;
    [SerializeField] private LayerMask layerMask = ~0;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI promptText;

    private Camera targetCamera;
    private InterActionText interActionText;
    private InterActionObject interActionObject;

    public override void OnStartClient()
    {
        base.OnStartClient();

        // 로컬 플레이어가 아니면 상호작용 UI를 제어하지 않는다.
        if (!IsOwner)
        {
            enabled = false;
            return;
        }

        // 프리팹이 생성된 후 자신의 카메라를 찾는다.
        targetCamera = GetComponentInChildren<Camera>(true);

        if (targetCamera == null)
        {
            targetCamera = Camera.main;                                 //카메라 할당
        }

        interActionText = GetComponent<InterActionText>();
        if (interActionText == null)
        {
            Debug.LogError("InterActionText 컴포넌트를 찾을 수 없습니다.", this);
            enabled = false;
            return;
        }
        interActionObject = GetComponent<InterActionObject>();
        if(interActionObject == null)
        {
            Debug.LogError("InterActionObject 컴포넌트를 찾을 수 없습니다.", this);
            enabled = false;
            return;
        }

        // 씬에 존재하는 UI를 찾는다.
        if (promptText == null)
        {
            promptText = FindFirstObjectByType<TextMeshProUGUI>();      //텍스트 출력 ui할당
        }

        if (targetCamera == null)
        {
            Debug.LogError("로컬 플레이어의 카메라를 찾을 수 없습니다.", this);
            enabled = false;
            return;
        }

        if (promptText == null)
        {
            Debug.LogError("상호작용 UI를 찾을 수 없습니다.", this);
            enabled = false;
            return;
        }
        interActionText.Initialize(targetCamera, promptText);               //카메라와 텍스트ui를 현재 스크립트에서 찾아 text스크립트를 초기화시킴
        
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
     
    }
    
    // Update is called once per frame
    void Update()
    {
        if (!IsOwner || targetCamera == null || promptText == null)
        {
            return;
        }

        Ray ray = targetCamera.ViewportPointToRay(                  //viewpoint설정
            new Vector3(0.5f, 0.5f, 0f)
        );

        if (Physics.Raycast(ray, out RaycastHit hit, maxCheckDistance, layerMask))          //포인트 조건 설정         layermask 쉬프연산으로 종류별로 확인 가능
        {
            interActionText.TargetObjectText(hit);
            
        }
        else
        {
            interActionText.ClearPrompt();
        }
    }
}
