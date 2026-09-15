using System;
using UnityEngine;

[CreateAssetMenu(fileName = "TestObject", menuName = "Scriptable Objects/TestObject")]
public class TestObject : ScriptableObject
{
    [SerializeField]public int ObjectId;
    [SerializeField]private Sprite sprite;
    [SerializeField]private string name;
    [SerializeField]private string comment;
    [SerializeField]private float BoilTime;
    [SerializeField]private float What;
    [Serializable]public struct AlchemyTarge
    {
        int FristId;
        int SecondId;
        int ThirdId;
        int FourId;
    }
}
