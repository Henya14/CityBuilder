using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;
using UnityEngine.Tilemaps;


public enum SelectionMode {
    Single,
    Line,
    Rectangle
}
public class GridManager : MonoBehaviour
{

    [SerializeField] Tile[] tiles;
    [SerializeField] int gridHeight = 20;
    [SerializeField] int gridWidth = 20;
    [SerializeField] float tileSize = 0.5f;
    [SerializeField] float chanceToSwitchTile = 0.3f;
    [SerializeField] Tile previousTilePlaced = null;
    bool isMouseButtonDown = false;
    SelectionMode selectionMode = SelectionMode.Single;
    public Vector2Int selectionSize {get; set;} = new Vector2Int(1,1); 
    public GameObject selectionPrefab {get; set;} 
    Vector3Int lastTilePositionMouseHoveredOver;
    GameObject selectionInstance;
    List<Vector3Int> lastSelectedTilePositions = new List<Vector3Int>();
    GameUIManager gameUIManager;


    Dictionary<Vector3Int, Tile> tileMap = new Dictionary<Vector3Int, Tile>();
    Dictionary<Vector3Int, AbstractBuildingType> buildingsMap = new Dictionary<Vector3Int, AbstractBuildingType>();
    int offsetX = 10;
    int offsetZ = 10;
    
    // Start is called before the first frame update
    void Start()
    {
        gameUIManager = FindObjectOfType<GameUIManager>();
        offsetX = gridHeight / 4;
        offsetZ = gridWidth / 4;
        GenerateGrid();
    }

    private void GenerateGrid()
    {   
        
        for (int x  = 0; x < gridHeight; x++) {
            for (int z = 0; z < gridWidth; z++) {
                Tile tileToPlace;
                if (previousTilePlaced) {
                    tileToPlace = UnityEngine.Random.Range(0.0f, 1.0f) <= chanceToSwitchTile ? tiles[UnityEngine.Random.Range(0, tiles.Length)]: previousTilePlaced;
                }  else {
                    tileToPlace = tiles[UnityEngine.Random.Range(0, tiles.Length)];
                }
                previousTilePlaced = tileToPlace;
                Tile newTile = Instantiate(tileToPlace, transform);
                Vector3Int gridPosition =  new Vector3Int(x, 0, z);
                newTile.gridPosition = gridPosition;

                Vector3 newPosition =  GetGamePositionForGridPosition(gridPosition);

                newTile.transform.position = newPosition;
                
                tileMap[newTile.gridPosition] = newTile;
                newTile.name = $"{newPosition.x}, {newPosition.z}";
            }
        }
    }

    public void TileSelectedAtPosition(Vector3Int selectionPosition) {
        lastTilePositionMouseHoveredOver = selectionPosition;
        if (selectionMode == SelectionMode.Single) {
            ClearlastSelectedTilePositions();
            lastSelectedTilePositions.AddRange(GetPositionsOfTilesInSelection(selectionPosition, selectionSize));
        } else if (selectionMode == SelectionMode.Rectangle) {
            if(isMouseButtonDown) {
                var firstItem = lastSelectedTilePositions[0];
                ClearlastSelectedTilePositions();
                lastSelectedTilePositions = GetPositionsOfTilesInRectangleBetweenTwoPositions(firstItem, selectionPosition);
            } else {
                lastSelectedTilePositions.Clear();
                lastSelectedTilePositions.Add(selectionPosition);
            }
        }
        SetSelectionOfTilesAtPositions(lastSelectedTilePositions, true);

        if (selectionInstance == null && selectionPrefab != null) {
            selectionInstance = Instantiate(selectionPrefab);
        } 

        if (selectionInstance != null) {
            var gamePosition = GetSelectionCenter(lastSelectedTilePositions);
            gamePosition.y += tileSize;
            selectionInstance.transform.position = gamePosition;
        }
    }

    public void TileDeselectedAtPosition(Vector3Int position) {
        if (!isMouseButtonDown) { 
            ClearlastSelectedTilePositions();
        }
    }

    List<Vector3Int> GetPositionsOfTilesInSelection(Vector3Int startTilePosition, Vector2 selectionSize) {
        List<Vector3Int> positions = new List<Vector3Int>();
        for (int x = 0; x < selectionSize.x; x++) {
            for (int z = 0; z < selectionSize.y; z++) {
                positions.Add(new Vector3Int(startTilePosition.x + x, 0, startTilePosition.z + z));
            }
        }
        return positions;
    }

