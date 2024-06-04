using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UIElements;

public enum GameMode
{
    SelectionMode,
    BuildMode,
    NavigationMode
}
public class GameUIManager : MonoBehaviour
{
    Button selectionModeButton;
    Button buildModeButton;
    Button navigationModeButton;
    VisualElement infoContainer;
    Label infoContainerText;
    ListView buildingList;

    Label balanceLabel;
    Label electricityLabel;
    Label coalLabel;
    Label woodLabel;

    Label questText;

    Button timeStartStopButton;
    Button timeForwardButton;
    Label timeTextField;

    Toggle moralityViewToggle;

    Button buildingsButton;
    VisualElement buildingHud;

    Button taxesButton;
    VisualElement taxesHud;
    List<Label> labels = new List<Label>();
    Button saveButton;

    List<Button> gameModeSelectorButtons = new List<Button>();
    public GameMode selectedGameMode { get; set; } = GameMode.SelectionMode;

    private BuildModeManager buildModeManager;
    private GridManager gridManager;
    private NavigationManager navigationManager;
    //New common manager: PropertyManager
    //private ResidentManager residentManager;
    [SerializeField] VisualTreeAsset buildingListElementTemplate;


    private float timeToAppear = 10f;
    private float timeWhenDisappear;

    const string SELECTED_BUTTON_CLASS_NAME = "selected";
    private void OnEnable()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        selectionModeButton = root.Q<Button>("selection-mode-button");
        buildModeButton = root.Q<Button>("building-mode-button");
        navigationModeButton = root.Q<Button>("navigation-mode-button");
        infoContainer = root.Q<VisualElement>("info-text-container");
        infoContainerText = root.Q<Label>("info-text-label");
        buildingList = root.Q<ListView>("building-list");
        balanceLabel = root.Q<Label>("balance-text");
        electricityLabel = root.Q<Label>("electricity-text");
        coalLabel = root.Q<Label>("coal-text");
        woodLabel = root.Q<Label>("wood-text");
        timeStartStopButton = root.Q<Button>("time-start-stop-button");
        timeForwardButton = root.Q<Button>("time-forward-button");
        timeTextField = root.Q<Label>("time-text-field");
        moralityViewToggle = root.Q<Toggle>("morality-toggle");
        buildingsButton = root.Q<Button>("buildings-button");
        buildingHud = root.Q<VisualElement>("building-hud-container");

        saveButton = root.Q<Button>("save-button");
        
        questText = root.Q<Label>("quest-text");
        taxesButton = root.Q<Button>("taxes-button");
        taxesHud = root.Q<VisualElement>("tax-box-container");

        var taxButtons = root.Query<Button>(null, "tax-button");
        taxButtons.ForEach(SetupButton);



        var taxLabels = root.Query<Label>(null, "tax-label");
        //taxLabels.ForEach(SetupLabel);

