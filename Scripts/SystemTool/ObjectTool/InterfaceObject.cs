using System;
using UnityEngine;

[CreateAssetMenu(fileName = "InterfaceObject", menuName = "Scriptable Objects/InterfaceObject")]
public class InterfaceObject : ScriptableObject
{
    [field: SerializeField] public string objectName { get; private set; }
    [field: SerializeField] public string description { get; private set; }
    [field: SerializeField] public string objectIcon { get; private set; }
}
