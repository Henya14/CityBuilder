using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class ResourceManagerUIManager : MonoBehaviour
{
    public ResourceManager ResourceManager;
    private VisualElement root;
    private Label title;
    private ListView list;
    private List<VisualElement> children;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetRoot(ref VisualElement root)
    {
        if(this.root == null)
        {
            this.root = root;
            title = root.Q<Label>("Resource-Title");
            list = root.Q<ListView>("resource-list");
        }

    }
    public VisualElement AddChildren()
    {

        VisualTreeAsset uiAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Level/UI/ResourceItem.uxml");
        VisualElement ui = uiAsset.Instantiate();
        VisualElement con = list.Q<VisualElement>("unity-content-container");
        con.hierarchy.Add(ui);

        return ui;
    }
}
