using System;
using System.Collections;
using System.Collections.Generic;
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


    List<Button> gameModeSelectorButtons = new List<Button>();
    public GameMode selectedGameMode {get; set;} = GameMode.SelectionMode;

    private BuildModeManager buildModeManager;
    private GridManager gridManager;
    [SerializeField] VisualTreeAsset buildingListElementTemplate;

    const string SELECTED_BUTTON_CLASS_NAME = "selected";
    private void OnEnable() {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        selectionModeButton = root.Q<Button>("selection-mode-button");
        buildModeButton = root.Q<Button>("building-mode-button");
        infoContainer = root.Q<VisualElement>("info-text-container");
        infoContainerText = root.Q<Label>("info-text-label");
        buildingList = root.Q<ListView>("building-list");
    }
    void Start()
    {
        selectionModeButton.clicked += OnSelectionModeButtonClicked;
        selectionModeButton.AddToClassList(SELECTED_BUTTON_CLASS_NAME);
        buildModeButton.clicked += OnBuildModeButtonClicked;

        gameModeSelectorButtons.Add(selectionModeButton);
        gameModeSelectorButtons.Add(buildModeButton);

        buildModeManager = FindObjectOfType<BuildModeManager>();
        gridManager = FindObjectOfType<GridManager>();
    }

    void OnSelectionModeButtonClicked() {
        SetSelectedForGameModeSelectorButtons(false);
        selectedGameMode = GameMode.SelectionMode;
        selectionModeButton.AddToClassList(SELECTED_BUTTON_CLASS_NAME);
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
    }

    void OnBuildingSelected(IEnumerable<object> selectedItems) {
        var selectedBuilding = buildingList.selectedItem as BuildingData;
        if (selectedBuilding == null) {
            gridManager.ChangeSelection(new Vector2Int(1,1), null);
        } else {
            buildModeManager.BuildingDataSelected(selectedBuilding);
            gridManager.ChangeSelection(selectedBuilding.size, selectedBuilding.prefab);
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

    public void TileSelected(Tile tile, Vector3 selectionCenter) {
        switch (selectedGameMode) {
            case GameMode.SelectionMode: 
                TileSelectedInSelectionMode(tile);
                break;
            case GameMode.BuildMode:
                TileSelectedInBuildMode(tile, selectionCenter);
                break;
            default:
                break;
        }
       
    }

    void TileSelectedInSelectionMode(Tile tile) {
         if (tile != null) {
            infoContainer.style.display = DisplayStyle.Flex;
            infoContainerText.text = tile.description;
        } else {
            infoContainer.style.display = DisplayStyle.None;
            infoContainerText.text = "";
        }
    }

    void TileSelectedInBuildMode(Tile tile, Vector3 selectionCenter) {
         if (tile != null) {
            buildModeManager.TileSelected(tile, selectionCenter);
        }
    }
}
