using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UIElements;

public enum GameMode {
    SelectionMode,
    BuildMode
}
public class GameUIManager : MonoBehaviour
{
    Button selectionModeButton;
    Button buildModeButton;
    VisualElement infoContainer;
    Label infoContainerText;
    ListView buildingList;

    Button timeStartStopButton;
    Button timeForwardButton;
    Label timeTextField;

    Toggle moralityViewToggle;

    Button buildingsButton;
    VisualElement buildingHud;


    List<Button> gameModeSelectorButtons = new List<Button>();
    public GameMode selectedGameMode {get; set;} = GameMode.SelectionMode;

    private BuildModeManager buildModeManager;
    private GridManager gridManager;
    private ResidentManager residentManager;
    [SerializeField] VisualTreeAsset buildingListElementTemplate;

    const string SELECTED_BUTTON_CLASS_NAME = "selected";
    private void OnEnable() {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        selectionModeButton = root.Q<Button>("selection-mode-button");
        buildModeButton = root.Q<Button>("building-mode-button");
        infoContainer = root.Q<VisualElement>("info-text-container");
        infoContainerText = root.Q<Label>("info-text-label");
        buildingList = root.Q<ListView>("building-list");

        timeStartStopButton = root.Q<Button>("time-start-stop-button");
        timeForwardButton = root.Q<Button>("time-forward-button");
        timeTextField = root.Q<Label>("time-text-field");

        moralityViewToggle = root.Q<Toggle>("morality-toggle");

        buildingsButton = root.Q<Button>("buildings-button");
        buildingHud = root.Q<VisualElement>("building-hud-container");

    }
    void Start() {
        selectionModeButton.clicked += OnSelectionModeButtonClicked;
        selectionModeButton.AddToClassList(SELECTED_BUTTON_CLASS_NAME);
        buildModeButton.clicked += OnBuildModeButtonClicked;

        gameModeSelectorButtons.Add(selectionModeButton);
        gameModeSelectorButtons.Add(buildModeButton);

        buildModeManager = FindObjectOfType<BuildModeManager>();
        gridManager = FindObjectOfType<GridManager>();
        residentManager = FindObjectOfType<ResidentManager>();

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
    }

    void OnSelectionModeButtonClicked() {
        SetSelectedForGameModeSelectorButtons(false);
        selectedGameMode = GameMode.SelectionMode;
        selectionModeButton.AddToClassList(SELECTED_BUTTON_CLASS_NAME);
        gridManager.ResetSelection();
        buildingList.style.display = DisplayStyle.None;
    }

    void OnBuildModeButtonClicked() {
        SetSelectedForGameModeSelectorButtons(false);
        selectedGameMode = GameMode.BuildMode;
        buildModeButton.AddToClassList(SELECTED_BUTTON_CLASS_NAME);
        buildingList.style.display = DisplayStyle.Flex;
        InitializeBuildingsList();
    }
    void InitializeBuildingsList() {
        var buildings = buildModeManager.GetBuildingDatas();

        buildingList.makeItem = () => {
                var newListEntry = buildingListElementTemplate.Instantiate();
                var newListEntryLogic = new BuildingListEntryController();
                newListEntry.userData = newListEntryLogic;

                newListEntryLogic.SetVisualElement(newListEntry);
                return newListEntry;
            };

        buildingList.bindItem = (item, index) => {
            (item.userData as BuildingListEntryController).SetBuildingData(buildings[index]);
        };
        buildingList.fixedItemHeight = 45;
        buildingList.itemsSource = buildings;

        buildingList.selectionChanged += OnBuildingSelected; 
        buildingList.SetSelection(-1);
    }

    void OnBuildingSelected(IEnumerable<object> selectedItems) {
        var selectedBuilding = buildingList.selectedItem as BuildingData;
        if (selectedBuilding == null) {
            gridManager.ChangeSelection(new Vector2Int(1,1), null, null);
        } else {
            buildModeManager.BuildingDataSelected(selectedBuilding);
            gridManager.ChangeSelection(selectedBuilding.size, selectedBuilding.buildingType, selectedBuilding.prefab);
        }
    }

    void SetSelectedForGameModeSelectorButtons(bool selected) {
        foreach (Button button in  gameModeSelectorButtons) {
            if (selected) {
                button.AddToClassList(SELECTED_BUTTON_CLASS_NAME);
            } else {
                button.RemoveFromClassList(SELECTED_BUTTON_CLASS_NAME);
            }
        }
    }

    public void TileSelected(Tile tile, List<Vector3Int>  selectedTilesGridPositions, List<Vector3> prafabPlacePositions) {
        switch (selectedGameMode) {
            case GameMode.SelectionMode: 
                TileSelectedInSelectionMode(tile);
                break;
            case GameMode.BuildMode:
                TileSelectedInBuildMode(tile, selectedTilesGridPositions, prafabPlacePositions);
                break;
            default:
                break;
        }
       
    }

    void TileSelectedInSelectionMode(Tile tile) {
         if (tile != null) {
            infoContainer.style.display = DisplayStyle.Flex;
            infoContainerText.text = tile.description + " morality:  " + tile.tileMorality.moralityLevel;
        } else {
            infoContainer.style.display = DisplayStyle.None;
            infoContainerText.text = "";
        }
    }

    void TileSelectedInBuildMode(Tile tile, List<Vector3Int> selectedTilesGridPositions, List<Vector3> prefabPlacePositions) {
         if (tile != null) {
            buildModeManager.TileSelected(tile, selectedTilesGridPositions, prefabPlacePositions);
        }
    }

    void OnTimeStartStopButtonClicked() {
        TimeManager.instance.StartStopTimer();
    }

    void OnTimeForwardButtonClicked() {
        TimeManager.instance.ChangeTimerSpeed();
    }

    void UpdateTimer() {
        timeTextField.text = $"{TimeManager.Hour:00}:{TimeManager.Minute:00}";
    }

    void LoadBuildings() {

        BuildingData[] loadedObjects = Resources.LoadAll<BuildingData>("Buildings");

        Debug.Log(loadedObjects.Length);
        foreach (BuildingData obj in loadedObjects) {
            if (obj.BuyableBuilding) {
                Button tempbutton = new Button();
                if(obj.BuildingPicture == null) {
                    tempbutton.text = obj.buildingName;
                }
                else {
                    tempbutton.text = "";
                    tempbutton.style.backgroundImage = obj.image;
                }
                
                tempbutton.clicked += () => buildModeManager.BuildingDataSelected(obj);
                tempbutton.clicked += () => gridManager.ChangeSelection(obj.size, obj.buildingType, obj.prefab);
                buildingHud.Add(tempbutton);
            }
        }
        
    }

    void ChangeVisibleOnBuildingHud() {
        Debug.Log("click");
        buildingHud.visible = !buildingHud.visible; 
        if(buildingHud.visible) {
            selectedGameMode = GameMode.BuildMode;
        }
        else {
            selectedGameMode = GameMode.SelectionMode;
        }
        Debug.Log(selectedGameMode);
    }
}
