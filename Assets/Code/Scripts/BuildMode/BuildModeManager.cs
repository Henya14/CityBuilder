using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml.Schema;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UIElements;



public class BuildModeManager : MonoBehaviour
{
    private BuildingData selectedBuildingData;
    private NavigationManager navigationManager;
    private GameUIManager gameUIManager;

    public GameObject debugSpherePrefab;
    public GameObject twoWayStraight;
    public GameObject twoWayCurvy;
    public GameObject threeWay;
    public GameObject fourWay;
    private List<BuildingData> buildingDatas;

    void Start()
    {
        navigationManager = FindObjectOfType<NavigationManager>();
        gameUIManager = FindObjectOfType<GameUIManager>();
    }
    public void ObjectSelected(Dictionary<Vector3, List<Vector3Int>> placingPositionsWithGridPositions, SelectableObject selectedObject)
    {
        var gridPositions = new List<Vector3Int>();
        placingPositionsWithGridPositions.Values.ToList().ForEach(v => gridPositions.AddRange(v));
        gridPositions = gridPositions.Select(p => new Vector3Int(p.x, p.y + 1, p.z)).ToList();

        var selectedBuilding = CreateBuildingFromBuildingData(selectedBuildingData);
        if (selectedBuilding == null) return;
        List<Vector3> placePositions = new List<Vector3>();
        Dictionary<(Vector3, Quaternion), List<Vector3Int>> updatedPlacingPositionsWithGridPositions = new Dictionary<(Vector3, Quaternion), List<Vector3Int>>();
        foreach (var prefabPlacePosition in placingPositionsWithGridPositions.Keys)
        {
            var (gamePosition, rotation) = selectedObject.GetGridManager().GetGamePositionAndRotationForGridPosition(gridPositions[0]);
            var gamePositionY = gamePosition.y;
            var placePosition = new Vector3(prefabPlacePosition.x, gamePositionY, prefabPlacePosition.z);
            placePositions.Add(placePosition);
            updatedPlacingPositionsWithGridPositions[(placePosition, rotation)] = placingPositionsWithGridPositions[prefabPlacePosition].Select(p => new Vector3Int(p.x, p.y + 1, p.z)).ToList();
        }

        selectedObject.GetGridManager().AddBuildingToGrid(selectedBuilding, gridPositions);
        Dictionary<Vector3Int, NeighbourData> neigboursForBuildingPositions = GetNeighboursForGridPositions(gridPositions, selectedObject.GetGridManager());
        selectedBuilding.twoWayStraight = twoWayStraight;
        selectedBuilding.twoWayCurvy = twoWayCurvy;
        selectedBuilding.threeWay = threeWay;
        selectedBuilding.fourWay = fourWay;
        selectedBuilding.PlaceAtPosition(updatedPlacingPositionsWithGridPositions, neigboursForBuildingPositions, selectedObject.GetGridManager());
        if (selectedBuilding is Building || selectedBuilding is Road)
        {
            AddBuildingToNavigationManager(selectedBuilding, neigboursForBuildingPositions);
        }

    }
    // public void LoadBuildings(List<BuildingSaveData> buildings)
    // {
    //     foreach (var BSD in buildings)
    //     {
    //         BuildingDataSelected(BSD.GetBuildingData());
    //         ObjectSelected(BSD.GetDictionary(gridManager, default));
    //     }
    // }



    private void AddBuildingToNavigationManager(AbstractBuildingType selectedBuilding, Dictionary<Vector3Int, NeighbourData> neigboursForBuildingPositions)
    {
        var weights = new Dictionary<SelectableObject, NeighbourWeights>();
        foreach (var neighboursForPosition in neigboursForBuildingPositions)
        {
            if (selectedBuilding is Road)
            {
                weights = new Dictionary<SelectableObject, NeighbourWeights>();
            }
            weights = GetWeightsToNeighbour(selectedBuilding, neighboursForPosition, weights);
            if (selectedBuilding is Road)
            {
                navigationManager.AddBuilding(selectedBuilding.GetSelectionManagerForGridPosition(new Vector3Int()), weights);
            }
        }

        if (selectedBuilding is Building)
        {
            navigationManager.AddBuilding(selectedBuilding.GetSelectionManagerForGridPosition(selectedBuilding.gridPositions[0]), weights);
        }
    }

