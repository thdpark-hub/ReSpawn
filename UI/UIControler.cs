using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class UIControler : MonoBehaviour
{
    private Canvas canvas;
    [SerializeField]private GameObject OptionPanel;
    [SerializeField]private PlayerInput inputActions;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(inputActions == null)
        {
            inputActions = GetComponent<PlayerInput>();
        }
    }

    void OnUIstart(InputValue value)
    {
        if (value.isPressed)
        {
            OptionPanel.SetActive(true);
            if(inputActions != null)
            {
                if (!inputActions.inputIsActive)
                {
                    inputActions.ActivateInput();
                }
                inputActions.actions.Enable();
                inputActions.SwitchCurrentActionMap("UI");
            }
        }
    }    // Update is called once per frame
    void OnUIexit(InputValue value)
    {
        if (value.isPressed)
        {
            OptionPanel.SetActive(false);
            inputActions.SwitchCurrentActionMap("Player");
        }
    }
    void Update()
    {
        
    }
}
