using System;
using System.Collections;
using System.Collections.Generic;
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

    Dictionary<Vector3Int, Tile> tileMap = new Dictionary<Vector3Int, Tile>();
    int offsetX = 10;
    int offsetZ = 10;
    
    // Start is called before the first frame update
    void Start()
    {
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
                tileMap[TransformVector3PositionToInt(newPosition)] = newTile;
                newTile.name = $"{xCoordinate}, {zCoordinate}";
            }
        }
    }

    Vector3Int TransformVector3PositionToInt(Vector3 position) {
        return new Vector3Int((int)(position.x * 2), (int)(position.y * 2), (int)(position.z * 2));
    }

    public Tile GetTileAtPosition(Vector3 position) {
        return tileMap[TransformVector3PositionToInt(position)];
    }
}
