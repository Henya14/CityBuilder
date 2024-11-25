using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class ActiveRouteUIManager : MonoBehaviour
{

    [SerializeField]
    private Route route;

    private const string Location = "Assets/Level/UI/ActiveRouteUI.uxml";
    private VisualElement root;
    private VisualElement parent;

    private Label routeNameLabel;

    private Toggle onRepeatToggle;
    private ProgressBar repeatProgress;

    private ProgressBar routeProgress;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void SetRoot(ref VisualElement parent)
    {
        this.parent = parent;
        VisualElement element = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Location).Instantiate();
        root = element.Q<VisualElement>("active-route-container");
        root.style.display = DisplayStyle.None;
        parent.hierarchy.Add(root);


        routeNameLabel = root.Q<Label>("route-name");
        onRepeatToggle = root.Q<Toggle>("on-repeat-toggle");
        repeatProgress = root.Q<ProgressBar>("repeat-progress");
        routeProgress = root.Q<ProgressBar>("route-progress");
        routeProgress.lowValue = (float)CarrierStatus.Setup;
        routeProgress.highValue = (float)CarrierStatus.CompletedRoute;
        repeatProgress.lowValue = 0;

        Hide();
    }
    public void SetRoute(Route route)
    {
        this.route = route;
        if(route != null)
        {
            routeNameLabel.text = route.name;
            onRepeatToggle.value = route.OnRepeat;
            onRepeatToggle.RegisterValueChangedCallback(evt =>
            {
                if(route.OnRepeat != evt.newValue)
                    route.OnRepeat = evt.newValue;
            });
        }
        
    }
    public void Hide()
    {
        root.style.display = DisplayStyle.None;
    }
    private void Show()
    {
        if(route.GetComponentInChildren<AbstractCarrier>()!=null)
        root.style.display = DisplayStyle.Flex;
        onRepeatToggle.value = route.OnRepeat;
        ProgressUpdate();

    }
    public void ShowIfNameContains(string name)
    {
        if(this.name.Contains(name)) Show();
        else Hide();
    }
        // Update is called once per frame
    void Update()
    {
        if(root.style.display == DisplayStyle.Flex)
        {
            ProgressUpdate();

        }
        if(route ==null)
        {
            Destroy(this);
        }
    }
    private void ProgressUpdate()
    {
        if (route != null)
        {
            repeatProgress.highValue = route.RepeatTime;
            repeatProgress.value = route.RepeatTime-route.Counter;

            if (route.GetComponentInChildren<AbstractCarrier>() != null)
                routeProgress.value = (float)route.GetComponentInChildren<AbstractCarrier>().Status;
            else 
                Hide();
        }
    }
    private void OnDestroy()
    {
        Hide();
        root.Clear();

    }
}
