using System;
using UnityEngine;
using UnityEngine.UIElements;

public class ResourceUIManager : MonoBehaviour
{
    private VisualElement root;
    private Label ResName;
    private Label ResAmount;
    private Resource Resource;
    // Start is called before the first frame update
    void Start()
    {
        Resource = this.GetComponent<Resource>();
        root = this.GetComponentInParent<ResourceManagerUIManager>().AddChildren();
        ResName = root.Q<Label>("ResourceName");
        ResName.text = Resource.ResourceName+":";
        ResAmount = root.Q<Label>("ResourceAmount");
        ResAmount.text = Resource.GetAmount().ToString();
    }

    // Update is called once per frame
    void Update()
    {
        if (Resource != null && root != null) { 
            ResAmount.text = ((float)Mathf.Floor(Resource.GetAmount() * 100) / 100)
            //ResAmount.text = Math.Round((double)Resource.GetAmount(),2, MidpointRounding.ToEven)
                .ToString("F2");
        }
        
    }
}
