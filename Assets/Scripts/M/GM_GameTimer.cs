using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GM_GameTimer : MonoBehaviour
{
    public Text stageTimerText;
    public float maxTime = 360;
    int minute = 0;
    int second = 0;
    public bool isStarted = false;
    void Start()
    {
        
    }

    
    void Update()
    {   
        if(isStarted)
        {
            maxTime -= Time.deltaTime;
            minute = (int)maxTime / 60;
            second = (int)((maxTime - 60 * minute) % 60);
            stageTimerText.text = $"{minute.ToString("D2")} : {((int)(second)).ToString("D2")}";
        }
        if(maxTime<=0)
        {
            GameManager.instance.GameOver(1);
        }
        
    }
}