    private Dictionary<SelectableObject, NeighbourWeights> GetWeightsToNeighbour(AbstractBuildingType selectedBuilding, KeyValuePair<Vector3Int, NeighbourData> neighboursForPosition, Dictionary<SelectableObject, NeighbourWeights> weights)
    {
        foreach (var neigbour in neighboursForPosition.Value.NeighboursForGridPositions)
        {
            if (selectedBuilding is not Road && (neigbour.Value == selectedBuilding || neigbour.Value == null))
            {
                continue;
            }
            if (neigbour.Value != null)
            {
                var neighbourWeights = new NeighbourWeights
                {
                    WeightFromNeighbour = 1,
                    WeightToNeighbour = 1,
                };

                var neighbourSelectableObject = neigbour.Value.GetSelectionManagerForGridPosition(neigbour.Key);
                if (neigbour.Value is Road) {

                    neighbourSelectableObject = neigbour.Value.GetSelectionManagerForGridPosition(new Vector3Int());
                }

                if (neighbourSelectableObject.GetSelectableObjectType() == SelectableObjectType.Road || selectedBuilding is Road)
                {
                    weights.TryAdd(neighbourSelectableObject, neighbourWeights);
                }
            }
        }

        return weights;
    }

    private Dictionary<Vector3Int, NeighbourData> GetNeighboursForGridPositions(List<Vector3Int> gridPositions, GridManager gridManager)
    {
        var neigbours = new Dictionary<Vector3Int, NeighbourData>();
        foreach (var gridPosition in gridPositions)
        {
            var neighbourDictionary = gridManager.GetNeigbouringBuildingsForPosition(gridPosition);
            var neighbourData = new NeighbourData(neighbourDictionary);
            neigbours[gridPosition] = neighbourData;
        }

        return neigbours;
    }

    public List<BuildingData> LoadBuildingDatasAndReturnAvailable()
    {
        buildingDatas = Resources.LoadAll<BuildingData>("Buildings").ToList();
        buildingDatas = buildingDatas.Select(bd =>
        {
            bd.isAvailable = UnityEngine.Random.Range(0, 2) == 1 ? true : false;
            return bd;
        }).ToList();
        return buildingDatas.Where(bd => !bd.BuyableBuilding).ToList();
    }

    public void BuildingDataSelected(BuildingData buildingData)
    {
        selectedBuildingData = buildingData;
    }

    public AbstractBuildingType CreateBuildingFromBuildingData(BuildingData data, GameObject gameObject = default, string name = default)
    {
        if (data == null) return null;
        if (gameObject == default)
        {
            gameObject = Instantiate(data.prefab);
        }
        if (name != default) {
            gameObject.name = name;
        }
        switch (data.buildingType)
        {
            case BuildingType.Road:
                var createdRoad = gameObject.AddComponent<Road>();
                createdRoad.Init(data);
                return createdRoad;
            case BuildingType.IndividualBuilding:
                var createdBuilding = gameObject.AddComponent<Building>();
                createdBuilding.Init(data);
                return createdBuilding;
            case BuildingType.Zone:
                var createdZone = gameObject.AddComponent<Zone>();
                createdZone.Init(data);
                return createdZone;
            default:
                throw new Exception("Unknown Building Type");
        }

    }

