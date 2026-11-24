using UnityEngine;
using FishNet.Object;
using System.Collections.Generic;

public class AlchemyLogic : NetworkBehaviour
{
    private AlchemyController alchemyController;
    private AlchemyManager alchemyManager;
    List<int> objectsList = new List<int>();
    Dictionary<int, int> objectDic = new Dictionary<int, int>();

    private void Awake() { }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Object") && objectsList.Count < 4 && objectDic.Count < 4)
        {
            RecipeData Item = other.GetComponent<RecipeData>();
            AddItem(Item);
        }
    }
    private void AddItem(RecipeData itemStack)
    {
        if (!objectsList.Contains(itemStack.id))
        {
            objectsList.Add(itemStack.id);
        }
    }
    private void Mixing()
    {
        alchemyController.SortNetwork(objectsList);
        //alchemyManager.CompareDictionary(objectsList);
    }

    public void StartMixing()
    {
        Mixing();
    }
    
    private void Update() { }
}