using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;



public class GridManager : MonoBehaviour
{

    [SerializeField] Tile[] tiles;
    [SerializeField] int gridHeight = 20;
    [SerializeField] int gridWidth = 20;
    [SerializeField] float tileSize = 0.5f;
    [SerializeField] float chanceToSwitchTile = 0.3f;
    [SerializeField] Tile previousTilePlaced = null;
    public Vector2Int selectionSize {get; set;} = new Vector2Int(1,1); 
    public GameObject selectionPrefab {get; set;} 
    GameObject selectionInstance;
    List<Vector3Int> lastSelectedTilePositions = new List<Vector3Int>();
    GameUIManager gameUIManager;


    Dictionary<Vector3Int, Tile> tileMap = new Dictionary<Vector3Int, Tile>();
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
                float xCoordinate = x * tileSize - offsetX;
                float zCoordinate = z * tileSize - offsetZ;
                Vector3 newPosition = new Vector3(xCoordinate, 0.0f, zCoordinate);

                newTile.transform.position = newPosition;
                newTile.positionInt = new Vector3Int(x, 0, z);
                tileMap[newTile.positionInt] = newTile;
                newTile.name = $"{xCoordinate}, {zCoordinate}";
            }
        }
    }

    public void TileSelectedAtPosition(Vector3Int position) {
        ClearlastSelectedTilePositions();
        lastSelectedTilePositions.AddRange(GetPositionsOfTilesInSelection(position, selectionSize));
        SetSelectionOfTilesAtPositions(lastSelectedTilePositions, true);

        if (selectionInstance == null && selectionPrefab != null) {
            selectionInstance = Instantiate(selectionPrefab);
            
        } 

        if (selectionInstance != null) {
            selectionInstance.transform.position = new Vector3(position.x * tileSize - offsetX, tileSize,  (position.z + selectionSize.y/2) * tileSize - offsetZ);
        }
    }

    public void TileDeselectedAtPosition(Vector3Int position) {
        ClearlastSelectedTilePositions();
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
            if (lastSelectedTilePositions.Count > 0) {
                gameUIManager.TileSelected(GetTileAtPosition(lastSelectedTilePositions[0]));
            } else {
                gameUIManager.TileSelected(null);
            }
        }
    }

    public void ChangeSelection(Vector2Int selectionSize, GameObject prefabToShowAtSelection) {
        this.selectionSize = selectionSize;
        if (selectionInstance != null) {
            Destroy(selectionInstance);
            selectionInstance = null;
        }
        selectionPrefab = prefabToShowAtSelection;
    }
}
