using System;
using System.Collections.Generic;
using UnityEngine;
using static BaseObject;

[CreateAssetMenu(
    fileName = "RecipeRepository",
    menuName = "Recipe/Recipe Repository"
)]

public class RecipeRepository : ScriptableObject
{
    // Inspector에서 등록하는 원본 데이터
    [SerializeField]
    private List<RecipeDataBase> recipes;

    // 조합 → 결과 ID
    private Dictionary<RecipeKey, int> outputData;

    // 결과 ID → GameObject
    [SerializeField]
    private List<RecipeData> items;

    private Dictionary<int, GameObject> itemRecipe;


    public void Initialize()
    {
        // Dictionary 생성
        outputData = new Dictionary<RecipeKey, int>();
        itemRecipe = new Dictionary<int, GameObject>();

        // 1. RecipeDataBase → Dictionary

        foreach (RecipeDataBase recipe in recipes)
        {
            outputData.Add(
                recipe.recipeKey,
                recipe.output
            );
        }


        // 2. Item → Dictionary


        foreach (RecipeData item in items)
        {
            itemRecipe.Add(
                item.id,
                item.gameObject
            );
        }
    }


    // 조합으로 결과 ID 찾기
    public bool TryGetOutput(
        RecipeKey key,
        out int output)
    {
        return outputData.TryGetValue(key, out output);
    }


    // 결과 ID로 GameObject 찾기
    public bool TryGetGameObject(
        int id,
        out GameObject gameObject)
    {
        return itemRecipe.TryGetValue(id, out gameObject);
    }
}