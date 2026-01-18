using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebugLabel : MonoBehaviour
{
    private TextMeshPro textMesh;
    private Camera mainCamera;

    private string labelText = "Debug Label";

    [SerializeField] private bool isBillboard = true;
    [SerializeField] private Color textColor = Color.white;

    // Start is called before the first frame update
    void Start()
    {
        textMesh = gameObject.transform.Find("Label").GetComponent<TextMeshPro>();
        if (textMesh != null)
        {
            textMesh.color = textColor;
            SetText(labelText);
        }
        mainCamera = Camera.main;

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void LateUpdate()
    {
        if (isBillboard && mainCamera != null && textMesh != null)
        {
            var lookVector = mainCamera.transform.position - transform.position;
            textMesh.transform.rotation = Quaternion.LookRotation(-lookVector);
        }
    }

    public void Init(string text)
    {
        labelText = text;
    }
    public void SetText(string text)
    {
        
        if (textMesh != null)
        {
            textMesh.text = text;
        }
    }
}
