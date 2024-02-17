using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Tile : MonoBehaviour
{
    List<Vector3> lastHighlight = new List<Vector3>();
    void OnMouseEnter() {
        var manager = FindObjectOfType<GridManager>();
        ClearLastHighlight();
        lastHighlight.AddRange(GetCoordsOfRectangle(transform.position, 3,4));
        foreach(var coord in lastHighlight) {
            var tile = manager.GetTileAtPosition(coord);
            tile.GetComponent<Highlight>()?.ToggleHighlight(true);
        }
    }

    void OnMouseExit() {
        ClearLastHighlight();
    }

    void ClearLastHighlight() {
        var manager = FindObjectOfType<GridManager>();
        foreach(var h in lastHighlight) {
            var tile = manager.GetTileAtPosition(h);
            tile.GetComponent<Highlight>()?.ToggleHighlight(false);
        }
        lastHighlight.Clear();
    }

    List<Vector3> GetCoordsOfRectangle(Vector3 startPos, int height, int width) {
        List<Vector3> positions = new List<Vector3>();
        for (int x = 0; x < height; x++ ) {
            for (int z = 0; z < width; z++) {
                positions.Add(new Vector3(startPos.x + x * 0.5f, 0.0f, startPos.z + z * 0.5f ));
            }
        }
        return positions;
    }
}
