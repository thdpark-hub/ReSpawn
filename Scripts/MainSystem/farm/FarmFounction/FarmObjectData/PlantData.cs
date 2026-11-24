using UnityEngine;

[CreateAssetMenu(fileName = "PlantData_", menuName = "Scriptable Objects/PlantData")]
public class PlantData : InterfaceObject
{
    // 현업 스펙: [field: SerializeField]로 인스펙터 수정 허용 + 외부 C# 읽기 전용 캡슐화
    [field: SerializeField] public int PlantID { get; private set; }
    [field: SerializeField] public int SeedID { get; private set; }
    [field: SerializeField] public string Comment { get; private set; }

    // 에디터에서 데이터 유효성 검사 (Pre-build Data Validation)
    private void OnValidate()
    {
        if (PlantID <= 0)
        {
            Debug.LogWarning($"[{name}] PlantID가 올바르지 않습니다. (0 이하)", this);
        }
    }
}