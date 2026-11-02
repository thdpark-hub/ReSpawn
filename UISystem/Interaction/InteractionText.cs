using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class InterActionText : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Vector3 textOffset = new Vector3(0f, 50f, 0f);

    private Camera targetCamera;
    private TextMeshProUGUI promptText;
    private IInteractable curInteractable;
    private RaycastHit hit;

    public void Initialize(Camera camera, TextMeshProUGUI text)                     //카메라와 UI를 초기화하기 위해 존재
    {
        targetCamera = camera;
        promptText = text;

        ClearPrompt();
    }

    public void TargetObjectText(RaycastHit hit)
    {
        if (targetCamera == null || promptText == null)
            return;

        IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();

        if (interactable == null)
        {
            ClearPrompt();
            return;
        }

        curInteractable = interactable;
        this.hit = hit;
        Vector3 screenPosition = targetCamera.WorldToScreenPoint(hit.point);

        if (screenPosition.z <= 0f)
        {
            ClearPrompt();
            return;
        }

        promptText.text = curInteractable.GetDescription(hit);
        promptText.rectTransform.position = screenPosition + textOffset;
        promptText.gameObject.SetActive(true);
    }
    void OnInterAction(InputValue value)
    {
        if (value.isPressed)
        {
            if(hit.collider != null)
            {
                if ((hit.collider.TryGetComponent<PlantObjectData>(out PlantObjectData plantData)) != null)
                {
                    curInteractable.PickUp();
                }
            }
        }
    }
    public void ClearPrompt()
    {
        curInteractable = null;

        promptText.text = null;
    }
}