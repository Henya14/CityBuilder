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


    List<Button> gameModeSelectorButtons = new List<Button>();
    public GameMode selectedGameMode {get; set;} = GameMode.SelectionMode;

    const string SELECTED_BUTTON_CLASS_NAME = "selected";
    private void OnEnable() {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        selectionModeButton = root.Q<Button>("SelectionModeButton");
        buildModeButton = root.Q<Button>("BuildingModeButton");
        infoContainer = root.Q<VisualElement>("InfoTextContainer");
        infoContainerText = root.Q<Label>("InfoText");
    }
    void Start()
    {
        selectionModeButton.clicked += OnSelectionModeButtonClicked;
        selectionModeButton.AddToClassList(SELECTED_BUTTON_CLASS_NAME);
        buildModeButton.clicked += OnBuildModeButtonClicked;

        gameModeSelectorButtons.Add(selectionModeButton);
        gameModeSelectorButtons.Add(buildModeButton);

    }

    void OnSelectionModeButtonClicked() {
        SetSelectedForGameModeSelectorButtons(false);
        selectedGameMode = GameMode.SelectionMode;
        selectionModeButton.AddToClassList(SELECTED_BUTTON_CLASS_NAME);
    }

    void OnBuildModeButtonClicked() {
        SetSelectedForGameModeSelectorButtons(false);
        selectedGameMode = GameMode.BuildMode;
        buildModeButton.AddToClassList(SELECTED_BUTTON_CLASS_NAME);
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

    public void TileSelected(Tile tile) {
        switch (selectedGameMode) {
            case GameMode.SelectionMode: 
                TileSelectedInSelectionMode(tile);
                break;
            case GameMode.BuildMode: 
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
}
