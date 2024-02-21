using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public Vector3Int positionInt {get; set;}
    [SerializeField] public string description;
    void OnMouseEnter() {
        var manager = FindObjectOfType<GridManager>();
        manager.TileSelectedAtPosition(positionInt);
    }

    void OnMouseExit() {
        var manager = FindObjectOfType<GridManager>();
        manager.TileDeselectedAtPosition(positionInt);
    }
}
