using System.Collections.Generic;
using UnityEngine;

public class AlchemyManager : MonoBehaviour
{
    [SerializeField]private RecipeDataBase recipeDataBase;
    [SerializeField]private RecipeData Recipe;
    private Dictionary<int, GameObject> ItemReicpe = new Dictionary<int, GameObject>();             //output을 이용해 오브젝트 가져오는 딕셔너리
    private Dictionary<RecipeKey, int> outputData = new Dictionary<RecipeKey, int>();         //output을 얻어내는 딕셔너리

    private RecipeKey inputKey;

    private void AlchemyStart()
    {
        if (outputData.TryGetValue(inputKey, out var result))
        {

        }
    }

    private AlchemyController alchemyController;
    private void Awake()
    {
        //ItemDictionary();
        //OutputDictionary();
    }
    /*
    private void OutputDictionary()
    {
        outputData.Clear();
        foreach(var getData in Recipe.recipeDatas)
        {
            int[] ids = new int[]
            {
                getData.recipeKey.id1,
                getData.recipeKey.id2,
                getData.recipeKey.id3,
                getData.recipeKey.id4
            };

            alchemyController.SortNetwork(ref ids[0], ref ids[1], ref ids[2], ref ids[3]);
            RecipeKey Datakey = new RecipeKey(ids[0], ids[1], ids[2], ids[3]);

            if (!outputData.ContainsKey(Datakey)){
                outputData.Add(Datakey, getData.output);
            }
        }
        
    }

    private void ItemDictionary()
    {
        ItemReicpe.Clear();
        foreach(var item in recipeDataBase.recipeList)
        {
            if (ItemReicpe.ContainsKey(item.id))
            {
                ItemReicpe.Add(item.id, item.prefab);
            }
        }
    }
    */
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
