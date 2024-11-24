using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;


public class TransportationUIManager : MonoBehaviour
{
    private List<TransportationDestination> destinations;
    TransportationStart transportationStart;
    private string originalname;
    [SerializeField]
    private TransportHUB transportHUB;

    private const string Location = "Assets/Level/UI/TransportationUI.uxml";
    private VisualElement root;
    private VisualElement parent;

    private VisualElement newRouteContainer;

    private Label noDestinationLabel;

    private VisualElement routeDataContainer;

    private DropdownField destinationDropdown;
    private DropdownField carrierDropdown;
    private Slider amountSlider;

    private VisualElement repeatContainer;
    private Toggle repeatToggle;

    private Button createRouteButton;

    void Start() {
        transportHUB = GetComponent<TransportHUB>();
    }
    void Update() { 

        if(transportationStart!=null && !transportationStart.GetGameObject().name.Equals(originalname))
        {
            CurrentSelected(transportationStart.GetGameObject());
        }
    }


    public void SetRoot(ref VisualElement parent)
    {
        this.parent = parent;
        VisualElement element = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Location).Instantiate();
        root = element.Q<VisualElement>("transportation-container");
        root.style.display = DisplayStyle.None;
        parent.hierarchy.Add(root);


        newRouteContainer = root.Q<VisualElement>("new-route-container");
        noDestinationLabel = root.Q<Label>("no-destination-label");
        routeDataContainer = root.Q<VisualElement>("route-data-container");
        destinationDropdown = root.Q<DropdownField>("destination-dropdown");
        carrierDropdown = root.Q<DropdownField>("carrier-dropdown");
        amountSlider = root.Q<Slider>("amount-slider");
        repeatContainer = root.Q<VisualElement>("repeat-container");
        repeatToggle = root.Q<Toggle>("repeat-toggle");
        createRouteButton = root.Q<Button>("create-route-button");

        carrierDropdown.RegisterCallback<ChangeEvent<string>>((evt) =>
        {
            CarrierChanged(evt.newValue);
        });
    }
    public void CurrentSelected(GameObject go)
    {
        transportationStart = go.GetComponent<TransportationStart>();
        if (transportationStart == null) { 
            HideRoot();
            return; 
        }
        if(carrierDropdown.index == -1)
        {
            carrierDropdown.choices.AddRange(transportHUB.GetCarriers().Select(carrier => carrier.name));
            carrierDropdown.index = 0;
        }
        originalname = transportationStart.GetGameObject().name;
        CarrierChanged(carrierDropdown.value);
        if(destinationDropdown.index == -1) { 
            HideNewRouteDataContainer();
        }
        else
        {
            ShowNewRouteDataContainer();
        }
        ShowRoot();

    }
    private void HideRoot()
    {
        root.style.display = DisplayStyle.None ;
    }
    private void ShowRoot()
    {
        root.style.display= DisplayStyle.Flex ;
    }
    private void CarrierChanged(string name)
    {

        foreach (var carrier in transportHUB.GetCarriers())
        {
            if (carrier.name.Equals(name)){
                SetCarrier(carrier.GetComponent<AbstractCarrier>());
                return;
            }
        }
    }
    private void SetCarrier( AbstractCarrier abstractCarrier)
    {
        destinations = new();
        destinations = FindObjectsOfType<MonoBehaviour>().OfType<TransportationDestination>()
            .Where(destination => 
                abstractCarrier.CanTransportBetween(transportationStart, destination)
                && !transportationStart.GetGameObject().Equals(destination.GetGameObject())
                )
            .ToList();
        destinationDropdown.choices.Clear();
        if(destinations.Count == 0) { 
            destinationDropdown.index = -1;
        }
        else {
            destinationDropdown.choices.AddRange(destinations.Select(dest => dest.GetGameObject().name));
            destinationDropdown.index = 0;
            destinationDropdown.value = destinationDropdown.choices[0];
        }
    }
    private void ShowNewRouteDataContainer()
    {
        routeDataContainer.style.display = DisplayStyle.Flex;
        noDestinationLabel.style.display = DisplayStyle.None;
    }
    private void HideNewRouteDataContainer()
    {
        routeDataContainer.style.display = DisplayStyle.None;
        noDestinationLabel.style.display = DisplayStyle.Flex;
    }

}
