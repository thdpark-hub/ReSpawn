using UnityEngine;
using FishNet.Object;

public class FarmTimer : NetworkBehaviour
{
    [SerializeField]private SeedData seedData;
    private float growTime;
    private float timer;
    private float waterTimer;
    private float waterTime;
    private bool getWater = false;

    public void SetSeed(SeedData input)
    {
        seedData = input;
    }
    public void setWater()
    {
        waterTimer = 0f;
    }

     private void Awake()
    {
        growTime = seedData.growTime;
        waterTime = seedData.waterTime;
    }

     private void Update() 
    {
        if (waterTimer >= waterTime && !getWater)
        {
            Debug.Log("물을 줘야합니다"); //ui정보를 통해서 물이 부족하다는 신호 보내기
        }
        else
        {
            waterTimer += Time.deltaTime;
        }
        
        if(getWater)
        {
            timer += Time.deltaTime;
            if(timer >= growTime)
            {
                Debug.Log("성장완료" + seedData.amount);      //결과갑 리턴
                Debug.Log("씨앗" + seedData.worldSize);   //씨앗 리턴
            }
        }
    }
}