    public void RoadCreated(RoadData roadData)
    {
        var tilesToRoadPoints = CreateRoadNavigationPoints(roadData);
        int gridmgCount = 0;
        var emptyTileBuildingData = buildingDatas.First(bd => bd.BuildingName == "Empty Tile");
        foreach (var batch in roadData.batchesOnLeft)
        {
            foreach (var batchData in batch)
            {
                gridmgCount++;
                var gridManager = gameObject.AddComponent<GridManager>();
                gameUIManager.AddGridManager(gridManager);
                gridManager.number = gridmgCount;

                var x = 0;
                foreach (var tiles in batchData.emptyTileDatas)
                {
                   
                   
                    var y = 0;
                    gridManager.AddBuildingToGrid(tilesToRoadPoints[tiles[0].closestRoadPointData][0].GetGameObject().GetComponent<Road>(), new List<Vector3Int> () {
                        new Vector3Int(x, 1 , -1)
                    });
                    foreach (var tile in tiles)
                    {
                        var gridPos = new Vector3Int(x, 0, y);
                        var obj = CreateBuildingFromBuildingData(emptyTileBuildingData, tile.gameObject, $"Tile x: {x} y: {y}");
                        
                        obj.PlaceAtPosition(new Dictionary<(Vector3, Quaternion), List<Vector3Int>>()
                        {
                            { (tile.position, tile.rotation), new List<Vector3Int> {gridPos} },
                        }, new Dictionary<Vector3Int, NeighbourData>(), gridManager, obj.gameObject);
                        gridManager.AddTile(gridPos, obj.gameObject);
                        
                        y++;
                    }
                    x++;
                }
            }
        }
        foreach (var batch in roadData.batchesOnRight)
        {
            foreach (var batchData in batch)
            {
                foreach (var tiles in batchData.emptyTileDatas)
                {
                    foreach (var tile in tiles)
                    {
                        var obj = CreateBuildingFromBuildingData(emptyTileBuildingData, tile.gameObject);
                        obj.PlaceAtPosition(new Dictionary<(Vector3, Quaternion), List<Vector3Int>>()
                        {
                            { (tile.position, tile.rotation), new List<Vector3Int> { new Vector3Int() } },
                        }, new Dictionary<Vector3Int, NeighbourData>(), default, obj.gameObject);
                    }
                }
            }
        }
       

        foreach (var roadPoint in roadData.roadPoints)
        {
            int pointsToCreate = (int)roadPoint.roadWidth + 1;
            bool shouldUseMiddlePointOfRoad = pointsToCreate % 2 == 1;


            Debug.Log(pointsToCreate);
        }
    }

