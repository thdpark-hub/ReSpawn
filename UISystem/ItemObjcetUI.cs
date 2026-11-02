using UnityEngine;

public class ItemObjcetUI : MonoBehaviour, IInteractable
{
    public InterfaceObject Object;
    public void getInterAction()
    {
        Debug.Log("test");
    }
    public void PickUp()
    {
        Destroy(gameObject);
        return;
    }
    public string GetDescription(RaycastHit hit)
    {
        string str = $"{Object.objectName}\n{Object.description}";
        return str;
    }
}