
using UnityEngine;

[CreateAssetMenu(fileName = "SeedData_", menuName = "Scriptable Objects/SeedData")]
public class SeedData : InterfaceObject
{
    //[field: SerializeField]로 인스펙터 수정 허용 + 외부 C# 읽기 전용 캡슐화
    [field: SerializeField] public int seedID { get; private set; }
    [field: SerializeField] public int plantType { get; private set; }
    [field: SerializeField] public int amount { get; private set; }
    [field: SerializeField] public float growTime { get; private set; }
    [field: SerializeField] public float waterTime { get; private set; }
    [field: SerializeField] public float worldSize { get; private set; }                    //여분 씨앗 생성갯수로 할 수도
    [field: SerializeField] public float createAmount { get; private set; }

    // 에디터에서 데이터 유효성 검사 (Pre-build Data Validation)a
    private void OnValidate()
    {
        if (seedID <= 0)
        {
            Debug.LogWarning($"[{name}] PlantID가 올바르지 않습니다. (0 이하)", this);
        }
    }
}