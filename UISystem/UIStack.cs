using UnityEngine;
using UnityEngine.UI;

public class UIStack : MonoBehaviour
{
    private GameObject[] NowPanel = new GameObject[5];
    private int top = -1;
    private GameObject Object;

    private void inputPanel()
    {
        NowPanel[++top] = Object;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
