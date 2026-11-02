using UnityEngine;
using UnityEngine.InputSystem;

public class InterActionObject : MonoBehaviour
{
    private InterActionController interActionController;
    private IInteractable curInteractable;
    private GameObject curObject;
    void OnInterAction(InputValue value)
    {
        if (value.isPressed)
        {
            
        }
    }
    public void GetObjectRaycast(RaycastHit hit)
    {
        curInteractable = hit.collider.GetComponentInParent<IInteractable>();
        curObject = hit.collider.gameObject;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
