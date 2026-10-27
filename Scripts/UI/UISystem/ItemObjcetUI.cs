using UnityEngine;

public class ItemObjcetUI : MonoBehaviour, IInteractable
{
    public InterfaceObject Object;
    public void getInterAction()
    {
        Debug.Log("test");
    }

    public string GetDescription()
    {
        string str = $"{Object.objectName}\n{Object.description}";
        return str;
    }
}

public interface IInteractable
{
    public string GetDescription(); // 화면에 띄워줄 프롬프트
    public void getInterAction(); // 인터렉트 됐을때 어떻게 할건지
}        

// 코드 수정 필요 => 인터넷에서 퍼온거라 우선은 테스트로 사용