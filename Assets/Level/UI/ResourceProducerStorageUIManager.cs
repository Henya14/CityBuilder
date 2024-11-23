using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

public class ResourceProducerStorageUIManager : MonoBehaviour
{
    private ResourceStorage storage;
    private ResourceProducer producer;
    private const string Location = "Assets/Level/UI/ResourceProducerInfoView.uxml";
    private VisualElement root;
    private VisualElement parent;

    private Button preButton;
    private Button nextButton;
    private Label selectedResourceLabel;
    private Button changeResourceButton;
    private List<string> resources;
    private int currentIdx;

    private Label storedAmountPerCapacityLabe;

    private VisualElement onlyProducerContainer;

    private Label producerStatusInfoLabel;

    private VisualElement reasonInfoContainer;
    private Label reasonInfoLabel;

    private Button turnOnOffButton;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(producer != null || storage != null)
        {

            SetAmountPerCapacity();
            DispalyReason(); 
            DisplayStatus();
            DisplayTurnButtonText();
        }
    }
    public void SetRoot(ref VisualElement parent){
        this.parent = parent;
        VisualElement element = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Location).Instantiate();
        root = element.Q<VisualElement>("producer-root-container");
        root.visible = false;
        parent.hierarchy.Add(root);
        preButton = root.Q<Button>("previous-resource-button");
        preButton.clicked += OnPrevClick;
        nextButton = root.Q<Button>("next-resource-button");
        nextButton.clicked += OnNextClick;
        selectedResourceLabel = root.Q<Label>("selected-resource-label");
        changeResourceButton = root.Q<Button>("change-resource-button");
        changeResourceButton.clicked += OnChangeClick;

        storedAmountPerCapacityLabe = root.Q<Label>("stored-amount-per-capacity");

        onlyProducerContainer = root.Q<VisualElement>("only-producer-container");
        
        producerStatusInfoLabel = root.Q<Label>("producer-status-info-label");

        reasonInfoContainer = root.Q<VisualElement>("reason-info-container");
        reasonInfoLabel = root.Q<Label>("reason-info-label");

        turnOnOffButton = root.Q<Button>("turn-on-off-button");
        turnOnOffButton.clicked += OnTurnClicked;

    }
    public void CurrentSelected(GameObject go)
    {
        ResourceProducer producer = go.GetComponent<ResourceProducer>();
        if(producer != null)
        {
            SetProducerInfo(producer);

        }
        else
        {
            ResourceStorage storage = go.GetComponent<ResourceStorage>();
            if(storage != null)
            {
                SetStorageInfo(storage);
                ProdicerVisibleOff();
            }
            else
            {
                StorageVisibleOff();
                ProdicerVisibleOff();
                this.producer = null;
                this.storage = null;
                onlyProducerContainer.visible = false;
                root.visible = false;
                return;
            }
        }
    }
    private void SetProducerInfo(ResourceProducer producer)
    {

        this.storage = null;
        this.producer = producer;

        SetAmountPerCapacity();
        resources = producer.ProduceOptions;
        currentIdx = resources.IndexOf(producer.Resource.ResourceName);
        DispalyResource();

        DisplayStatus();
        DispalyReason();
        DisplayTurnButtonText();

        onlyProducerContainer.visible = true;
        root.visible=true;

    }
    private void SetStorageInfo(ResourceStorage storage)
    {

        this.producer = null;
        this.storage = storage;

        SetAmountPerCapacity();
        resources = this.GetComponent<ResourceManager>().GetAllResourceName();
        currentIdx = resources.IndexOf(storage.Resource.ResourceName);
        DispalyResource();

        onlyProducerContainer.visible=false;
        root.visible = true;

    }
    private void SetAmountPerCapacity()
    {
        float amount = producer != null ? producer.StoredAmount : storage.StoredAmount;
        float capacity = producer != null ? producer.Capacity : storage.Capacity;
        storedAmountPerCapacityLabe.text =
            $"{((float)Mathf.Floor(amount * 100) / 100).ToString("F2")}" +
            $"/" +
            $"{((float)Mathf.Floor(capacity * 100) / 100).ToString("F2")}";
    }
    private void OnNextClick()
    {
        currentIdx++;
        if( currentIdx >= resources.Count )
        {
            currentIdx = 0;
        }
        DispalyResource();

    }
    private void OnPrevClick()
    {
        currentIdx--;
        if( currentIdx < 0 )
        {
            currentIdx = resources.Count-1;
        }
        DispalyResource();

    }
    private void OnChangeClick()
    {
        if(producer != null)
        {
            producer.AssignToResource(resources[currentIdx]);
        }
        else if (storage != null)
        {
            storage.AssignToResource(resources[currentIdx]);
        }

    }
    private void OnTurnClicked()
    {
        if(producer != null)
        {
            producer.Switch();
            DisplayTurnButtonText();
        }
    }
    private void DispalyResource()
    {
        if(resources.Count == 1)
        {
            preButton.visible = false;
            nextButton.visible = false;
            changeResourceButton.visible = false;
            selectedResourceLabel.visible = false;
        }
        else
        {
            preButton.visible = true;
            nextButton.visible = true;
            changeResourceButton.visible = true;
            selectedResourceLabel.visible = true;
        }
        selectedResourceLabel.text = resources[currentIdx];
    }
    private void DispalyReason()
    {
        if(producer != null)
        {
            string reason = producer.Reason;
            if (string.IsNullOrEmpty(reason))
            {
                reasonInfoContainer.visible = false;
            }
            else
            {
                reasonInfoLabel.text = reason;
                reasonInfoContainer.visible = true;

            }
        }
    }
    private void DisplayStatus()
    {
        if(producer != null)
        {
            string text = "";
            if (!producer.TurnedOn)
                text = "NOT Running";
            else
            {
                if (producer.IsRunning)
                    text = "Running";
                else
                    text = "NOT Running";
            }

            producerStatusInfoLabel.text = text;
        }
    }
    private void DisplayTurnButtonText()
    {
        if(producer != null)
        {
            if (producer.TurnedOn)
                turnOnOffButton.text = "Turn Off";
            else
                turnOnOffButton.text = "Turn On";
        }
    }
    private void StorageVisibleOff()
    {
        preButton.visible = false;
        nextButton.visible = false;
        changeResourceButton.visible = false;

    }
    private void ProdicerVisibleOff()
    {

        reasonInfoContainer.visible = false;
        onlyProducerContainer.visible = false;
        root.visible = false;
    }
}
