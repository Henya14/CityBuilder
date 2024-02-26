using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public Vector3Int gridPosition {get; set;}
    [SerializeField] public string description;
    public Vector2Int tileSize {get; set;} = new Vector2Int(1,1); 
    void OnMouseEnter() {
        var manager = FindObjectOfType<GridManager>();
        manager.TileSelectedAtPosition(gridPosition);
    }

    void OnMouseExit() {
        var manager = FindObjectOfType<GridManager>();
        manager.TileDeselectedAtPosition(gridPosition);
    }
}
