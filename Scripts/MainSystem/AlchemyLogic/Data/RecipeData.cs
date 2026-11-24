using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RecipeData", menuName = "Recipe/RecipeData")]
public class RecipeData: ScriptableObject
{
    [field: SerializeField] public int id { get; private set; }
    //[field: SerializeField] public int amount;
    [field: SerializeField] public GameObject gameObject { get; private set; }
    [field: SerializeField] public int amount;
}
