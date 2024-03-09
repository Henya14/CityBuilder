using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

public class CamControl : MonoBehaviour
{
    private Vector3 originalrotatio;
    // Start is called before the first frame update
    void Start()
    {
        originalrotatio = transform.parent.eulerAngles;
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("Camera reset");
            transform.parent.eulerAngles = originalrotatio;

        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Camera E pressed");
        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log("Camera Q pressed");
            transform.parent.eulerAngles=new Vector3(30, 135, 0);
        }
    }
}
