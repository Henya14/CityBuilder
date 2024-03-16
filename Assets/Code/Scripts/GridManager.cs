using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;
using UnityEngine.Tilemaps;


public enum SelectionMode
{
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
    public Vector2Int selectionSize { get; set; } = new Vector2Int(1, 1);
    public GameObject selectionPrefab { get; set; }
    GameObject selectionInstance;
    List<Vector3Int> lastSelectedTilePositions = new List<Vector3Int>();
    GameUIManager gameUIManager;
    bool isSelectionValid = false;



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

        for (int x = 0; x < gridHeight; x++)
        {
            for (int z = 0; z < gridWidth; z++)
            {
                Tile tileToPlace;
                if (previousTilePlaced)
                {
                    tileToPlace = UnityEngine.Random.Range(0.0f, 1.0f) <= chanceToSwitchTile ? tiles[UnityEngine.Random.Range(0, tiles.Length)] : previousTilePlaced;
                }
                else
                {
                    tileToPlace = tiles[UnityEngine.Random.Range(0, tiles.Length)];
                }
                previousTilePlaced = tileToPlace;
                Tile newTile = Instantiate(tileToPlace, transform);
                Vector3Int gridPosition = new Vector3Int(x, 0, z);
                newTile.gridPosition = gridPosition;

                Vector3 newPosition = GetGamePositionForGridPosition(gridPosition);

                newTile.transform.position = newPosition;

                tileMap[newTile.gridPosition] = newTile;
                newTile.name = $"{newPosition.x}, {newPosition.z}";
            }
        }
    }

    public void TileSelectedAtPosition(Vector3Int selectionPosition)
    {
        if (selectionMode == SelectionMode.Single)
        {
            ClearlastSelectedTilePositions();
            lastSelectedTilePositions.AddRange(GetPositionsOfTilesInSelection(selectionPosition, selectionSize));
        }
        else if (selectionMode == SelectionMode.Rectangle || selectionMode == SelectionMode.Line)
        {
            if (isMouseButtonDown)
            {
                HandleSelectionWhenMouseIsDown(selectionPosition);
            }
            else
            {
                lastSelectedTilePositions.Clear();
                lastSelectedTilePositions.Add(selectionPosition);
            }
        }
        isSelectionValid = IsSelectionValid(lastSelectedTilePositions);
        if (isSelectionValid)
        {

            SetSelectionOfTilesAtPositions(lastSelectedTilePositions, true);
            if (selectionInstance == null && selectionPrefab != null && selectionMode == SelectionMode.Single)
            {
                selectionInstance = Instantiate(selectionPrefab);
            }

            if (selectionInstance != null)
            {
                var gamePosition = GetSelectionCenter(lastSelectedTilePositions);
                gamePosition.y += tileSize;
                selectionInstance.transform.position = gamePosition;
            }
        }
        else
        {
            Destroy(selectionInstance);
        }
    }

    private void HandleSelectionWhenMouseIsDown(Vector3Int selectionPosition)
    {
        if (lastSelectedTilePositions.Count == 0)
        {
            return;
        }
        var firstSelectionPosition = lastSelectedTilePositions[0];
        ClearlastSelectedTilePositions();
        var secondSelectionPosition = selectionPosition;
        if (selectionMode == SelectionMode.Line)
        {
            var deltaX = secondSelectionPosition.x - firstSelectionPosition.x;
            var deltaZ = secondSelectionPosition.z - firstSelectionPosition.z;
            if (Math.Sign(deltaX) == Math.Sign(deltaZ) || deltaX == 0)
            {
                secondSelectionPosition.x = firstSelectionPosition.x;
            }
            else
            {
                secondSelectionPosition.z = firstSelectionPosition.z;
            }
        }
        lastSelectedTilePositions = GetPositionsOfTilesInRectangleBetweenTwoPositions(firstSelectionPosition, secondSelectionPosition);
    }

    private bool IsSelectionValid(List<Vector3Int> selectedTilePositions)
    {
        bool isValid = true;
        var selectionMaxX = selectedTilePositions.Select(p => p.x).Max();
        var tileMapMaxX = tileMap.Keys.Select(p => p.x).Max();
        var tileMapMaxZ = tileMap.Keys.Select(p => p.z).Max();
        var selectionMaxZ = selectedTilePositions.Select(p => p.z).Max();
        if (selectionMaxX > tileMapMaxX || selectionMaxZ > tileMapMaxZ)
        {
            return false;
        }
        var buildingPositions = buildingsMap.Keys.Select(p => new Vector2(p.x, p.z));
        var selectedTilePositionsVec2 = selectedTilePositions.Select(p => new Vector2(p.x, p.z));

        foreach (var selectedTilePosition in selectedTilePositionsVec2)
        {
            if (buildingPositions.Contains(selectedTilePosition))
            {
                isValid = false;
            }
        }



        return isValid;
    }

    public void TileDeselectedAtPosition(Vector3Int position)
    {
        if (!isMouseButtonDown)
        {
            ClearlastSelectedTilePositions();
        }
    }

    List<Vector3Int> GetPositionsOfTilesInSelection(Vector3Int startTilePosition, Vector2 selectionSize)
    {
        List<Vector3Int> positions = new List<Vector3Int>();
        for (int x = 0; x < selectionSize.x; x++)
        {
            for (int z = 0; z < selectionSize.y; z++)
            {
                positions.Add(new Vector3Int(startTilePosition.x + x, 0, startTilePosition.z + z));
            }
        }
        return positions;
    }

    List<Vector3Int> GetPositionsOfTilesInRectangleBetweenTwoPositions(Vector3Int startPosition, Vector3Int endPosition)
    {
        List<Vector3Int> positions = new List<Vector3Int>
        {
            startPosition
        };

        var newStartPos = new Vector3Int(Math.Min(startPosition.x, endPosition.x), 0, Math.Min(startPosition.z, endPosition.z));
        var newEndPosition = new Vector3Int(Math.Max(startPosition.x, endPosition.x), 0, Math.Max(startPosition.z, endPosition.z));

        for (int x = newStartPos.x; x <= newEndPosition.x; x++)
        {
            for (int z = newStartPos.z; z <= newEndPosition.z; z++)
            {
                var newPos = new Vector3Int(x, 0, z);
                if (newPos != positions[0])
                {
                    positions.Add(new Vector3Int(x, 0, z));
                }
            }
        }
        return positions;
    }

    void ClearlastSelectedTilePositions()
    {
        SetSelectionOfTilesAtPositions(lastSelectedTilePositions, false);
        lastSelectedTilePositions.Clear();
    }

    void SetSelectionOfTilesAtPositions(List<Vector3Int> tilePositions, bool selected)
    {
        foreach (var tilePosition in tilePositions)
        {
            var tile = GetTileAtPosition(tilePosition);
            if (tile != null)
            {
                tile.GetComponent<Highlight>()?.ToggleHighlight(selected);
            }
        }
    }

    public Tile GetTileAtPosition(Vector3Int position)
    {
        return tileMap.GetValueOrDefault(position, null);
    }

    public AbstractBuildingType GetBuildingAtPosition(Vector3Int position)
    {
        return buildingsMap.GetValueOrDefault(position, null);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isMouseButtonDown = true;
            if (lastSelectedTilePositions.Count > 0 && selectionMode == SelectionMode.Single && isSelectionValid)
            {
                var selectedTile = GetTileAtPosition(lastSelectedTilePositions[0]);
                gameUIManager.TileSelected(selectedTile, lastSelectedTilePositions, new List<Vector3> { GetSelectionCenter(lastSelectedTilePositions) });
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isMouseButtonDown = false;
            if (selectionInstance != null)
            {
                Destroy(selectionInstance);
            }
            if ((selectionMode == SelectionMode.Rectangle || selectionMode == SelectionMode.Line) && lastSelectedTilePositions.Count > 0 && isSelectionValid)
            {
                var selectedTile = GetTileAtPosition(lastSelectedTilePositions[0]);
                var prefabPlacePositions = lastSelectedTilePositions.Select(lstp => GetSelectionCenter(new List<Vector3Int> { lstp })).ToList();
                gameUIManager.TileSelected(selectedTile, lastSelectedTilePositions, prefabPlacePositions);
                ClearlastSelectedTilePositions();
            }
        }
    }

    public void ChangeSelection(Vector2Int selectionSize, BuildingType? buildingType, GameObject prefabToShowAtSelection)
    {
        this.selectionSize = selectionSize;
        if (selectionInstance != null)
        {
            Destroy(selectionInstance);
            selectionInstance = null;
        }
        switch (buildingType)
        {
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

    public void AddBuildingToGrid(AbstractBuildingType building, List<Vector3Int> gridPositions)
    {
        foreach (var gridPosition in gridPositions)
        {
            buildingsMap.Add(gridPosition, building);
        }
    }

    public Vector3 GetGamePositionForGridPosition(Vector3Int gridPosition)
    {
        float x = gridPosition.x * tileSize - offsetX;
        float y = gridPosition.y * tileSize;
        float z = gridPosition.z * tileSize - offsetZ;
        Vector3 gamePosition = new Vector3(x, y, z);
        return gamePosition;
    }

    private Vector3 GetSelectionCenter(List<Vector3Int> selectedTiles)
    {
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

    public void ResetSelection()
    {
        ClearlastSelectedTilePositions();
        ChangeSelection(new Vector2Int(1, 1), null, null);
    }

    public Dictionary<Direction, AbstractBuildingType> GetNeigbouringBuildingsOfTile(Vector3Int tilePosition)
    {
        Dictionary<Direction, AbstractBuildingType> neighborDictionary = new Dictionary<Direction, AbstractBuildingType>();

        neighborDictionary[Direction.North] = GetBuildingAtPosition(new Vector3Int(tilePosition.x, tilePosition.y, tilePosition.z + 1));
        neighborDictionary[Direction.South] = GetBuildingAtPosition(new Vector3Int(tilePosition.x, tilePosition.y, tilePosition.z - 1));
        neighborDictionary[Direction.East] = GetBuildingAtPosition(new Vector3Int(tilePosition.x + 1, tilePosition.y, tilePosition.z));
        neighborDictionary[Direction.West] = GetBuildingAtPosition(new Vector3Int(tilePosition.x - 1, tilePosition.y, tilePosition.z));

        return neighborDictionary;

    }
}
