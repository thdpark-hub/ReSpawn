using System.Collections.Generic;
using UnityEngine;

public class AlchemyControler : MonoBehaviour
{
    public int inputId;
    void swap(ref int first, ref int second)
    {
        int tmp;
        tmp = first;
        first = second;
        second = tmp;
    }

    public void SortNetwork(ref int first, ref int second, ref int third, ref int fourth)
    {
        if(first >= second) swap(ref first, ref second);
        if(third >= fourth) swap(ref third, ref fourth);

        if(first >= third) swap(ref first, ref third);
        if(second >= fourth) swap(ref second, ref fourth);

        if(second >= third) swap(ref second, ref third);
    }
    public struct AlchemyTarget
    {
        public int FirstId;
        public int SecondId;
        public int ThirdId;
        public int FourthId;
    }
    public AlchemyTarget target;                //구조체 넣기위한 선언
    public bool StartAlchemy = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    private Dictionary<Tkey, Tvalue>;
    // Update is called once per frame
    void Update()
    {
        if (StartAlchemy)
        {
            SortNetwork(ref target.FirstId, ref target.SecondId,ref target.ThirdId, ref target.FourthId);

        }
    }
}
