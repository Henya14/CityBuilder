using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;

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
    [SerializeField] public float tileSize = 0.5f;
    [SerializeField] float chanceToSwitchTile = 0.3f;
    [SerializeField] Tile previousTilePlaced = null;
    bool isMouseButtonDown = false;
    SelectionMode selectionMode = SelectionMode.Single;
    public Vector2Int selectionSize { get; set; } = new Vector2Int(1, 1);
    public GameObject selectionPrefab { get; set; }
    GameObject selectionInstance;
    List<Vector3Int> lastSelectedObjectPositions = new List<Vector3Int>();
    bool isSelectionValid = false;
    GameUIManager gameUIManager;
    float cooldown;

    bool notOnMap = true;
    [SerializeField] GameObject debugCube;
    [SerializeField] GameObject debugArrow;
    [SerializeField] GameObject carPrefab;

    List<GameObject> cubes = new List<GameObject>();
    List<GameObject> arrows = new List<GameObject>();


    Dictionary<Vector3Int, Tile> tileMap = new Dictionary<Vector3Int, Tile>();
    Dictionary<Vector3Int, AbstractBuildingType> buildingsMap = new Dictionary<Vector3Int, AbstractBuildingType>();
    Dictionary<Vector3Int, AbstarctProperty> propertyMap = new Dictionary<Vector3Int, AbstarctProperty>();
    int offsetX = 10;
    int offsetZ = 10;

    [SerializeField] SaveLoadManager saveLoadManager;
    [SerializeField] public static bool loadOnStart = false;

    // Start is called before the first frame update
    void Start()
    {
        gameUIManager = FindObjectOfType<GameUIManager>();
        saveLoadManager = FindObjectOfType<SaveLoadManager>();
        offsetX = gridHeight / 4;
        offsetZ = gridWidth / 4;
        GenerateGrid();

        if (loadOnStart) Invoke(nameof(Load), float.MinValue);
        
    }

    private void GenerateGrid()
    {

        for (int x = 0; x < gridHeight; x++)
        {
            for (int z = 0; z < gridWidth; z++)
            {
                PlaceTile(null, new Vector3Int(x, 0, z), 0);
            }
        }
    }
    
    public void LoadGrid(List<TileSaveData> tileDatas)
    {
        ClearGrid();
        
        foreach(var tileData in tileDatas)
        {

            PlaceTile(tileData.Description, new Vector3Int(tileData.PositionX, tileData.PositionY, tileData.PositionZ), tileData.moralityLevel);

        }
    }
    private void PlaceTile(string tileObjectName, Vector3Int gridPosition, float moralityLevel)
    {
        
        Tile tileToPlace = tiles[0];
        if (tileObjectName != null)
        {
            for (int i = 0; i < tiles.Length; i++)
            {
                if (tiles[i].name.Equals(tileObjectName))
                {
                    tileToPlace = tiles[i];
                }
            }
        }
        else
        {
            if (previousTilePlaced)
            {
                tileToPlace = UnityEngine.Random.Range(0.0f, 1.0f) <= chanceToSwitchTile ? tiles[UnityEngine.Random.Range(0, tiles.Length)] : previousTilePlaced;
            }
            else
            {
                tileToPlace = tiles[UnityEngine.Random.Range(0, tiles.Length)];
            }
        }
        previousTilePlaced = tileToPlace;

        Tile newTile = Instantiate(tileToPlace, transform);

        Morality newMorality = new Morality();

        newMorality.moralityLevel = moralityLevel;
        newTile.tileMorality = newMorality;

        
        newTile.gridPosition = gridPosition;
        Vector3 newPosition = GetGamePositionForGridPosition(gridPosition);
        newTile.transform.position = newPosition;

        tileMap[newTile.gridPosition] = newTile;
        newTile.name = $"GamePosition: {newPosition.x}, {newPosition.z}, GridPosition: {gridPosition.x}, {gridPosition.z} ";
    }

    private void ClearGrid()
    {
        
        foreach (var tile in tileMap)
        {
            Destroy(tile.Value.gameObject);
        }
        tileMap = new Dictionary<Vector3Int, Tile>();
    }

    public void ObjectSelectedAtPosition(Vector3Int selectionPosition)
    {
        if (selectionMode == SelectionMode.Single)
        {
            ClearlastSelectedTilePositions();
            lastSelectedObjectPositions.AddRange(GetPositionsOfObjectsInSelection(selectionPosition, selectionSize));
        }
        else if (selectionMode == SelectionMode.Rectangle || selectionMode == SelectionMode.Line)
        {
            if (isMouseButtonDown)
            {
                HandleSelectionWhenMouseIsDown(selectionPosition);
            }
            else
            {
                lastSelectedObjectPositions.Clear();
                lastSelectedObjectPositions.Add(selectionPosition);
            }
        }
        isSelectionValid = IsSelectionValid(lastSelectedObjectPositions);
        if (isSelectionValid)
        {
            SetSelectionOfObjectsAtPositions(lastSelectedObjectPositions, true);
            if (selectionInstance != null)
            {
                var gamePosition = GetSelectionCenter(lastSelectedObjectPositions);
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
        if (lastSelectedObjectPositions.Count == 0)
        {
            return;
        }
        var firstSelectionPosition = lastSelectedObjectPositions[0];
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
        lastSelectedObjectPositions = GetPositionsOfTilesInRectangleBetweenTwoPositions(firstSelectionPosition, secondSelectionPosition);
    }

    private bool IsSelectionValid(List<Vector3Int> selectedTilePositions)
    {
        bool isValid = true;
        if (selectedTilePositions.Count == 0)
        {
            return false;
        }
        var selectionMaxX = selectedTilePositions.Select(p => p.x).Max();
        var tileMapMaxX = tileMap.Keys.Select(p => p.x).Max();
        var tileMapMaxZ = tileMap.Keys.Select(p => p.z).Max();
        var selectionMaxZ = selectedTilePositions.Select(p => p.z).Max();
        if (selectionMaxX > tileMapMaxX || selectionMaxZ > tileMapMaxZ)
        {
            return false;
        }

        if (gameUIManager.selectedGameMode == GameMode.BuildMode)
        {

            var buildingPositions = buildingsMap.Keys.Select(p => new Vector2(p.x, p.z));
            var selectedTilePositionsVec2 = selectedTilePositions.Select(p => new Vector2(p.x, p.z));

            foreach (var selectedTilePosition in selectedTilePositionsVec2)
            {
                if (buildingPositions.Contains(selectedTilePosition))
                {
                    isValid = false;
                }
            }

        }
        return isValid;
    }

    public void ObjectDeselectedAtPosition(Vector3Int position)
    {
        if (!isMouseButtonDown)
        {
            ClearlastSelectedTilePositions();
        }
    }

    List<Vector3Int> GetPositionsOfObjectsInSelection(Vector3Int startObjectPosition, Vector2 selectionSize)
    {
        List<Vector3Int> positions = new List<Vector3Int>();
        for (int x = 0; x < selectionSize.x; x++)
        {
            for (int z = 0; z < selectionSize.y; z++)
            {
                positions.Add(new Vector3Int(startObjectPosition.x + x, startObjectPosition.y, startObjectPosition.z + z));
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
        SetSelectionOfObjectsAtPositions(lastSelectedObjectPositions, false);
        lastSelectedObjectPositions.Clear();
    }

    void SetSelectionOfObjectsAtPositions(List<Vector3Int> objectPositions, bool selected)
    {
        foreach (var objectPosition in objectPositions)
        {
            var selectedObject = GetSelectedObjectAtPosition(objectPosition);
            if (selectedObject != null && notOnMap)
            {
                selectedObject.ToggleHighlight(selected);
            }
        }
    }

    public Tile GetTileAtPosition(Vector3Int position)
    {
        return tileMap.GetValueOrDefault(position, null);
    }

    public SelectionManager GetSelectedObjectAtPosition(Vector3Int position)
    {
        var gameObject = tileMap.GetValueOrDefault(position, null)?.gameObject;
        if (gameObject == null)
        {
            gameObject = buildingsMap.GetValueOrDefault(position, null)?.GetBuildingPrefabForPosition(position);
        }
        return gameObject?.GetComponent<SelectionManager>();
    }

    public AbstractBuildingType GetBuildingAtPosition(Vector3Int position)
    {
        return buildingsMap.GetValueOrDefault(position, null);
    }

    public SelectableObject GetSelectableObjectAtPosition(Vector3Int position)
    {
        return buildingsMap.GetValueOrDefault(position, null)?.GetSelectionManagerForGridPosition(position) ?? propertyMap.GetValueOrDefault(position, null).SelectionManager;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isMouseButtonDown = true;

            if (lastSelectedObjectPositions.Count > 0 && selectionMode == SelectionMode.Single && isSelectionValid)
            {
                var selectedObject = GetSelectedObjectAtPosition(lastSelectedObjectPositions[0]);
                if (selectedObject == null) return;
                ManageObjectSelectionInSingleMode(selectedObject);
                isSelectionValid = false;
                //VisualizeNeighbours();
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isMouseButtonDown = false;
            if (selectionInstance != null)
            {
                Destroy(selectionInstance);
            }
            if ((selectionMode == SelectionMode.Rectangle || selectionMode == SelectionMode.Line) && lastSelectedObjectPositions.Count > 0 && isSelectionValid)
            {
                var selectedObject = GetSelectedObjectAtPosition(lastSelectedObjectPositions[0]);
                ManageObjectSelectionInRectangleAndLineMode(selectedObject);
                ClearlastSelectedTilePositions();
                isSelectionValid = false;
                //VisualizeNeighbours();
            }
        }
        else if (Input.GetKey(KeyCode.K) && cooldown < 0)
        {
            isMouseButtonDown = true;
            if (lastSelectedObjectPositions.Count > 0)
            {
                var selectedObject = GetSelectedObjectAtPosition(lastSelectedObjectPositions[0]);
                ManageObjectSelectionInSingleMode(selectedObject);
                Debug.Log("click at:" + selectedObject.GetGridPosition());
                var tile = selectedObject.GetGameObject().GetComponent<Tile>();
                if (tile != null)
                {
                    RecalcMorality(tile);
                }
            }
            else
            {
                gameUIManager.ObjectSelected(null, new Dictionary<Vector3, List<Vector3Int>>());
            }

            cooldown = 2f;
        }

        cooldown -= Time.deltaTime;
    }

    private void ManageObjectSelectionInSingleMode(SelectionManager selectedObject)
    {
        Dictionary<Vector3, List<Vector3Int>> placingPositionsWithGridPositions = new Dictionary<Vector3, List<Vector3Int>>();
        placingPositionsWithGridPositions[GetSelectionCenter(lastSelectedObjectPositions)] = lastSelectedObjectPositions;
        gameUIManager.ObjectSelected(selectedObject, placingPositionsWithGridPositions);
    }

    private void ManageObjectSelectionInRectangleAndLineMode(SelectionManager selectedObject)
    {

        Dictionary<Vector3, List<Vector3Int>> placingPositionsWithGridPositions = new Dictionary<Vector3, List<Vector3Int>>();
        lastSelectedObjectPositions.ForEach(lstp =>
        {
            var selectionCenter = GetSelectionCenter(new List<Vector3Int> { lstp });
            placingPositionsWithGridPositions[selectionCenter] = new List<Vector3Int> { lstp };
        });
        gameUIManager.ObjectSelected(selectedObject, placingPositionsWithGridPositions);
    }

    private void PlaceCarAtSelectedObject(SelectableObject selectedObject)
    {
        var car = Instantiate(carPrefab);
        var selectedObjectGridPosition = selectedObject.GetGridPosition();
        var carGridPosition = new Vector3Int(selectedObjectGridPosition.x, 1, selectedObjectGridPosition.z);
        carPrefab.GetComponent<CarNavigation>().CurrentGridPosition = carGridPosition;

        var carGamePosition = GetSelectionCenter(new List<Vector3Int> { carGridPosition });
        carGamePosition.y = GetGamePositionForGridPosition(carGridPosition).y;
        carGamePosition.z += tileSize / 4;
        car.transform.position = carGamePosition;
    }

    private void VisualizeNeighbours()
    {
        foreach (var arrow in arrows)
        {
            Destroy(arrow);
        }

        arrows.Clear();

        foreach (var cube in cubes)
        {
            Destroy(cube);
        }

        foreach (var positionAndBuilding in buildingsMap)
        {
            var cent = GetSelectionCenter(new List<Vector3Int> { positionAndBuilding.Key, positionAndBuilding.Key });
            foreach (var dir in positionAndBuilding.Value.neighbourDatasForPositions[positionAndBuilding.Key].NeighboursForGridPositions)
            {
                if (dir.Value == null)
                {
                    continue;
                }
                var arr = Instantiate(debugArrow);
                cent.y = tileSize;

                var rotation = 0.0f;
                var highlight = arr.GetComponent<Highlight>();
                var directionVector = dir.Key - positionAndBuilding.Key;

                var direction = Direction.North;

                if (directionVector.z > 0)
                {
                    direction = Direction.North;
                }
                else if (directionVector.z < 0)
                {
                    direction = Direction.South;
                }
                else if (directionVector.x > 0)
                {
                    direction = Direction.East;
                }
                else
                {
                    direction = Direction.West;
                }

                switch (direction)
                {
                    case Direction.North:
                        rotation = 90.0f;
                        highlight.SetHighlightColor(Color.blue);
                        break;
                    case Direction.South:
                        rotation = -90.0f;
                        highlight.SetHighlightColor(Color.red);
                        break;
                    case Direction.East:
                        rotation = 0.0f;
                        highlight.SetHighlightColor(Color.green);
                        break;
                    case Direction.West:
                        rotation = 180.0f;
                        highlight.SetHighlightColor(Color.yellow);
                        break;
                }
                highlight.ToggleHighlight(true);
                arr.transform.position = cent;
                arr.transform.Rotate(Vector3.forward, rotation);
                arrows.Add(arr);

            }

        }

        Debug.Log("Arrow count:" + arrows.Count);
        //Todo erase from here:
        foreach (var prop in propertyMap)
        {
            Debug.Log($"Property at ({(float)prop.Key.x / 2 - 5}, {(float)prop.Key.z / 2 - 5}): {prop.Value.HeadCount}/{prop.Value.Capacity}");
        }
        //Until This!!!!!!!!!!!
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
            case BuildingType.IndividualBuilding:
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
            //Debug.Log($"{gridPosition}, {buildingsMap.Keys}");
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

    public Vector3Int GetGridPositionForGamePosition(Vector3 gamePosition)
    {
        float x = gamePosition.x / tileSize + 2 * offsetX;
        float y = gamePosition.y / tileSize;
        float z = gamePosition.z / tileSize + 2 * offsetZ;
        Vector3Int gridPosition = new Vector3Int((int)Math.Ceiling(x), (int)Math.Ceiling(y), (int)Math.Ceiling(z));
        return gridPosition;
    }
    public Vector3 GetSelectionCenter(List<Vector3Int> selectedPositions)
    {
        var xMin = selectedPositions.Select(v => v.x).Min();
        var zMin = selectedPositions.Select(v => v.z).Min();

        var xMax = selectedPositions.Select(v => v.x).Max();
        var zMax = selectedPositions.Select(v => v.z).Max();

        var selectionStart = GetGamePositionForGridPosition(new Vector3Int(xMin - 1, 0, zMin));
        var selectionEnd = GetGamePositionForGridPosition(new Vector3Int(xMax, 0, zMax + 1));

        Vector3 selectionVector = (selectionEnd - selectionStart) / 2;
        Vector3 selectionCenter = selectionVector + selectionStart;
        return selectionCenter;
    }

    private void RecalcMorality(Tile selectedTile)
    {
        for (int i = 0; i < gridHeight; i++)
        {
            for (int j = 0; j < gridWidth; j++)
            {
                Vector3Int element = new Vector3Int(i, 0, j);
                int distance = (int)Vector3Int.Distance(tileMap[element].gridPosition, selectedTile.gridPosition);
                switch (distance)
                {
                    case 5:
                        tileMap[element].tileMorality.moralityLevel += 0.1f;
                        break;
                    case 4:
                        tileMap[element].tileMorality.moralityLevel += 0.2f;
                        break;
                    case 3:
                        tileMap[element].tileMorality.moralityLevel += 0.3f;
                        break;
                    case 2:
                        tileMap[element].tileMorality.moralityLevel += 0.4f;
                        break;
                    case 1:
                        tileMap[element].tileMorality.moralityLevel += 0.5f;
                        break;
                    case 0:
                        tileMap[element].tileMorality.moralityLevel += 0.75f;
                        break;
                    default: break;
                }
            }
        }
        if (!notOnMap)
        {
            ChangeMaterialsToMorality();
        }

    }

    public void ChangeMaterialsToMorality()
    {
        notOnMap = false;
        for (int i = 0; i < gridHeight; i++)
        {
            for (int j = 0; j < gridWidth; j++)
            {
                Vector3Int element = new Vector3Int(i, 0, j);
                tileMap[element].changeMaterial();
            }
        }
    }

    public void ResetMaterialsOnFields()
    {
        notOnMap = true;
        for (int i = 0; i < gridHeight; i++)
        {
            for (int j = 0; j < gridWidth; j++)
            {
                Vector3Int element = new Vector3Int(i, 0, j);
                tileMap[element].resetMaterial();
            }
        }
    }

    public void ResetSelection()
    {
        ClearlastSelectedTilePositions();
        ChangeSelection(new Vector2Int(1, 1), null, null);
    }

    public Vector3Int GetVectorToDirection(Vector3Int position, Direction direction)
    {
        switch (direction)
        {
            case Direction.North:
                return new Vector3Int(position.x, position.y, position.z + 1);
            case Direction.South:
                return new Vector3Int(position.x, position.y, position.z - 1);
            case Direction.East:
                return new Vector3Int(position.x + 1, position.y, position.z);
            case Direction.West:
                return new Vector3Int(position.x - 1, position.y, position.z);
            default:
                return position;
        }
    }
   
    public Dictionary<Vector3Int, AbstractBuildingType> GetNeigbouringBuildingsForPosition(Vector3Int position)
    {
        Dictionary<Vector3Int, AbstractBuildingType> neighbourDictionary = new Dictionary<Vector3Int, AbstractBuildingType>();
        foreach (var direction in Enum.GetValues(typeof(Direction)).Cast<Direction>())
        {
            var neighbourPositionToDirection = GetVectorToDirection(position, direction);
            neighbourDictionary[neighbourPositionToDirection] = GetBuildingAtPosition(neighbourPositionToDirection);
        }
        return neighbourDictionary;
    }
    public Dictionary<Vector3Int, AbstractBuildingType> GetBuildingsMap()
    {
        return buildingsMap;
    }

    public AbstarctProperty GetPropertyAt(Vector3Int position)
    {
        return propertyMap.GetValueOrDefault(position, null);
    }
    public void AddProperty(Vector3Int position, AbstarctProperty property)
    {
        if (propertyMap.GetValueOrDefault(position, null) != null)
        {
            throw new Exception("There is already a building at" + position);
        }
        else
        {
            propertyMap.Add(position, property);
            var buildingPosition = new Vector3Int(position.x, position.y - 1, position.z);
            if (buildingsMap.GetValueOrDefault(buildingPosition) is Zone)
            {
                Zone zone = buildingsMap.GetValueOrDefault(buildingPosition, null) as Zone;
                zone.AddProperty(buildingPosition, property);
            }
            //gameUIManager.AddEstate(position, property.PropertyType);
        }
    }

    public void Save()
    {
        saveLoadManager.SaveTiles(tileMap);
        saveLoadManager.SaveBuildings(buildingsMap);
        saveLoadManager.SaveProperties(propertyMap);
        saveLoadManager.SaveRest();
    }

    public void Load()
    {
        saveLoadManager.LoadTiles();
        saveLoadManager.LoadBuildings();
        saveLoadManager.LoadProperties();
        saveLoadManager.LoadRest();
    }

    public List<AbstarctProperty> GetProperties()
    {
        var ret= new List<AbstarctProperty>();
        foreach (var item in propertyMap)
        {
            ret.Add(item.Value);
        }
        return ret;
    }
}
