using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class InterAction : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private Camera camera;
    [SerializeField]private float maxCheckDistance;
    [SerializeField]private LayerMask layerMask;
    private TextMeshProUGUI promptText;
    private GameObject currentObject;
    private ItemObjcetUI ItemObjcetUI;

    private IInteractable curInteractable;

    void Start()
    {
        camera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        RaycastHit raycastHit;

        if (Physics.Raycast(ray, out raycastHit, maxCheckDistance, layerMask))
        {
            currentObject = raycastHit.collider.gameObject;
            
            curInteractable = raycastHit.collider.GetComponent<IInteractable>();
            SetPromptText();
        }
        else
        {
            currentObject = null;
        }
    }

    private void SetPromptText()
    {
        promptText.gameObject.SetActive(true);
        promptText.text = curInteractable.GetDescription();
    }

    void OnInterAction(InputValue value)
    {
        
    }
}
