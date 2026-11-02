using System;
using UnityEngine;

[CreateAssetMenu(fileName = "InterfaceObject", menuName = "Scriptable Objects/InterfaceObject")]
public class InterfaceObject : ScriptableObject
{
    [field: SerializeField] public int ID { get; private set; }
    [field: SerializeField] public string objectName { get; private set; }
    [field: SerializeField] public string description { get; private set; }
    [field: SerializeField] public string objectIcon { get; private set; }
}
public interface IInteractable
{
    public string GetDescription(RaycastHit hit); // 화면에 띄워줄 프롬프트
    public void getInterAction(); // 인터렉트 됐을때 어떻게 할건지
    public void PickUp();
}

// 코드 수정 필요 => 인터넷에서 퍼온거라 우선은 테스트로 사용