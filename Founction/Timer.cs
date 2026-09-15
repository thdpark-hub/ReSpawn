using UnityEngine;

public class Timer : MonoBehaviour
{
    public float PlayTime = 0f;
    private float Counter = 0f;
    public bool TimerStart = false;
    public bool TimerFinish = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Counter = PlayTime;
        if (TimerStart)
        {
            Counter += Time.deltaTime;
            if(Counter >= PlayTime)
            {
                TimerFinish = true;
                TimerStart = false;
            }
        }
    }
}
