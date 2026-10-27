using System.Collections.Generic;
using UnityEngine;

public class AlchemyController : MonoBehaviour
{
    private int first;
    private int second;
    private int third;
    private int fourth;
    void swap(List<int> list,int a, int b)
    {
        int temp = list[a];
        list[a] = list[b];
        list[b] = temp;
    }

    public void SortNetwork(List<int> recipeDatas)
    { 
        if (recipeDatas[0]>= recipeDatas[1]) swap(recipeDatas, 0, 1);
        if (recipeDatas[2] >= recipeDatas[3]) swap(recipeDatas, 2, 3);

        if (recipeDatas[0] >= recipeDatas[2]) swap(recipeDatas, 0, 2);
        if (recipeDatas[1] >= recipeDatas[3]) swap(recipeDatas, 1, 3);

        if (recipeDatas[1] >= recipeDatas[2]) swap(recipeDatas, 1, 2);
    }
    
    public bool StartAlchemy = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (StartAlchemy)
        {
            //SortNetwork(ref target.FirstId, ref target.SecondId, ref target.ThirdId, ref target.FourthId);
        }
    }
}