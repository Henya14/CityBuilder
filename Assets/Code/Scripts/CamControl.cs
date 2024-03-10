using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

public class CamControl : MonoBehaviour
{
    private Vector3 originalParentRotation;
    private Vector3 originalRotation;
    private Vector3 originalPosition;
    private bool mainMode;
    private float originalFarClipPlane; //near 0,3 -> 2,6 if something wrong switch it back
    public Vector3 secondaryPosition;
    public float secondaryFarClipPlane;
    public float secondaryCamMoveSpeed;
    public Vector3 maxSecondaryCamPosition;
    public Vector3 minSecondaryCamPosition;

    // Start is called before the first frame update
    void Start()
    {
        originalParentRotation = transform.parent.eulerAngles;
        originalPosition = transform.localPosition;
        originalRotation = transform.eulerAngles;
        originalFarClipPlane = Camera.main.farClipPlane;
        mainMode = true;
        if (secondaryFarClipPlane <= 0) secondaryFarClipPlane = originalFarClipPlane;
        if (secondaryCamMoveSpeed <= 0) secondaryCamMoveSpeed = 0.1f;
        if (!(
            secondaryPosition.x <= maxSecondaryCamPosition.x && secondaryPosition.x >= minSecondaryCamPosition.x &&
            secondaryPosition.y <= maxSecondaryCamPosition.y && secondaryPosition.y >= minSecondaryCamPosition.y &&
            secondaryPosition.z <= maxSecondaryCamPosition.z && secondaryPosition.z >= minSecondaryCamPosition.z
            ))
                secondaryPosition=minSecondaryCamPosition;
    }
    // Update is called once per frame
    void Update()
    {
        //Switch viewmode
        if (Input.GetKeyDown(KeyCode.F))
        {
            mainMode = !mainMode;
            Debug.Log("Switch to " + (mainMode ? "main mode" : "secondary mode"));

            if (mainMode)
            {
                CamReset();
            }else
            {
                ResetToSecondary();
            }
        }




        if (mainMode)
        {
            //reset
            if (Input.GetKeyDown(KeyCode.R))
            {
                Debug.Log("Camera reset");
                CamReset();
            }
            //rotate
            else if (Input.GetKey(KeyCode.E))
            {
                Debug.Log("Camera rotate right");
                transform.parent.Rotate(0, -1, 0);
            }
            else if (Input.GetKey(KeyCode.Q))
            {
                Debug.Log("Camera rotate left");
                transform.parent.Rotate(0, 1, 0);
            }
        }
        else
        {
            //reset
            if (Input.GetKeyDown(KeyCode.R))
            {
                Debug.Log("Camera secondary reset");
                ResetToSecondary();
            }

            //Maybe better with switch but then no diagonal movement

            //linear move
            if (Input.GetKey(KeyCode.W))
            {
                Debug.Log("Camera secondary move forward");
                Move(true,true);
            }else if (Input.GetKey(KeyCode.S))
            {
                Debug.Log("Camera secondary move backword");
                Move(true,false);
            }
            //sideway move
            if (Input.GetKey(KeyCode.D))
            {
                Debug.Log("Camera secondary move right");
                Move(false,true);
            }
            else if (Input.GetKey(KeyCode.A))
            {
                Debug.Log("Camera secondary move left");
                Move(false,false);
            }
            //rotation
            if (Input.GetKey(KeyCode.E))
            {
                Debug.Log("Camera rotate right");
                transform.Rotate(0, 1, 0);
            }
            else if (Input.GetKey(KeyCode.Q))
            {
                Debug.Log("Camera rotate left");
                transform.Rotate(0, -1, 0);
            }

        }
    }
    //Main mode reset
    void CamReset()
    {
        transform.parent.eulerAngles = originalParentRotation;
        
        transform.localPosition = originalPosition;
        transform.eulerAngles = originalRotation;
        
        Camera.main.farClipPlane = originalFarClipPlane;
    }
    //secondary move reset
    void ResetToSecondary()
    {
        transform.parent.eulerAngles = new Vector3(0, 0, 0);

        transform.localPosition = secondaryPosition;
        transform.eulerAngles = new Vector3(0, 0, 0);

        Camera.main.farClipPlane = secondaryFarClipPlane; //viewdistance reduce
    }
    //secondary movement function
    void Move(bool forOrBackWord, bool onward)
    {
        Vector3 fw = transform.forward;
        Vector3 r = transform.right;

        var pos = transform.localPosition;
        var newPos = pos + ((forOrBackWord ? fw : r) * secondaryCamMoveSpeed * (onward ? 1 : -1));
        //Restrict area
        if (
            newPos.x < maxSecondaryCamPosition.x && newPos.x > minSecondaryCamPosition.x &&
            newPos.y<maxSecondaryCamPosition.y && newPos.y>minSecondaryCamPosition.y &&
            newPos.z < maxSecondaryCamPosition.z && newPos.z > minSecondaryCamPosition.z
            )
                transform.localPosition = newPos;
        
    }
}
