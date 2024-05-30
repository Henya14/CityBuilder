using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

public class CamControl : MonoBehaviour
{
    private Vector3 originalRotation;
    private Vector3 originalPosition;
    private bool mainMode;
    private float originalFarClipPlane; //near 0,3 -> 2,6 if something wrong switch it back
    public Vector3 secondaryPosition;
    public float secondaryFarClipPlane;
    public float secondaryCamMoveSpeed;
    public Vector3 maxSecondaryCamPosition;
    public Vector3 minSecondaryCamPosition;
    [SerializeField] float zoomScale;

    // Start is called before the first frame update
    void Start()
    {
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
                Move(true,true);
            }else if (Input.GetKey(KeyCode.S))
            {
                Move(true,false);
            }
            //sideway move
            if (Input.GetKey(KeyCode.D))
            {
                Move(false,true);
            }
            else if (Input.GetKey(KeyCode.A))
            {
                Move(false,false);
            }
            //rotation
            if (Input.GetKey(KeyCode.E))
            {
                transform.Rotate(0, -1, 0);
            }
            else if (Input.GetKey(KeyCode.Q))
            {
                transform.Rotate(0, 1, 0);
            }

            if (!Mathf.Approximately(Input.mouseScrollDelta.y, 0.0f)) {
                Zoom(Input.mouseScrollDelta.y);
            }
        
    }
    //Main mode reset
    void CamReset()
    {
        transform.parent.eulerAngles = new Vector3(0,0,0);
        
        transform.localPosition = originalPosition;
        transform.eulerAngles = originalRotation;
        
        Camera.main.farClipPlane = originalFarClipPlane;
    }
    //secondary move reset
    void ResetToSecondary()
    {

        transform.localPosition = secondaryPosition;
        transform.eulerAngles = new Vector3(0, 0, 0);

        Camera.main.farClipPlane = secondaryFarClipPlane; //viewdistance reduce
    }

    void Zoom(float mouseScrollDelta) {
        var pos = transform.localPosition;
        pos.y += -Input.mouseScrollDelta.y * zoomScale * Time.deltaTime;
        pos.y = Mathf.Clamp(pos.y, minSecondaryCamPosition.y, maxSecondaryCamPosition.y);
        transform.position = pos;
    }
    //secondary movement function
    void Move(bool forOrBackWord, bool onward)
    {
        Vector3 fw = transform.forward;
        Vector3 r = transform.right;

        var pos = transform.localPosition;
        var newPos = pos + (forOrBackWord ? fw : r) * secondaryCamMoveSpeed * (onward ? 1 : -1) * Time.deltaTime;
        //Restrict area
        newPos.x = Mathf.Clamp(newPos.x, minSecondaryCamPosition.x, maxSecondaryCamPosition.x);
        newPos.z = Mathf.Clamp(newPos.z, minSecondaryCamPosition.z, maxSecondaryCamPosition.z);
        transform.position = newPos;
        
    }
}