    private Dictionary<RoadPointData, List<SelectableObject>> CreateRoadNavigationPoints(RoadData roadData)
    {   
        Dictionary<RoadPointData, List<SelectableObject>> tilesToRoadPoints = new Dictionary<RoadPointData, List<SelectableObject>>();
        List<List<SelectableObject>> roadPoints = new List<List<SelectableObject>>(roadData.roadPoints.Count);
        for (int i = 0; i < roadData.roadPoints.Count; i++)
        {
            var roadPoint = roadData.roadPoints[i];
            int pointsToCreate = (int)roadPoint.roadWidth + 1;
            bool shouldUseMiddlePointOfRoad = pointsToCreate % 2 == 1;
            int pointsToCreateOnLeft = (int)Math.Floor((double)pointsToCreate / 2);
            int pointsToCreateOnRight = pointsToCreateOnLeft;

            if (roadPoints.ElementAtOrDefault(i) == default)
            {
                roadPoints.Add(new List<SelectableObject>());
            }
            for (int j = 0; j < pointsToCreateOnLeft; j++)
            {


                var vectorFromLeftToMiddleOfRoad = roadPoint.middleRoadPoint - roadPoint.leftRoadPoint;
                var position = roadPoint.leftRoadPoint + vectorFromLeftToMiddleOfRoad / (pointsToCreateOnLeft + 1.0f) * (j + 1);
                var (point, selectionManager) = CreatePointAtPositionWithColor(position, Color.red);
                var weights = new Dictionary<SelectableObject, NeighbourWeights>();
                if (i > 0)
                {
                    weights.Add(roadPoints[i - 1][j], new NeighbourWeights
                    {
                        WeightFromNeighbour = 1,
                        WeightToNeighbour = GraphConnection<SelectableObject>.NO_CONNECTION_WEIGHT
                    });
                }
                navigationManager.AddBuilding(selectionManager, weights);
                roadPoints[i].Add(selectionManager);

            }
            if (shouldUseMiddlePointOfRoad)
            {
                var position = roadPoint.middleRoadPoint;
                var (point, selectionManager) = CreatePointAtPositionWithColor(position, Color.white);
                var weights = new Dictionary<SelectableObject, NeighbourWeights>();
                if (i > 0)
                {
                    weights.Add(roadPoints[i - 1][pointsToCreateOnLeft], new NeighbourWeights
                    {
                        WeightFromNeighbour = GraphConnection<SelectableObject>.NO_CONNECTION_WEIGHT,
                        WeightToNeighbour = 1
                    });
                    if (i == roadData.roadPoints.Count - 1)
                    {
                        weights.Add(roadPoints[i][pointsToCreateOnLeft - 1], new NeighbourWeights
                        {
                            WeightFromNeighbour = 1,
                            WeightToNeighbour = GraphConnection<SelectableObject>.NO_CONNECTION_WEIGHT,
                        });
                    }
                }
                else if (i == 0)
                {
                    weights.Add(roadPoints[0][pointsToCreateOnLeft - 1], new NeighbourWeights
                    {
                        WeightFromNeighbour = GraphConnection<SelectableObject>.NO_CONNECTION_WEIGHT,
                        WeightToNeighbour = 1
                    });
                }

                navigationManager.AddBuilding(selectionManager, weights);
                roadPoints[i].Add(selectionManager);
            }
            for (int j = 0; j < pointsToCreateOnRight; j++)
            {
                var vectorFromMiddleToRightOfRoad = roadPoint.rightRoadPoint - roadPoint.middleRoadPoint;
                var position = roadPoint.middleRoadPoint + vectorFromMiddleToRightOfRoad / (pointsToCreateOnRight + 1.0f) * (j + 1);
                var (point, selectionManager) = CreatePointAtPositionWithColor(position, Color.yellow);
                var weights = new Dictionary<SelectableObject, NeighbourWeights>();
                if (i > 0)
                {
                    weights.Add(roadPoints[i - 1][pointsToCreateOnLeft + j + 1], new NeighbourWeights
                    {
                        WeightFromNeighbour = GraphConnection<SelectableObject>.NO_CONNECTION_WEIGHT,
                        WeightToNeighbour = 1
                    });

                    if (i == roadData.roadPoints.Count - 1)
                    {
                        weights.Add(roadPoints[i][pointsToCreateOnLeft - 1 - j], new NeighbourWeights
                        {
                            WeightFromNeighbour = 1,
                            WeightToNeighbour = GraphConnection<SelectableObject>.NO_CONNECTION_WEIGHT,
                        });
                    }
                }
                else if (i == 0)
                {
                    weights.Add(roadPoints[0][pointsToCreateOnLeft - 1 - j], new NeighbourWeights
                    {
                        WeightFromNeighbour = GraphConnection<SelectableObject>.NO_CONNECTION_WEIGHT,
                        WeightToNeighbour = 1
                    });
                }

                navigationManager.AddBuilding(selectionManager, weights);
                roadPoints[i].Add(selectionManager);
            }

            tilesToRoadPoints.Add(roadPoint, roadPoints[i]);
        }
        return tilesToRoadPoints;
    }

    private (AbstractBuildingType, SelectableObject) CreatePointAtPositionWithColor(Vector3 position, Color color)
    {

        var point = CreateBuildingFromBuildingData(selectedBuildingData);
        var gridPosition = new Vector3Int();
        point.PlaceAtPosition(new Dictionary<(Vector3, Quaternion), List<Vector3Int>>() {
            {(position, Quaternion.identity), new List<Vector3Int>{gridPosition}},
        }, new Dictionary<Vector3Int, NeighbourData>(), default, point.gameObject);


        return (point, point.GetSelectionManagerForGridPosition(gridPosition));
    }

    /*Not used currently
public void ConstructEstate(Vector3Int position,PropertyType pType)
{
   var dc = selectPropertyObject(pType);
   //TODO: GameObject placement
   Vector3Int key = position;
   var manager= FindObjectOfType<GridManager>();
   dc.name = $"TEST Construction {(float)key.x / 2 - 5}, {(float)key.z / 2 - 5}";
   dc.transform.parent = manager.transform;
   dc.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

   dc.transform.localPosition = new Vector3((float)key.x / 2 - 5 - 0.25f, 0.75f, (float)key.z / 2 - 5 + 0.25f);

}

private GameObject selectPropertyObject(PropertyType pType)
{
   //Random object select according to it's function
   //Dummy:
   return GameObject.CreatePrimitive(PrimitiveType.Cube);

}
*/
}
