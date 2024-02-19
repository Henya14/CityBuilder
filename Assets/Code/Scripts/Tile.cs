using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] public string description;
    void OnMouseEnter() {
        var manager = FindObjectOfType<GridManager>();
        manager.TileSelectedAtPosition(transform.position);
    }

    void OnMouseExit() {
        var manager = FindObjectOfType<GridManager>();
        manager.TileDeselectedAtPosition(transform.position);
    }
}
