using NUnit.Framework.Interfaces;
using UnityEngine;

public class PlantObjectData : MonoBehaviour
{
    // ScriptableObject 데이터를 참조하는 변수
    [SerializeField] private PlantData data;

    private void Start()
    {
        if (data != null)
        {
            Debug.Log($"아이템 이름: {data.PlantID}, 가격: {data.SeedID}");
        }
    }
}