        taxLabels.ForEach(labels.Add);
    }
    void Start()
    {
        selectionModeButton.clicked += OnSelectionModeButtonClicked;
        selectionModeButton.AddToClassList(SELECTED_BUTTON_CLASS_NAME);
        buildModeButton.clicked += OnBuildModeButtonClicked;
        navigationModeButton.clicked += OnNavigationModeButtonClicked;

        gameModeSelectorButtons.Add(selectionModeButton);
        gameModeSelectorButtons.Add(buildModeButton);
        gameModeSelectorButtons.Add(navigationModeButton);

        buildModeManager = FindObjectOfType<BuildModeManager>();
        gridManager = FindObjectOfType<GridManager>();
        navigationManager = FindObjectOfType<NavigationManager>();
        //residentManager = FindObjectOfType<ResidentManager>();

        timeStartStopButton.clicked += OnTimeStartStopButtonClicked;
        timeForwardButton.clicked += OnTimeForwardButtonClicked;
        TimeManager.OnMinuteChanged += UpdateTimer;

        moralityViewToggle.RegisterValueChangedCallback(evt =>
        {
            if (evt.newValue)
            {
                gridManager.ChangeMaterialsToMorality();
            }
            else
            {
                gridManager.ResetMaterialsOnFields();
            }
        });

        LoadBuildings();
        buildingsButton.clicked += ChangeVisibleOnBuildingHud;

        saveButton.clicked += gridManager.Save;

        PlayerBalance.OnPlayerStatsChanged += UpdateBalanceText;
        taxesButton.clicked += TaxesMenu;

        UpdateTaxes();
    }

    private void Update()
    {
        if ((questText.style.display == DisplayStyle.Flex) && (Time.time >= timeWhenDisappear))
        {
            questText.style.display = DisplayStyle.None;
        }
    }

    void OnSelectionModeButtonClicked()
    {
        HideSpecialBuildings();
        selectedGameMode = GameMode.SelectionMode;
        ResetOnGameModeChange();
        selectionModeButton.AddToClassList(SELECTED_BUTTON_CLASS_NAME);
    }

    private void ResetOnGameModeChange()
    {
        SetSelectedForGameModeSelectorButtons(false);
        navigationManager.DeselectObjects();
        gridManager.ResetSelection();
        buildingList.SetSelection(-1);
        buildingList.style.display = DisplayStyle.None;
    }

    void OnBuildModeButtonClicked()
    {
        HideSpecialBuildings();
        SetSelectedForGameModeSelectorButtons(false);
        selectedGameMode = GameMode.BuildMode;
        buildModeButton.AddToClassList(SELECTED_BUTTON_CLASS_NAME);
        buildingList.style.display = DisplayStyle.Flex;
        InitializeBuildingsList();
    }

    void OnNavigationModeButtonClicked()
    {
        selectedGameMode = GameMode.NavigationMode;
        ResetOnGameModeChange();
        navigationModeButton.AddToClassList(SELECTED_BUTTON_CLASS_NAME);
    }
    void InitializeBuildingsList()
    {
        var buildings = buildModeManager.GetBuildingDatas();

        buildingList.makeItem = () =>
        {
            var newListEntry = buildingListElementTemplate.Instantiate();
            var newListEntryLogic = new BuildingListEntryController();
            newListEntry.userData = newListEntryLogic;

            newListEntryLogic.SetVisualElement(newListEntry);
            return newListEntry;
        };

        buildingList.bindItem = (item, index) =>
        {
            (item.userData as BuildingListEntryController).SetBuildingData(buildings[index]);
        };
        buildingList.fixedItemHeight = 45;
        buildingList.itemsSource = buildings;

        buildingList.selectionChanged += OnBuildingSelected;
        buildingList.SetSelection(0);
    }

    void OnBuildingSelected(IEnumerable<object> selectedItems)
    {
        var selectedBuilding = buildingList.selectedItem as BuildingData;
        if (selectedBuilding == null)
        {
            gridManager.ChangeSelection(new Vector2Int(1, 1), null, null);
            buildModeManager.BuildingDataSelected(null);
        }
        else
        {
            buildModeManager.BuildingDataSelected(selectedBuilding);
            gridManager.ChangeSelection(selectedBuilding.size, selectedBuilding.buildingType, selectedBuilding.prefab);
        }
    }

    void SetSelectedForGameModeSelectorButtons(bool selected)
    {
        foreach (Button button in gameModeSelectorButtons)
        {
            if (selected)
            {
                button.AddToClassList(SELECTED_BUTTON_CLASS_NAME);
            }
            else
            {
                button.RemoveFromClassList(SELECTED_BUTTON_CLASS_NAME);
            }
        }
    }

    public void ObjectSelected(SelectableObject selectedObject, Dictionary<Vector3, List<Vector3Int>> placingPositionsWithGridPositions)
    {
        switch (selectedGameMode)
        {
            case GameMode.SelectionMode:
                ObjectSelectedInSelectionMode(selectedObject);
                break;
            case GameMode.BuildMode:
                ObjectSelectedInBuildMode(selectedObject, placingPositionsWithGridPositions);
                break;
            case GameMode.NavigationMode:
                ObjectSelectedInNavigationMode(selectedObject);
                break;
            default:
                break;
        }

    }

    void ObjectSelectedInSelectionMode(SelectableObject selectedObject)
    {
        if (selectedObject != null)
        {
            var displayText = selectedObject.GetDescription();
            infoContainer.style.display = DisplayStyle.Flex;
            var tile = selectedObject.GetGameObject().GetComponent<Tile>();
            if (tile != null)
            {
                displayText += " morality:  " + tile.tileMorality.moralityLevel;
            }
            infoContainerText.text = displayText;

        }
        else
        {
            infoContainer.style.display = DisplayStyle.None;
            infoContainerText.text = "";
        }
    }

    void ObjectSelectedInBuildMode(SelectableObject selectedObject, Dictionary<Vector3, List<Vector3Int>> placingPositionsWithGridPositions)
    {
        if (selectedObject != null)
        {
            buildModeManager.ObjectSelected(placingPositionsWithGridPositions);
        }
    }

    void ObjectSelectedInNavigationMode(SelectableObject selectedObject)
    {
        if (selectedObject != null)
        {
            navigationManager.ObjectSelected(selectedObject);
        }
    }

    void OnTimeStartStopButtonClicked()
    {
        TimeManager.instance.StartStopTimer();
    }

    void OnTimeForwardButtonClicked()
    {
        TimeManager.instance.ChangeTimerSpeed();
    }

    public void UpdateTimer() {
        timeTextField.text = $"{TimeManager.Hour:00}:{TimeManager.Minute:00}";
    }

    void LoadBuildings()
    {
        BuildingData[] loadedObjects = Resources.LoadAll<BuildingData>("Buildings");
        foreach (BuildingData obj in loadedObjects)
        {
            if (obj.BuyableBuilding)
            {
                Button tempbutton = new Button();
                tempbutton.text = obj.Name;
                tempbutton.clicked += () => buildModeManager.BuildingDataSelected(obj);
                tempbutton.clicked += () => gridManager.ChangeSelection(obj.size, obj.buildingType, obj.prefab);
                buildingHud.Add(tempbutton);
            }
        }

    }

    void HideSpecialBuildings()
    {
        if (buildingHud.visible)
            buildingHud.visible = !buildingHud.visible;
    }

    void ChangeVisibleOnBuildingHud()
    {
        buildingHud.visible = !buildingHud.visible;
        if (buildingHud.visible)
        {
            ResetOnGameModeChange();
            selectedGameMode = GameMode.BuildMode;
        }
        else
        {
            OnSelectionModeButtonClicked();
        }
    }
    //Todo: saveManager modif upadte...->Update 
    public void UpdateBalanceText()
    {
        balanceLabel.text = PlayerBalance.Balance.ToString() + " $";
        electricityLabel.text = PlayerBalance.Electricity.ToString();
        coalLabel.text = PlayerBalance.Coal.ToString();
        woodLabel.text = PlayerBalance.Wood.ToString();
    }

    public void QuestCompleted(string text)
    {
        questText.style.display = DisplayStyle.Flex;
        questText.text = text;
        timeWhenDisappear = Time.time + timeToAppear;
    }

    void TaxesMenu()
    {
        taxesHud.visible = !taxesHud.visible;
    }

    private void SetupButton(Button button)
    {
        //Subscribe to the clickedWithEventInfo event
        button.clickable.clickedWithEventInfo += Clickable_clickedWithEventInfo;
    }

    private void Clickable_clickedWithEventInfo(EventBase obj)
    {
        var button = (Button)obj.target;
        List<string> buttonFunction = button.name.Split("-").ToList();
        if (buttonFunction[2] == "plus")
        {
            PlayerBalance.instance.IncreaseTaxes(buttonFunction[1], int.Parse(buttonFunction[0]) - 1);
        }
        if (buttonFunction[2] == "minus")
        {
            PlayerBalance.instance.DecreaseTaxes(buttonFunction[1], int.Parse(buttonFunction[0]) - 1);
        }
        UpdateTaxes();
    }
    private void UpdateTaxes()
    {
        for (int i = 0; i < labels.Count; i++)
        {
            string selectedValue = "";
            List<string> labelName = labels[i].name.Split("-").ToList();
            if (labelName[1] == "resident")
            {
                selectedValue = PlayerBalance.instance.ResidentsTaxes[int.Parse(labelName[0]) - 1].ToString();
            }
            else if (labelName[1] == "shop")
            {
                selectedValue = PlayerBalance.instance.ShopTaxes[int.Parse(labelName[0]) - 1].ToString();
            }
            else if (labelName[1] == "factory")
            {
                selectedValue = PlayerBalance.instance.FactoryTaxes[int.Parse(labelName[0]) - 1].ToString();
            }
            labels[i].text = selectedValue + " %";
        }

    }

}
