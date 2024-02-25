using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BuildingListEntryController
{
    Label NameLabel;

    public void SetVisualElement(VisualElement visualElement) {
        NameLabel = visualElement.Q<Label>("building-name");
    }

    public void SetBuildingData(BuildingData buildingData) {
        NameLabel.text = buildingData.name;
    } 
}