     List<Vector3Int> GetPositionsOfTilesInRectangleBetweenTwoPositions(Vector3Int startPosition, Vector3Int endPosition) {
        List<Vector3Int> positions = new List<Vector3Int>
        {
            startPosition
        };

        var newStartPos = new Vector3Int(Math.Min(startPosition.x, endPosition.x), 0, Math.Min(startPosition.z, endPosition.z));
        var newEndPosition = new Vector3Int(Math.Max(startPosition.x, endPosition.x), 0, Math.Max(startPosition.z, endPosition.z));

        for (int x = newStartPos.x; x <= newEndPosition.x; x++) {
            for (int z = newStartPos.z; z <= newEndPosition.z; z++) {
                var newPos = new Vector3Int(x, 0, z);
                if (newPos != positions[0]) {
                    positions.Add(new Vector3Int(x, 0, z));
                }
            }
        }
        return positions;
     }

    void ClearlastSelectedTilePositions() {
        SetSelectionOfTilesAtPositions(lastSelectedTilePositions, false);
        lastSelectedTilePositions.Clear();
    }

    void SetSelectionOfTilesAtPositions(List<Vector3Int> tilePositions, bool selected) {
            foreach(var tilePosition in tilePositions) {
            var tile = GetTileAtPosition(tilePosition);
            if (tile != null) {
                tile.GetComponent<Highlight>()?.ToggleHighlight(selected);
            }
        }
    }

    public Tile GetTileAtPosition(Vector3Int position) {
        return tileMap.GetValueOrDefault(position, null);
    }

    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            isMouseButtonDown = true;
            if (lastSelectedTilePositions.Count > 0) {
                var selectedTile = GetTileAtPosition(lastSelectedTilePositions[0]);
                gameUIManager.TileSelected(selectedTile, GetSelectionCenter(lastSelectedTilePositions));
            } else {
                gameUIManager.TileSelected(null, new Vector3(0,0,0));
            }
        } else if (Input.GetMouseButtonUp(0)) {
            isMouseButtonDown = false;
            if (selectionInstance != null) {
                Destroy(selectionInstance);
            }
            if (selectionMode == SelectionMode.Rectangle) {
                ClearlastSelectedTilePositions();
                lastSelectedTilePositions.Add(lastTilePositionMouseHoveredOver);
                SetSelectionOfTilesAtPositions(lastSelectedTilePositions, true);
            }
        }
    }

    public void ChangeSelection(Vector2Int selectionSize, BuildingType? buildingType, GameObject prefabToShowAtSelection) {
        this.selectionSize = selectionSize;
        if (selectionInstance != null) {
            Destroy(selectionInstance);
            selectionInstance = null;
        }
        switch(buildingType) {
            case BuildingType.Building: 
                selectionMode = SelectionMode.Single;
                break;
            case BuildingType.Zone: 
                selectionMode = SelectionMode.Rectangle;
                break;
            case BuildingType.Road:
                selectionMode = SelectionMode.Line;
                break;
            default: 
                selectionMode = SelectionMode.Single;
                break;
        }
        selectionPrefab = prefabToShowAtSelection;
    }

    public void AddBuildingToGrid(AbstractBuildingType building) {
        buildingsMap.Add(building.gridPosition, building);
    }

    public Vector3 GetGamePositionForGridPosition(Vector3Int gridPosition) {
        float x = gridPosition.x * tileSize - offsetX;
        float y = gridPosition.y * tileSize;
        float z = gridPosition.z * tileSize - offsetZ;
        Vector3 gamePosition = new Vector3(x, y, z);
        return gamePosition;
    }

    private Vector3 GetSelectionCenter(List<Vector3Int> selectedTiles) {
        var xMin = selectedTiles.Select(v => v.x).Min();
        var zMin = selectedTiles.Select(v => v.z).Min();

        var xMax = selectedTiles.Select(v => v.x).Max();
        var zMax = selectedTiles.Select(v => v.z).Max();

        var selectionStart = GetGamePositionForGridPosition(new Vector3Int(xMin - 1, 0, zMin));
        var selectionEnd = GetGamePositionForGridPosition(new Vector3Int(xMax, 0, zMax + 1));

        Vector3 selectionVector = (selectionEnd - selectionStart) / 2;
        Vector3 selectionCenter = selectionVector + selectionStart;
        return selectionCenter;
    }
}
