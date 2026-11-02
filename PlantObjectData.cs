using UnityEngine;

public class PlantObjectData : MonoBehaviour, IInteractable
{
    [Header("Plant Data")]
    [SerializeField] private InterfaceObject data;
    [SerializeField]private SeedData seedData;

    private void Start()
    {
        if (data == null)
        {
            Debug.LogWarning(
                $"{gameObject.name}: PlantData가 연결되지 않았습니다.",
                this
            );

            return;
        }

        Debug.Log(
            $"식물 ID: {data.objectName}"
        );
    }

    public string GetDescription(RaycastHit hit)
    {
        if (data == null)
        {
            return string.Empty;
        }
        if((hit.collider.gameObject.layer) != 0) { }
        return $"{data.objectName}\n{data.description}";
    }
    public void PickUp()
    {
        Destroy(gameObject);
        Debug.Log("ID정보: "+data.ID + "이름: "+data.objectName +"정보: " + data.description);
        return;
    }
    public void getInterAction()
    {
        if (data == null)
        {
            return;
        }

        //if조건문으로 인벤토리 용량확인, 없다면 넘어가고 있다면 개수확인하고 추가함
        //상호작용시 오브젝트 데이터 인벤토리에 추가
        //실 오브젝트 파괴
        //사실상 데이터만 남아 있음
    }
}