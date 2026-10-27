using UnityEngine;

[CreateAssetMenu(fileName = "BaseObject", menuName = "Scriptable Objects/BaseObject")]
public class BaseObject : ScriptableObject
{
    [System.Serializable]
    public struct BaseObjectData
    {
        string name;
        public int objectID;
        public GameObject prefab;
        public int amount;
    }

    public BaseObjectData baseData;
}
