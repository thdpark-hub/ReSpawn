using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemSlot
{
    public InterfaceObject item;   // ScriptableObject 참조
    public int count;
    public bool IsEmpty => item == null;
}

public class InventoryList : MonoBehaviour
{
    private ItemSlot[] inventorySlot = new ItemSlot[50];
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    private int CheckInventory(InterfaceObject curGameObject, ref int Empty)
    {
        for(int i = 0 ; i < inventorySlot.Length; i++)
        {
            if(inventorySlot[i].item == curGameObject)
            {
                return i;
            }
            if(Empty == -1 && !inventorySlot[i].IsEmpty)
            {
                Empty = i;
            }
        }

        return -1;
    }
    public void ListUp(InterfaceObject curGameObject)
    {
        int location = -1;
        int emptyLocation = -1;
        location = CheckInventory(curGameObject, ref emptyLocation);
        if(location == -1)
        {
            for(int i = 0; i < inventorySlot.Length; i++)
            {
                if(inventorySlot[i].IsEmpty)
                {
                    inventorySlot[i].item = curGameObject;
                }
            }
        }
        else{
            if(curGameObject.ID / 100000 == 1)
            {
                inventorySlot[emptyLocation].item = curGameObject;
            }
            else
            {
                inventorySlot[location].count++;
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
