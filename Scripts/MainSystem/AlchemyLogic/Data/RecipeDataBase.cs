using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RecipeDataBase", menuName = "Recipe/RecipeDataBase")]
public class RecipeDataBase : ScriptableObject
{
    [field: SerializeField] public RecipeData id1 { get; private set; }
    [field: SerializeField] public RecipeData id2 { get; private set; }
    [field: SerializeField] public RecipeData id3 { get; private set; }
    [field: SerializeField] public RecipeData id4 { get; private set; }

    public RecipeKey recipeKey
    {
        get
        {
            return new RecipeKey(id1, id2, id3, id4);
        }
    }

    [field: SerializeField]
    public int output { get; private set; }
}
