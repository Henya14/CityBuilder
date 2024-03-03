using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static Action OnMinuteChanged;
    public static Action OnHourChanged;

    public static int Minute {  get; private set; }
    public static int Hour { get; private set; }

    private float minuteFromRealTime = 0.5f;
    private float timer;

    void Start()
    {
        Minute = 0;
        Hour = 0;
        timer = minuteFromRealTime;
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if(timer <= 0)
        {
            Minute++;
            OnMinuteChanged?.Invoke();

            if (Minute >= 60)
            {
                Hour++;
                Minute = 0;
                OnHourChanged?.Invoke();
            }

            timer = minuteFromRealTime;
        }
    }
}
