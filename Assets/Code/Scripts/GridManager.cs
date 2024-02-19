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
    Vector2Int selectionSize = new Vector2Int(1,1); 
    List<Vector3> lastSelectedTilePositions = new List<Vector3>();
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
                tileMap[TransformVector3ToVector3Int(newPosition)] = newTile;
                newTile.name = $"{xCoordinate}, {zCoordinate}";
            }
        }
    }

    public void TileSelectedAtPosition(Vector3 position) {
        ClearlastSelectedTilePositions();
        lastSelectedTilePositions.AddRange(GetPositionsOfTilesInSelection(position, selectionSize));
        SetSelectionOfTilesAtPositions(lastSelectedTilePositions, true);
    }

    public void TileDeselectedAtPosition(Vector3 position) {
        ClearlastSelectedTilePositions();
    }

    List<Vector3> GetPositionsOfTilesInSelection(Vector3 startTilePosition, Vector2 selectionSize) {
        List<Vector3> positions = new List<Vector3>();
        for (int x = 0; x < selectionSize.x; x++ ) {
            for (int z = 0; z < selectionSize.y; z++) {
                positions.Add(new Vector3(startTilePosition.x + x * tileSize, 0.0f, startTilePosition.z + z * tileSize));
            }
        }
        return positions;
    }

    void ClearlastSelectedTilePositions() {
        SetSelectionOfTilesAtPositions(lastSelectedTilePositions, false);
        lastSelectedTilePositions.Clear();
    }

    void SetSelectionOfTilesAtPositions(List<Vector3> tilePositions, bool selected) {
            foreach(var tilePosition in tilePositions) {
            var tile = GetTileAtPosition(tilePosition);
            if (tile != null) {
                tile.GetComponent<Highlight>()?.ToggleHighlight(selected);
            }
        }
    }

    Vector3Int TransformVector3ToVector3Int(Vector3 position) {
        return new Vector3Int((int)(position.x * 2), (int)(position.y * 2), (int)(position.z * 2));
    }

    public Tile GetTileAtPosition(Vector3 position) {
        return tileMap.GetValueOrDefault(TransformVector3ToVector3Int(position), null);
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
}
