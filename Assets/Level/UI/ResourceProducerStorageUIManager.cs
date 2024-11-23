using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class ResourceProducerStorageUIManager : MonoBehaviour
{
    private const string Location = "Assets/Level/UI/ResourceProducerInfoView.uxml";
    private VisualElement root;
    private VisualElement parent;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SetRoot(ref VisualElement parent){
        this.parent = parent;
        VisualElement element = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Location).Instantiate();
        root = element.Q<VisualElement>("producer-root-container");;
        parent.hierarchy.Add(root);
    }
}
