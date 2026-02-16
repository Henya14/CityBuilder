using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class DayLightController : MonoBehaviour
{

    GameObject sun;
    Light sunLight;

    [SerializeField] Color morningColor;
    [SerializeField] Color dayColor;
    [SerializeField] Color eveningColor;
    // Start is called before the first frame update
    void Start()
    {
        sun = GameObject.Find("Sun");
        sunLight = sun.GetComponent<Light>();
        //TimeManager.OnMinuteChanged += OnMinuteChanged;
    }

    // Update is called once per frame
    void Update()
    {
        int hour = TimeManager.Hour;
    }

    void OnMinuteChanged()
    {
        float rotateSpeed = 0.0f;
        if (TimeManager.Hour <= 4)
        {
            rotateSpeed = 0.125f;
        }
        else if (TimeManager.Hour > 4 && TimeManager.Hour < 18)
        {
            rotateSpeed = 0.11f;
        }
        else
        {
            rotateSpeed = 0.178f;
        }

        

         if ((TimeManager.Hour == 0 && TimeManager.Minute == 0) || sun.transform.eulerAngles.x >= 180f)
        {
            sun.transform.rotation = Quaternion.Euler(0f, sun.transform.rotation.y, sun.transform.rotation.z);
        }
        else
        {
            sun.transform.Rotate(rotateSpeed, 0f, 0f, Space.Self);
        }
        
        int hour = TimeManager.Hour;
        if (hour >= 4 && hour < 6)
        {
            sunLight.color = Color.Lerp(morningColor, dayColor, (hour - 4 + TimeManager.Minute / 60f) / 2f);
        }
        else if (hour >= 6 && hour < 18)
        {
            sunLight.color = dayColor;
        }
        else if (hour >= 18 && hour < 20)
        {
            sunLight.color = Color.Lerp(dayColor, eveningColor, (hour - 18 + TimeManager.Minute / 60f) / 2f);
        }
        else
        {
            sunLight.color = eveningColor;
        }
    }
}
