using NUnit.Framework.Internal;
using Unity.VisualScripting;
using UnityEngine;

public class Alchemy : MonoBehaviour
{
    [SerializeField]private LayerMask ObjectLayer;
    private AlchemyControler alchemyControler;
    void Start()
    {
        alchemyControler = GetComponent<AlchemyControler>();
    }
    
    void OnTriggerEnter(Collider other)
    {
        int layerMask = 1 << other.gameObject.layer;
        if((layerMask & ObjectLayer.value) != 0)
        {
            TestObject testObject = GetComponent<TestObject>();
            alchemyControler.inputId = testObject.ObjectId;         //구조체에 넣음
        }
    }
}