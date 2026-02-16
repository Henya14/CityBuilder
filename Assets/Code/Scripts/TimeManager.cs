using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager instance;

    private void Awake() 
    {
        if (instance == null)
            instance = this;
    }

    public static Action On10MinutesChanged;

    public static Action OnMinuteChanged;
    public static Action OnHourChanged;

    enum SpeedTypes { normal, fast, fastest};
    SpeedTypes speed;

    public static int Minute { get; private set; }
    public static int Hour { get; private set; }

    float minuteFromRealTime = 1f; 
    float timer;

    bool stopped;

    void Start() 
    {
        Minute = 0;
        Hour = 0;
        timer = minuteFromRealTime;
        stopped = false;
    }

    void Update() 
    {
        if (!stopped)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                Minute++;
                
                if (Minute % 10 == 0) 
                {
                    On10MinutesChanged?.Invoke();
                }
                if (Minute >= 59)
                {
                    Hour++;
                    Minute = 0;
                    OnHourChanged?.Invoke();
                    
                }

                if (Hour >= 24)
                {
                    Hour = 0;
                }
                OnMinuteChanged?.Invoke();

                timer = minuteFromRealTime;
            }
        }
        
    }

    public void ChangeTimerSpeed() 
    {
        switch (speed)
        {
            case SpeedTypes.normal:
                speed = SpeedTypes.fast;
                minuteFromRealTime = 0.25f;
                break;
            case SpeedTypes.fast:
                speed = SpeedTypes.fastest;
                minuteFromRealTime = 0.1f;
                break;
            case SpeedTypes.fastest:
                speed = SpeedTypes.normal;
                minuteFromRealTime = 5.75f;
                break;
            default: speed = SpeedTypes.normal;
                minuteFromRealTime = 0.75f;
                break;

        }
    }
    public void StartStopTimer() 
    {
        stopped = !stopped;
    }

    public void LoadTime(int hour, int minute)
    {
        Hour=hour;
        Minute=minute;
    }
}
