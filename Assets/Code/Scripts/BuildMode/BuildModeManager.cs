using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml.Schema;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.AI;
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
    [SerializeField] Material moralityMaterial;

    void Start()
    {
        navigationManager = FindObjectOfType<NavigationManager>();
        gameUIManager = FindObjectOfType<GameUIManager>();
    }
    public void ObjectSelected(Dictionary<Vector3, List<Vector3Int>> placingPositionsWithGridPositions, SelectableObject selectedObject)
    {
        if (placingPositionsWithGridPositions == null)
        {
            return;
        }
        var gridPositions = new List<Vector3Int>();
        placingPositionsWithGridPositions.Values.ToList().ForEach(v => gridPositions.AddRange(v));
        gridPositions = gridPositions.Select(p => new Vector3Int(p.x, p.y + 1, p.z)).ToList();
        var (selectedBuilding, go) = CreateBuildingFromBuildingData(selectedBuildingData);
        if (selectedBuilding == null || selectedBuilding is Road) return;
        List<Vector3> placePositions = new List<Vector3>();
        Dictionary<(Vector3, Quaternion), List<Vector3Int>> updatedPlacingPositionsWithGridPositions = new Dictionary<(Vector3, Quaternion), List<Vector3Int>>();
        foreach (var prefabPlacePosition in placingPositionsWithGridPositions.Keys)
        {
            var placingGridBasePosition = placingPositionsWithGridPositions[prefabPlacePosition][0];
            var placingGridPosition = new Vector3Int(placingGridBasePosition.x, placingGridBasePosition.y + 1, placingGridBasePosition.z);
            var (gamePosition, rotation) = selectedObject.GetGridManager().GetGamePositionAndRotationForGridPosition(placingGridPosition);
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
        selectedBuilding.PlaceAtPosition(updatedPlacingPositionsWithGridPositions, neigboursForBuildingPositions, selectedObject.GetGridManager(), go);
        if (selectedBuilding is Building)
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
            weights = GetWeightsToNeighbour(selectedBuilding, neighboursForPosition, weights);
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
                if (neigbour.Value is Road)
                {

                    neighbourSelectableObject = neigbour.Value.GetSelectionManagerForGridPosition(new Vector3Int());
                }
                // not all buildings have a selectable object type
                if (neighbourSelectableObject.GetSelectableObjectType() != null && neighbourSelectableObject.GetSelectableObjectType() == SelectableObjectType.Road || selectedBuilding is Road)
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

    public (AbstractBuildingType, GameObject) CreateBuildingFromBuildingData(BuildingData data, GameObject gameObject = default, string name = default, Transform parent = default)
    {
        if (data == null) return (default, default);
        if (gameObject == default)
        {
            gameObject = Instantiate(data.prefab, parent);
        }
        if (name != default)
        {
            gameObject.name = name;
        }
        switch (data.buildingType)
        {
            case BuildingType.Road:
                var createdRoad = gameObject.AddComponent<Road>();
                createdRoad.Init(data);
                return (createdRoad, gameObject);
            case BuildingType.IndividualBuilding:
                var createdBuilding = gameObject.AddComponent<Building>();
                createdBuilding.Init(data);
                return (createdBuilding, gameObject);
            case BuildingType.Zone:
                var createdZone = gameObject.AddComponent<Zone>();
                createdZone.Init(data);
                return (createdZone, gameObject);
            default:
                throw new Exception("Unknown Building Type");
        }

    }

    public void RoadCreated(RoadData roadData)
    {
        var (tilesToRoadPoints, graphNodesToRoadPoints) = CreateRoadNavigationPoints(roadData);


        int gridmgCount = 0;
        gridmgCount = SetUpEmptyTilesForBatches(roadData.batchesOnLeft, tilesToRoadPoints, gridmgCount, roadData.roadMesh);
        SetUpEmptyTilesForBatches(roadData.batchesOnRight, tilesToRoadPoints, gridmgCount, roadData.roadMesh);
        //remove empty tiles gameobjects in children
        var roadChildren = roadData.roadMesh.GetComponentsInChildren<Transform>().ToList();
        foreach (var rc in roadChildren)
        {
            if (rc.gameObject.name.Contains("Clone"))
            {
                Destroy(rc.gameObject);
            }
        }
    }
    List<Rect> resourceRectagles;
    private int SetUpEmptyTilesForBatches(List<List<BatchData>> batchDatasList, Dictionary<RoadPointData, List<SelectableObject>> tilesToRoadPoints, int gridmgCount, GameObject roadGameObject)
    {
        var emptyTileBuildingData = buildingDatas.First(bd => bd.BuildingName == "Empty Tile");
        foreach (var batchDatas in batchDatasList)
        {
            foreach (var batchData in batchDatas)
            {
                gridmgCount++;
                var gridManager = roadGameObject.AddComponent<GridManager>();

                gameUIManager.AddGridManager(gridManager);
                gridManager.number = gridmgCount;

                var x = 0;
                foreach (var tiles in batchData.emptyTileDatas)
                {


                    var y = 0;
                    var tilesClosestRoadPointData = tiles[0].closestRoadPointData;
                    var roadPoints = tilesToRoadPoints[tilesClosestRoadPointData];
                    var closestRoadPoint = roadPoints.OrderBy(rp => (rp.GetGameObject().transform.position - tiles[0].position).sqrMagnitude).First();

                    var roadToAdd = closestRoadPoint.GetGameObject().GetComponent<Road>();
                    gridManager.AddBuildingToGrid(roadToAdd, new List<Vector3Int>() {
                        new Vector3Int(x, 1 , -1)
                    });
                    foreach (var tile in tiles)
                    {
                        var gridPos = new Vector3Int(x, 0, y);
                        var (obj, go) = CreateBuildingFromBuildingData(emptyTileBuildingData, tile.gameObject, $"Tile x: {x} y: {y}");
                        obj.PlaceAtPosition(new Dictionary<(Vector3, Quaternion), List<Vector3Int>>()
                        {
                            { (tile.position, tile.rotation), new List<Vector3Int> {gridPos} },
                        }, new Dictionary<Vector3Int, NeighbourData>(), gridManager, obj.gameObject);
                        var tileScript = obj.AddComponent<Tile>();
                        tileScript.SetMoralityMaterial(moralityMaterial);

                        Morality newMorality = new Morality();
                        newMorality.moralityLevel = 1.0f;
                        tileScript.tileMorality = newMorality;
                        gridManager.AddTile(gridPos, tileScript);
                        y++;
                    }
                    x++;
                }
            }
        }


        return gridmgCount;
    }

    private (Dictionary<RoadPointData, List<SelectableObject>> tilesToRoadPoints, RoadData roadData) CreateRoadNavigationPoints(RoadData roadData)
    {
        Dictionary<RoadPointData, List<SelectableObject>> tilesToRoadPoints = new Dictionary<RoadPointData, List<SelectableObject>>();
        Dictionary<RoadPointData, List<GraphNodeForRoadPoint>> graphNodesToRoadPoints = new Dictionary<RoadPointData, List<GraphNodeForRoadPoint>>();
        List<List<SelectableObject>> roadPointSelectableObjects = new List<List<SelectableObject>>(roadData.roadPoints.Count);
        for (int i = 0; i < roadData.roadPoints.Count; i++)
        {
            RoadPointData roadPoint = roadData.roadPoints[i];
            int pointsToCreate = (int)roadPoint.roadWidth + 1;
            bool shouldUseMiddlePointOfRoad = pointsToCreate % 2 == 1;
            int pointsToCreateOnLeft = (int)Math.Floor((double)pointsToCreate / 2);
            int pointsToCreateOnRight = pointsToCreateOnLeft;

            // var point2 = Instantiate(debugSpherePrefab);
            // point2.transform.position = roadPoint.leftRoadPoint;

            // var selectionManager2 = point2.AddComponent<SelectionManager>();
            // selectionManager2.SetHighlightColor(Color.red);
            // selectionManager2.ToggleHighlight(true);

            // point2 = Instantiate(debugSpherePrefab);
            // point2.transform.position = roadPoint.middleRoadPoint;

            // selectionManager2 = point2.AddComponent<SelectionManager>();
            // selectionManager2.SetHighlightColor(Color.white);
            // selectionManager2.ToggleHighlight(true);

            // point2 = Instantiate(debugSpherePrefab);
            // point2.transform.position = roadPoint.rightRoadPoint;

            // selectionManager2 = point2.AddComponent<SelectionManager>();
            // selectionManager2.SetHighlightColor(Color.yellow);
            // selectionManager2.ToggleHighlight(true);

            if (roadPointSelectableObjects.ElementAtOrDefault(i) == default)
            {
                roadPointSelectableObjects.Add(new List<SelectableObject>());
            }
            if (graphNodesToRoadPoints.GetValueOrDefault(roadPoint, default) == default)
            {
                graphNodesToRoadPoints[roadPoint] = new List<GraphNodeForRoadPoint>();
            }
            for (int j = 0; j < pointsToCreateOnLeft; j++)
            {


                var vectorFromLeftToMiddleOfRoad = roadPoint.middleRoadPoint - roadPoint.leftRoadPoint;
                var position = roadPoint.leftRoadPoint + vectorFromLeftToMiddleOfRoad / (pointsToCreateOnLeft + 1.0f) * (j + 1);
                var (point, selectionManager) = CreatePointAtPositionWithColor(position, Color.white, roadData.roadMesh, roadPointSelectableObjects[i].Count);
                var weights = new Dictionary<SelectableObject, NeighbourWeights>();
                if (i > 0)
                {
                    weights.Add(roadPointSelectableObjects[i - 1][j], new NeighbourWeights
                    {
                        WeightFromNeighbour = 1,
                        WeightToNeighbour = GraphConnection<SelectableObject>.NO_CONNECTION_WEIGHT
                    });
                }
                navigationManager.AddBuilding(selectionManager, weights);
                graphNodesToRoadPoints[roadPoint].Add(new GraphNodeForRoadPoint
                {
                    graphNode = navigationManager.GetGraphNodeForSelectableObject(selectionManager),
                    pointSide = PointSide.Left
                });
                roadPointSelectableObjects[i].Add(selectionManager);

            }
            if (shouldUseMiddlePointOfRoad)
            {
                var position = roadPoint.middleRoadPoint;
                var (point, selectionManager) = CreatePointAtPositionWithColor(position, Color.white, roadData.roadMesh, roadPointSelectableObjects[i].Count);
                var weights = new Dictionary<SelectableObject, NeighbourWeights>();
                if (i > 0)
                {
                    weights.Add(roadPointSelectableObjects[i - 1][pointsToCreateOnLeft], new NeighbourWeights
                    {
                        WeightFromNeighbour = GraphConnection<SelectableObject>.NO_CONNECTION_WEIGHT,
                        WeightToNeighbour = 1
                    });
                    if (i == roadData.roadPoints.Count - 1 && roadData.roadEndConnectionToOtherRoad == default)
                    {
                        weights.Add(roadPointSelectableObjects[i][pointsToCreateOnLeft - 1], new NeighbourWeights
                        {
                            WeightFromNeighbour = 1,
                            WeightToNeighbour = GraphConnection<SelectableObject>.NO_CONNECTION_WEIGHT,
                        });
                    }
                }
                else if (i == 0 && roadData.roadStartConnectionToOtherRoad == default)
                {
                    weights.Add(roadPointSelectableObjects[0][pointsToCreateOnLeft - 1], new NeighbourWeights
                    {
                        WeightFromNeighbour = GraphConnection<SelectableObject>.NO_CONNECTION_WEIGHT,
                        WeightToNeighbour = 1
                    });
                }

                navigationManager.AddBuilding(selectionManager, weights);
                graphNodesToRoadPoints[roadPoint].Add(new GraphNodeForRoadPoint
                {
                    graphNode = navigationManager.GetGraphNodeForSelectableObject(selectionManager),
                    pointSide = PointSide.Right
                });
                roadPointSelectableObjects[i].Add(selectionManager);
            }
            for (int j = 0; j < pointsToCreateOnRight; j++)
            {
                var vectorFromMiddleToRightOfRoad = roadPoint.rightRoadPoint - roadPoint.middleRoadPoint;
                var position = roadPoint.middleRoadPoint + vectorFromMiddleToRightOfRoad / (pointsToCreateOnRight + 1.0f) * (j + 1);
                var (point, selectionManager) = CreatePointAtPositionWithColor(position, Color.white, roadData.roadMesh, roadPointSelectableObjects[i].Count);
                var weights = new Dictionary<SelectableObject, NeighbourWeights>();
                if (i > 0)
                {
                    var idx = pointsToCreateOnLeft + j;
                    if (shouldUseMiddlePointOfRoad)
                    {
                        idx++;
                    }
                    weights.Add(roadPointSelectableObjects[i - 1][idx], new NeighbourWeights
                    {
                        WeightFromNeighbour = GraphConnection<SelectableObject>.NO_CONNECTION_WEIGHT,
                        WeightToNeighbour = 1
                    });

                    if (i == roadData.roadPoints.Count - 1 && roadData.roadEndConnectionToOtherRoad == default)
                    {
                        weights.Add(roadPointSelectableObjects[i][pointsToCreateOnLeft - 1 - j], new NeighbourWeights
                        {
                            WeightFromNeighbour = 1,
                            WeightToNeighbour = GraphConnection<SelectableObject>.NO_CONNECTION_WEIGHT,
                        });
                    }
                }
                else if (i == 0 && roadData.roadStartConnectionToOtherRoad == default)
                {
                    weights.Add(roadPointSelectableObjects[0][pointsToCreateOnLeft - 1 - j], new NeighbourWeights
                    {
                        WeightFromNeighbour = GraphConnection<SelectableObject>.NO_CONNECTION_WEIGHT,
                        WeightToNeighbour = 1
                    });
                }

                navigationManager.AddBuilding(selectionManager, weights);
                graphNodesToRoadPoints[roadPoint].Add(new GraphNodeForRoadPoint
                {
                    graphNode = navigationManager.GetGraphNodeForSelectableObject(selectionManager),
                    pointSide = PointSide.Right
                });
                roadPointSelectableObjects[i].Add(selectionManager);
            }

            tilesToRoadPoints.Add(roadPoint, roadPointSelectableObjects[i]);
        }
        
        var graphNodesToRoadPointsKeys = graphNodesToRoadPoints.Keys.ToList();
        // add adjacency connections for points on the same road
            for (int i = 1; i < graphNodesToRoadPointsKeys.Count; i++)
            {
                var currentGraphNodes = graphNodesToRoadPoints[graphNodesToRoadPointsKeys[i]];
                var previousGraphNodes = graphNodesToRoadPoints[graphNodesToRoadPointsKeys[i - 1]];
                // add on the left the connections
                for (int j = 0; j < previousGraphNodes.Count; j++)
                {
                    var previousRoadPoint = previousGraphNodes[j];
                    GraphNodeForRoadPoint? leftNextRoadPoint = j > 0 ? currentGraphNodes[j - 1] : null;
                    GraphNodeForRoadPoint? rightNextRoadPoint = j < currentGraphNodes.Count - 1 ? currentGraphNodes[j + 1] : null;
                    if (previousRoadPoint.pointSide == PointSide.Left)
                    {
                        if (leftNextRoadPoint != null && leftNextRoadPoint.Value.pointSide == PointSide.Left)
                        {
                            previousRoadPoint.graphNode.AddConnection(leftNextRoadPoint.Value.graphNode, GraphConnection<SelectableObject>.NO_CONNECTION_WEIGHT);
                            leftNextRoadPoint.Value.graphNode.AddConnection(previousRoadPoint.graphNode, 1);
                        }
                        if (rightNextRoadPoint != null && rightNextRoadPoint.Value.pointSide == PointSide.Left)
                        {
                            previousRoadPoint.graphNode.AddConnection(rightNextRoadPoint.Value.graphNode, GraphConnection<SelectableObject>.NO_CONNECTION_WEIGHT);
                            rightNextRoadPoint.Value.graphNode.AddConnection(previousRoadPoint.graphNode, 1);
                        }
                    }
                    else if (previousRoadPoint.pointSide == PointSide.Right)
                    {
                        if (leftNextRoadPoint != null && leftNextRoadPoint.Value.pointSide == PointSide.Right)
                        {
                            previousRoadPoint.graphNode.AddConnection(leftNextRoadPoint.Value.graphNode, 1);
                            leftNextRoadPoint.Value.graphNode.AddConnection(previousRoadPoint.graphNode, GraphConnection<SelectableObject>.NO_CONNECTION_WEIGHT);
                        }
                        if (rightNextRoadPoint != null && rightNextRoadPoint.Value.pointSide == PointSide.Right)
                        {
                            previousRoadPoint.graphNode.AddConnection(rightNextRoadPoint.Value.graphNode, 1);
                            rightNextRoadPoint.Value.graphNode.AddConnection(previousRoadPoint.graphNode, GraphConnection<SelectableObject>.NO_CONNECTION_WEIGHT);
                        }
                    }

                }

                
            }
        roadData.graphNodesForRoadPoints = graphNodesToRoadPoints;
        gameUIManager.SetRoadDataForRoad(roadData.roadName, roadData);

        if (roadData.roadEndConnectionToOtherRoad != default)
        {
            var roadDataForConnectingRoad = gameUIManager.GetRoadDataForRoad(roadData.roadEndConnectionToOtherRoad.connectingRoadName);
            CreateIntersectionConnectionForConnetingRoadAndRoadExtension(roadData.roadEndConnectionToOtherRoad.connectionPoint, roadData, roadDataForConnectingRoad, graphNodesToRoadPoints, true);
        }
        if (roadData.roadStartConnectionToOtherRoad != default)
        {
            var roadDataForConnectingRoad = gameUIManager.GetRoadDataForRoad(roadData.roadStartConnectionToOtherRoad.connectingRoadName);
            CreateIntersectionConnectionForConnetingRoadAndRoadExtension(roadData.roadStartConnectionToOtherRoad.connectionPoint, roadData, roadDataForConnectingRoad, graphNodesToRoadPoints, false);
        }

        return (tilesToRoadPoints, roadData);
    }

    /// <summary>
    /// Creates navigation graph connections between a newly built road and an existing road it connects to.
    /// Handles two cases:
    ///   1. The connection point is at the START or END of the existing road (endpoint-to-endpoint merge).
    ///   2. The connection point is in the MIDDLE of the existing road (T-intersection), delegated to CreateIntersectionConnectionsForConnectingRoad.
    /// </summary>
    /// <param name="closestRoadPointData">The RoadPointData on the existing road closest to the connection.</param>
    /// <param name="roadData">The newly created road's data.</param>
    /// <param name="roadDataForConnectingRoad">The existing road's data that we are connecting to.</param>
    /// <param name="graphNodesToRoadPoints">Mapping from RoadPointData to graph nodes for the new road.</param>
    /// <param name="isLast">True if connecting at the END of the new road; false if at the START.</param>
    private void CreateIntersectionConnectionForConnetingRoadAndRoadExtension(RoadPointData closestRoadPointData, RoadData roadData, RoadData roadDataForConnectingRoad, Dictionary<RoadPointData, List<GraphNodeForRoadPoint>> graphNodesToRoadPoints, bool isLast)
    {
        // Pick the endpoint of the new road that faces the connection (last point if isLast, first point otherwise)
        var roadPointData = isLast ? roadData.roadPoints[roadData.roadPoints.Count - 1] : roadData.roadPoints[0];
        var middlePointToCompare = roadPointData.middleRoadPoint;

        // Determine which side (left or right) of the existing road's cross-section is closer to the new road's endpoint.
        // This is used later to orient the graph connections correctly (traffic direction).
        var distanceToLeftPoint = (middlePointToCompare - closestRoadPointData.leftRoadPoint).magnitude;
        var distanceToRightPoint = (middlePointToCompare - closestRoadPointData.rightRoadPoint).magnitude;
        bool isLeftCloserToPoint = distanceToLeftPoint < distanceToRightPoint;

        // Find where the connection point sits along the existing road's list of road points
        var indexOfClosestRoadData = roadDataForConnectingRoad.roadPoints.IndexOf(closestRoadPointData);

        // CASE 1: Connection point is at the very start or end of the existing road (endpoint-to-endpoint merge).
        // In this case we do a simple 1-to-1 bidirectional link between each pair of graph nodes.
        if (indexOfClosestRoadData == 0 || indexOfClosestRoadData == roadDataForConnectingRoad.roadPoints.Count - 1)
        {
            // Figure out which end of the NEW road is closer to the existing road's endpoint
            var startRoadPointDataMiddlePoint = roadData.roadPoints[0].middleRoadPoint;
            var endRoadPointDataMiddlePoint = roadData.roadPoints[roadData.roadPoints.Count - 1].middleRoadPoint;
            bool isStartCloserToClosestRoadData = (closestRoadPointData.middleRoadPoint - startRoadPointDataMiddlePoint).magnitude < (closestRoadPointData.middleRoadPoint - endRoadPointDataMiddlePoint).magnitude;
            var roadPointDataToPlayWith = isStartCloserToClosestRoadData ? roadData.roadPoints[0] : roadData.roadPoints[roadData.roadPoints.Count - 1];

            // Get the graph nodes for both the new road's endpoint and the existing road's endpoint
            var graphNodes = graphNodesToRoadPoints[roadPointDataToPlayWith];
            var connectingGraphNodes = roadDataForConnectingRoad.graphNodesForRoadPoints[closestRoadPointData].Select(cgn => cgn.graphNode).ToList();

            // Remove loop-back connections on the existing road's endpoint nodes.
            // These U-turn connections were added when the existing road was a dead-end;
            // now that it's being extended, vehicles should continue onto the new road instead.
            foreach (var connectingGraphNode in connectingGraphNodes)
            {
                var loopBackConnectionsToDelete = connectingGraphNode.WhereConnections(connection =>
                connectingGraphNodes.Contains(connection.Destination));
                Debug.Log("geee");
                foreach (var loopBackConnection in loopBackConnectionsToDelete)
                {

                    connectingGraphNode.RemoveConnection(loopBackConnection);
                }
            }

            // Create bidirectional connections between each pair of corresponding graph nodes
            // (existing road node[i] <-> new road node[i]), linking the two road segments together
            for (int i = 0; i < graphNodes.Count; i++)
            {
                var currentGraphNodeForRoadPoint = graphNodes[i];


                connectingGraphNodes[i].AddConnection(currentGraphNodeForRoadPoint.graphNode, 1);
                currentGraphNodeForRoadPoint.graphNode.AddConnection(connectingGraphNodes[i], 1);

            }
            Debug.Log("party");
        }
        // CASE 2: Connection point is in the middle of the existing road — creates a T-intersection.
        // This requires more complex wiring so it's delegated to a separate method.
        else
        {

            CreateIntersectionConnectionsForConnectingRoad(graphNodesToRoadPoints, roadDataForConnectingRoad, closestRoadPointData, roadPointData, indexOfClosestRoadData, isLeftCloserToPoint, isLast);
        }

        // Right-hand traffic: "yield to the right" — whichever road has the other
        // on its RIGHT side must yield. This applies to both the new and existing road.
        SetUpYieldRelationships(graphNodesToRoadPoints, roadPointData, roadData, roadDataForConnectingRoad, closestRoadPointData, isLast);
    }

    struct RoadAtIntersectionData
    {
        public Vector3 direction;
        public Vector3 directionToConnectionPoint;
        public List<GraphNodeForRoadPoint> graphNodes;
        public Vector3 middleRoadPoint;
        public string roadName;
        public List<GraphNodeForRoadPoint> graphNodesForNextRoadPoints;
    }
    /// <summary>
    /// Sets up right-hand traffic yield relationships at an intersection using the
    /// "yield to the right" rule. Discovers ALL roads meeting at the intersection
    /// point (not just the new + existing pair) and sets up pairwise yield
    /// relationships between every pair. This correctly handles 3-way, 4-way,
    /// and N-way intersections.
    /// </summary>
    private void SetUpYieldRelationships(
        Dictionary<RoadPointData, List<GraphNodeForRoadPoint>> graphNodesToRoadPoints,
        RoadPointData newRoadPoint,
        RoadData newRoadData,
        RoadData existingRoadData,
        RoadPointData existingRoadConnectionPoint,
        bool isLast)
    {
        // Collect all roads meeting at this intersection.
        // Each entry is: (road direction at intersection, graph nodes at intersection)
        var roadsAtIntersection = new List<RoadAtIntersectionData>();

        // 1. Add the new road
        Vector3 newRoadDirection = ComputeRoadDirectionAtEndpoint(newRoadData, isLast);
        var newRoadDirectionToConnectionPoint = ComputeRoadDirectionToConnectionPointAtEndpoint(newRoadData, isLast);
        roadsAtIntersection.Add(new RoadAtIntersectionData
        {
            direction = newRoadDirection,
            directionToConnectionPoint = newRoadDirectionToConnectionPoint,
            graphNodes = graphNodesToRoadPoints[newRoadPoint],
            middleRoadPoint = newRoadPoint.middleRoadPoint,
            roadName = newRoadData.roadName,
            graphNodesForNextRoadPoints = new List<GraphNodeForRoadPoint>()
        });

        // 2. Add the existing road
        Vector3 existingRoadDirection = ComputeRoadDirectionAtPoint(existingRoadData, existingRoadConnectionPoint);
        var existingRoadPointIndex = existingRoadData.roadPoints.IndexOf(existingRoadConnectionPoint);
        if (existingRoadPointIndex >= existingRoadData.roadPoints.Count - 1)
        {
            Debug.Log("Couldn't find connection point on existing road's list of road points, or connection point is at the end of the list. This should never happen.");
            return;
        }
        var nextRoadPoint = existingRoadData.roadPoints[existingRoadPointIndex + 1];
        var nextRoadPointGraphNodes = existingRoadData.graphNodesForRoadPoints[nextRoadPoint];
        roadsAtIntersection.Add(new RoadAtIntersectionData
        {
            direction = existingRoadDirection,
            directionToConnectionPoint = existingRoadDirection,
            graphNodes = existingRoadData.graphNodesForRoadPoints[existingRoadConnectionPoint],
            middleRoadPoint = existingRoadConnectionPoint.middleRoadPoint,
            roadName = existingRoadData.roadName,
            graphNodesForNextRoadPoints = nextRoadPointGraphNodes
        });
        // 3. Find any OTHER roads already connected at the same intersection point.
        //    These are roads whose start or end connection references the same point on the existing road.
        Vector3 intersectionWorldPos = existingRoadConnectionPoint.middleRoadPoint;
        var allRoadDatas = gameUIManager.GetAllRoadData();
        foreach (var otherRoad in allRoadDatas)
        {
            // Skip the new road and the existing road themselves
            if (otherRoad.roadName == newRoadData.roadName || otherRoad.roadName == existingRoadData.roadName)
                continue;

            // Check if this road's start or end connects near the same intersection point
            bool connectedAtStart = otherRoad.roadStartConnectionToOtherRoad != default
                && (otherRoad.roadStartConnectionToOtherRoad.connectionPoint.middleRoadPoint - intersectionWorldPos).magnitude < 0.1f;
            bool connectedAtEnd = otherRoad.roadEndConnectionToOtherRoad != default
                && (otherRoad.roadEndConnectionToOtherRoad.connectionPoint.middleRoadPoint - intersectionWorldPos).magnitude < 0.1f;

            if (connectedAtStart)
            {
                var dir = ComputeRoadDirectionAtEndpoint(otherRoad, false);
                var nodes = otherRoad.graphNodesForRoadPoints[otherRoad.roadPoints[0]];
                roadsAtIntersection.Add(new RoadAtIntersectionData
                {
                    direction = dir,
                    graphNodes = nodes,
                    middleRoadPoint = otherRoad.roadPoints[0].middleRoadPoint,
                    roadName = otherRoad.roadName,
                    graphNodesForNextRoadPoints = new List<GraphNodeForRoadPoint>()
                });
            }
            else if (connectedAtEnd)
            {
                var dir = ComputeRoadDirectionAtEndpoint(otherRoad, true);
                var nodes = otherRoad.graphNodesForRoadPoints[otherRoad.roadPoints[otherRoad.roadPoints.Count - 1]];
                roadsAtIntersection.Add(new RoadAtIntersectionData
                {
                    direction = dir,
                    graphNodes = nodes,
                    middleRoadPoint =
                otherRoad.roadPoints[otherRoad.roadPoints.Count - 1].middleRoadPoint,
                    roadName = otherRoad.roadName,
                    graphNodesForNextRoadPoints = new List<GraphNodeForRoadPoint>()
                });
            }
        }

        // Now set up pairwise yield-to-the-right between every pair of roads at this intersection.
        for (int a = 0; a < roadsAtIntersection.Count; a++)
        {
            for (int b = 0; b < roadsAtIntersection.Count; b++)
            {
                if (a == b) continue;

                var roadA = roadsAtIntersection[a];
                var roadB = roadsAtIntersection[b];

                // Yield-to-the-right: use road directions (not spatial positions) to
                // determine which road approaches from the right. roadA.direction and
                // roadB.direction both point toward the intersection, so -roadB.direction
                // is the direction road B's traffic comes FROM. Crossing with roadA.direction
                // tells us if that approach is on road A's right side.
                // This avoids near-zero cross products when roads meet at nearly the same
                // point or when the angle between them is very small / very large.
                Vector3 cross = Vector3.Cross(roadA.directionToConnectionPoint, -roadB.directionToConnectionPoint);

                if (cross.y > 0) // Road B approaches from road A's right → road A yields to road B
                {
                    SetYieldsBetween(roadA, roadB);
                }
            }
        }
    }

    /// <summary>
    /// Computes the forward direction of a road at its start or end endpoint.
    /// </summary>
    private Vector3 ComputeRoadDirectionAtEndpoint(RoadData road, bool atEnd)
    {
        if (road.roadPoints.Count < 2)
            return Vector3.forward;

        if (atEnd)
        {
            var secondToLast = road.roadPoints[road.roadPoints.Count - 2].middleRoadPoint;
            var last = road.roadPoints[road.roadPoints.Count - 1].middleRoadPoint;
            return (last - secondToLast).normalized;
        }
        else
        {
            var first = road.roadPoints[0].middleRoadPoint;
            var second = road.roadPoints[1].middleRoadPoint;
            return (second - first).normalized;
        }
    }

    private Vector3 ComputeRoadDirectionToConnectionPointAtEndpoint(RoadData road, bool atEnd)
    {
        if (road.roadPoints.Count < 2)
            return Vector3.forward;

        if (atEnd)
        {
            var secondToLast = road.roadPoints[road.roadPoints.Count - 2].middleRoadPoint;
            var last = road.roadPoints[road.roadPoints.Count - 1].middleRoadPoint;
            return (last - secondToLast).normalized;
        }
        else
        {
            var first = road.roadPoints[0].middleRoadPoint;
            var second = road.roadPoints[1].middleRoadPoint;
            return (first - second).normalized;
        }
    }

    /// <summary>
    /// Computes the forward direction of a road at a specific road point (mid-road intersection).
    /// </summary>
    private Vector3 ComputeRoadDirectionAtPoint(RoadData road, RoadPointData point)
    {
        var index = road.roadPoints.IndexOf(point);
        if (index < road.roadPoints.Count - 1)
        {
            var current = road.roadPoints[index].middleRoadPoint;
            var next = road.roadPoints[index + 1].middleRoadPoint;
            return (next - current).normalized;
        }
        else if (index > 0)
        {
            var prev = road.roadPoints[index - 1].middleRoadPoint;
            var current = road.roadPoints[index].middleRoadPoint;
            return (current - prev).normalized;
        }
        return Vector3.forward;
    }

    /// <summary>
    /// Returns the average world position of a set of graph nodes (used as the road's spatial
    /// location at an intersection for the cross product direction check).
    /// </summary>
    private Vector3 GetAveragePosition(List<GraphNodeForRoadPoint> graphNodes)
    {
        Vector3 sum = Vector3.zero;
        int count = 0;
        foreach (var gn in graphNodes)
        {
            var go = gn.graphNode.Value.GetGameObject();
            if (go != null)
            {
                sum += go.transform.position;
                count++;
            }
        }
        return count > 0 ? sum / count : Vector3.zero;
    }

    /// <summary>
    /// Makes all Road components on roadA's graph nodes yield to all Road components on roadB's graph nodes.
    /// </summary>
    private void SetYieldsBetween(RoadAtIntersectionData yieldingRoadData, RoadAtIntersectionData priorityRoadData)
    {

        var angleBetweenRoads = Vector3.SignedAngle(yieldingRoadData.direction, priorityRoadData.direction, Vector3.up);
        foreach (var yieldingNode in yieldingRoadData.graphNodes)
        {
            var yieldingRoad = yieldingNode.graphNode.Value.GetGameObject().GetComponent<Road>();


            if (yieldingRoad == null) continue;
            var yieldingRoadPointsToPriorityRoadPoints = RayLineIntersection2D(yieldingRoadData.middleRoadPoint, yieldingRoadData.direction, priorityRoadData.middleRoadPoint, priorityRoadData.direction, out Vector3 intersection);
            if (yieldingRoadPointsToPriorityRoadPoints)
            {
               if (yieldingNode.pointSide == PointSide.Left) continue;
            } else
            {
                 if (yieldingNode.pointSide == PointSide.Right) continue;
            }
            // special case when the yieldng road is pointing away from the intersection point

            foreach (var priorityNode in priorityRoadData.graphNodes)
            {
                var priorityRoad = priorityNode.graphNode.Value.GetGameObject().GetComponent<Road>();
                if (priorityRoad == null) continue;
                if (angleBetweenRoads > 0 && priorityNode.pointSide == PointSide.Left && yieldingRoadPointsToPriorityRoadPoints)
                {

                    yieldingRoad.AddYieldsToRoad(priorityRoad);
                } else if (angleBetweenRoads > 0 && priorityNode.pointSide == PointSide.Right && !yieldingRoadPointsToPriorityRoadPoints)
                {

                    yieldingRoad.AddYieldsToRoad(priorityRoad);
                }
                else if (angleBetweenRoads < 0 && priorityNode.pointSide == PointSide.Right && yieldingRoadPointsToPriorityRoadPoints)
                {

                    yieldingRoad.AddYieldsToRoad(priorityRoad);
                }
                else if (angleBetweenRoads < 0 && priorityNode.pointSide == PointSide.Left && !yieldingRoadPointsToPriorityRoadPoints)
                {

                    yieldingRoad.AddYieldsToRoad(priorityRoad);
                }
            }
        }

        if (priorityRoadData.graphNodesForNextRoadPoints.Count > 0)
        {
            // p+1 left should yield to yield right/left
            foreach (var priorityNode in priorityRoadData.graphNodesForNextRoadPoints)
            {
                if (priorityNode.pointSide == PointSide.Right) continue;
                foreach (var yieldingNode in yieldingRoadData.graphNodes)
                {
                    var yieldingRoad = yieldingNode.graphNode.Value.GetGameObject().GetComponent<Road>();
                    var priorityRoad = priorityNode.graphNode.Value.GetGameObject().GetComponent<Road>();
                    if (yieldingRoad != null && priorityRoad != null)
                    {
                        if (angleBetweenRoads > 0 && yieldingNode.pointSide == PointSide.Left)
                        {
                            priorityRoad.AddYieldsToRoad(yieldingRoad);
                        }
                        else if (angleBetweenRoads < 0 && yieldingNode.pointSide == PointSide.Right)
                        {
                            priorityRoad.AddYieldsToRoad(yieldingRoad);
                        }
                    }
                }
            }
        }
        else if (yieldingRoadData.graphNodesForNextRoadPoints.Count > 0)
        {
            // p right/left should yield to y+1 right

            foreach (var yieldingNode in yieldingRoadData.graphNodesForNextRoadPoints)
            {
                if (yieldingNode.pointSide == PointSide.Right) continue;
                foreach (var priorityNode in priorityRoadData.graphNodes)
                {
                    var yieldingRoad = yieldingNode.graphNode.Value.GetGameObject().GetComponent<Road>();
                    var priorityRoad = priorityNode.graphNode.Value.GetGameObject().GetComponent<Road>();
                    if (yieldingRoad != null && priorityRoad != null)
                    {
                        if (angleBetweenRoads > 0 && priorityNode.pointSide == PointSide.Left)
                        {
                            priorityRoad.AddYieldsToRoad(yieldingRoad);
                        }
                        else if (angleBetweenRoads < 0 && priorityNode.pointSide == PointSide.Right)
                        {
                            priorityRoad.AddYieldsToRoad(yieldingRoad);
                        }
                    }
                }
            }
        }

    }

    public static bool RayLineIntersection2D(Vector3 rayOrigin, Vector3 rayDir,
    Vector3 linePoint, Vector3 lineDir, out Vector3 intersection)
    {
        intersection = Vector3.zero;

        // cross product of ray, and line direction
        float denom = rayDir.x * lineDir.z - rayDir.z * lineDir.x;

        // Parallel check
        if (Mathf.Abs(denom) < 1e-6f)
            return false;

        Vector3 diff = linePoint - rayOrigin;
        float rayOriginToLinePointCross = diff.x * lineDir.z - diff.z * lineDir.x;
        float t = rayOriginToLinePointCross / denom;

        // Ray only goes forward (t >= 0)
        if (t < 0)
            return false;

        intersection = rayOrigin + rayDir * t;
        return true;
    }

    /// <summary>
    /// Creates the navigation graph connections for a T-intersection where a new road connects
    /// to the MIDDLE of an existing road. This wires up the new road's lane graph nodes to the
    /// existing road's lane graph nodes on both sides of the intersection point, respecting
    /// traffic direction (controlled by the isLast flag).
    ///
    /// The existing road is split conceptually at the intersection into two halves:
    ///   - graphNodes1: the graph nodes at the intersection point on the existing road
    ///   - graphNodes2: the graph nodes at the next road point along the existing road
    /// These may be swapped based on which side of the existing road is closer (isLeftCloserToPoint).
    ///
    /// For each lane node on the new road (left-side and right-side), connections are made to:
    ///   1. The corresponding lane node on the existing road (same-side connection for traffic flow).
    ///   2. Nodes from the opposite side of the existing road (cross-lane turning connections).
    ///   3. External connections from the existing road that should now also reach the new road.
    /// </summary>
    private void CreateIntersectionConnectionsForConnectingRoad(Dictionary<RoadPointData, List<GraphNodeForRoadPoint>> graphNodesToRoadPoints, RoadData roadDataForConnectingRoad, RoadPointData closestRoadPointData, RoadPointData roadPointData, int indexOfClosestRoadData, bool isLeftCloserToPoint, bool isLast)
    {
        // Get the next road point after the intersection on the existing road
        var connectingRoadPointForLeftConnection = roadDataForConnectingRoad.roadPoints[indexOfClosestRoadData + 1];

        // graphNodes1 = nodes at the intersection point, graphNodes2 = nodes at the adjacent road point
        var graphNodes1 = roadDataForConnectingRoad.graphNodesForRoadPoints[closestRoadPointData];
        var graphNodes2 = roadDataForConnectingRoad.graphNodesForRoadPoints[connectingRoadPointForLeftConnection];

        // Swap node groups so that graphNodes1 is always on the side closer to the new road
        if (!isLeftCloserToPoint)
        {
            var temp = graphNodes1;
            graphNodes1 = graphNodes2;
            graphNodes2 = temp;
        }

        // Get the graph nodes that were just created for the new road's endpoint at the intersection
        var createdRoadGraphNodes = graphNodesToRoadPoints[roadPointData];
        var leftIndex = 0;
        var rightIndex = 0;
        var countOfLeftCreatedPoint = createdRoadGraphNodes.Count(gn => gn.pointSide == PointSide.Left);
        var countOfRightCreatedPoint = createdRoadGraphNodes.Count(gn => gn.pointSide == PointSide.Right);

        // Double-swap effectively restores original order when !isLeftCloserToPoint,
        // ensuring consistent orientation for the connection logic below
        if (!isLeftCloserToPoint)
        {
            var temp = graphNodes1;
            graphNodes1 = graphNodes2;
            graphNodes2 = temp;
            // createdRoadGraphNodes.Reverse();
        }

        // graphNodes1.ForEach(gn =>
        // {
        //     gn.graphNode.Value.SetHighlightColor(Color.red);
        //     gn.graphNode.Value.ToggleHighlight(true);

        // });

        // graphNodes2.ForEach(gn =>
        // {
        //     gn.graphNode.Value.SetHighlightColor(Color.yellow);
        //     gn.graphNode.Value.ToggleHighlight(true);

        // });
        // Iterate over every graph node created for the new road's endpoint and wire it
        // into the existing road's navigation graph. Left-side and right-side lane nodes
        // are handled separately because traffic flows in opposite directions on each side.
        for (int i = 0; i < createdRoadGraphNodes.Count; i++)
        {
            var createdRoadGraphNode = createdRoadGraphNodes[i];

            // ===== LEFT-SIDE lane node of the new road =====
            if (createdRoadGraphNode.pointSide == PointSide.Left)
            {
                // --- Step 1: Connect to the same-side (left) lane on the existing road ---
                // Pick the left-lane nodes from the appropriate side of the existing road
                // (graphNodes2 when connecting at end, graphNodes1 when connecting at start)
                var leftGraphNodes = (isLast ? graphNodes2 : graphNodes1).Where(gn => gn.pointSide == PointSide.Left).ToList();
                var leftGraphNode = leftGraphNodes[leftIndex >= leftGraphNodes.Count ? leftGraphNodes.Count - 1 : leftIndex].graphNode;

                // Create a directed connection: traffic flows FROM existing road TO new road (isLast)
                // or FROM new road TO existing road (!isLast)
                if (isLast)
                {
                    leftGraphNode.AddConnection(createdRoadGraphNode.graphNode, 1);
                }
                else
                {
                    createdRoadGraphNode.graphNode.AddConnection(leftGraphNode, 1);
                }


                // --- Step 2: Re-route existing external connections through the new intersection ---
                // Find the left-lane node on the OTHER side of the intersection on the existing road
                var leftOtherConnectedGraphNode = (isLast ? graphNodes1 : graphNodes2).Where(gn => gn.pointSide == PointSide.Left).ToList()[leftIndex].graphNode;

                // Find connections that go through this node but DON'T stay within the intersection itself.
                // These are "pass-through" connections from the rest of the existing road that should
                // also be reachable from/to the new road.
                var connections = leftOtherConnectedGraphNode.WhereConnections(conn =>
                {
                    return (isLast ? conn.Destination : conn.Source) == leftOtherConnectedGraphNode && !(isLast ? graphNodes2 : graphNodes1).Select(gn => gn.graphNode).Contains(isLast ? conn.Source : conn.Destination);
                });
                foreach (var connection in connections)
                {
                    // leftOtherConnectedGraphNode.Value.SetHighlightColor(Color.gray);
                    // leftOtherConnectedGraphNode.Value.ToggleHighlight(true);
                    // connection.Source.Value.SetHighlightColor(Color.cyan);
                    // createdRoadGraphNode.graphNode.Value.SetHighlightColor(Color.magenta);
                    // leftOtherConnectedGraphNode.Value.SetHighlightColor(Color.magenta);

                    // connection.Source.Value.ToggleHighlight(true);
                    // createdRoadGraphNode.graphNode.Value.ToggleHighlight(true);

                    // Add a connection so that traffic from/to the rest of the existing road
                    // can also reach the new road through this intersection
                    if (isLast)
                    {
                        connection.Source.AddConnection(createdRoadGraphNode.graphNode, 1);
                    }
                    else
                    {
                        createdRoadGraphNode.graphNode.AddConnection(connection.Destination, 1);
                    }
                }

                // --- Step 3: Cross-lane connection (left new road node -> right existing road node) ---
                // This allows vehicles to turn across lanes at the intersection
                var rightGraphNodes = (isLast ? graphNodes1 : graphNodes2).Where(gn => gn.pointSide == PointSide.Right).ToList();
                var rightGraphNodeToConnectTo = rightGraphNodes[leftIndex].graphNode;
                if (isLast)
                {
                    rightGraphNodeToConnectTo.AddConnection(createdRoadGraphNode.graphNode, 1);
                }
                else
                {
                    createdRoadGraphNode.graphNode.AddConnection(rightGraphNodeToConnectTo, 1);
                }

                leftIndex++;

                // --- Step 4: Handle width mismatch ---
                // If the new road has fewer left-lane nodes than the existing road has right-lane nodes,
                // connect the remaining right-lane nodes to the last left-lane node of the new road
                // so no existing lane is left disconnected.
                if (leftIndex >= countOfLeftCreatedPoint && countOfLeftCreatedPoint < rightGraphNodes.Count)
                {
                    while (leftIndex < rightGraphNodes.Count)
                    {
                        var idx = leftIndex >= rightGraphNodes.Count ? rightGraphNodes.Count - 1 : leftIndex;
                        if (isLast)
                        {
                            rightGraphNodes[rightGraphNodes.Count - idx].graphNode.AddConnection(createdRoadGraphNode.graphNode, 1);
                        }
                        else
                        {
                            createdRoadGraphNode.graphNode.AddConnection(rightGraphNodes[rightGraphNodes.Count - idx].graphNode, 1);
                        }

                        leftIndex++;
                    }
                }
            }
            // ===== RIGHT-SIDE lane node of the new road =====
            else
            {
                // --- Step 1: Re-route existing external connections through the new intersection ---
                // Find the right-lane node on the other side of the intersection on the existing road
                var rightOtherConnectedGraphNode = (isLast ? graphNodes1 : graphNodes2).Where(gn => gn.pointSide == PointSide.Right).ToList()[rightIndex].graphNode;

                // Find external connections (not within the intersection or the new road itself)
                // that should now also connect to this new road node
                var connections = rightOtherConnectedGraphNode.WhereConnections(conn =>
                {
                    return (isLast ? conn.Source : conn.Destination) == rightOtherConnectedGraphNode && !createdRoadGraphNodes.Select(gn => gn.graphNode).Contains(isLast ? conn.Destination : conn.Source) && !(isLast ? graphNodes2 : graphNodes1).Select(gn => gn.graphNode).Contains(isLast ? conn.Destination : conn.Source);
                });
                foreach (var connection in connections)
                {
                    // connection.Destination.Value.SetHighlightColor(Color.cyan);
                    // createdRoadGraphNode.graphNode.Value.SetHighlightColor(Color.magenta);
                    // // leftOtherConnectedGraphNode.Value.SetHighlightColor(Color.magenta);

                    // connection.Destination.Value.ToggleHighlight(true);
                    // createdRoadGraphNode.graphNode.Value.ToggleHighlight(true);

                    // Connect through so traffic from the existing road can reach/leave the new road
                    if (isLast)
                    {
                        createdRoadGraphNode.graphNode.AddConnection(connection.Destination, 1);
                    }
                    else
                    {
                        connection.Source.AddConnection(createdRoadGraphNode.graphNode, 1);
                    }
                }

                // --- Step 2: Cross-lane connection (right new road node -> left existing road node) ---
                // Enables turning across lanes; uses a mirrored index so that the outermost
                // right node connects to the outermost left node on the opposite side
                var leftGraphNodes = (isLast ? graphNodes1 : graphNodes2).Where(gn => gn.pointSide == PointSide.Left).ToList();
                var indexForLeftGraphPoints = leftGraphNodes.Count - 1 - rightIndex;
                if (indexForLeftGraphPoints < 0)
                {
                    indexForLeftGraphPoints = 0;
                }
                if (isLast)
                {
                    createdRoadGraphNode.graphNode.AddConnection(leftGraphNodes[indexForLeftGraphPoints].graphNode, 1);
                }
                else
                {
                    leftGraphNodes[indexForLeftGraphPoints].graphNode.AddConnection(createdRoadGraphNode.graphNode, 1);
                }

                // --- Step 3: Connect to the same-side (right) lane on the existing road ---
                // This is the direct forward-flow connection along the right lane
                var rightGraphNodes = (isLast ? graphNodes2 : graphNodes1).Where(gn => gn.pointSide == PointSide.Right).ToList();
                var rightGraphNode = rightGraphNodes[rightIndex].graphNode;
                if (isLast)
                {
                    createdRoadGraphNode.graphNode.AddConnection(rightGraphNodes[rightIndex >= rightGraphNodes.Count ? rightGraphNodes.Count - 1 : rightIndex].graphNode, 1);
                }
                else
                {
                    rightGraphNodes[rightIndex >= rightGraphNodes.Count ? rightGraphNodes.Count - 1 : rightIndex].graphNode.AddConnection(createdRoadGraphNode.graphNode, 1);
                }
                rightIndex++;
            }
        }
    }

    private (AbstractBuildingType, SelectableObject) CreatePointAtPositionWithColor(Vector3 position, Color color, GameObject roadMesh, int index)
    {

        var (point, go) = CreateBuildingFromBuildingData(selectedBuildingData, parent: roadMesh.transform);
        var gridPosition = new Vector3Int();
        point.PlaceAtPosition(new Dictionary<(Vector3, Quaternion), List<Vector3Int>>() {
            {(position, Quaternion.identity), new List<Vector3Int>{gridPosition}},
        }, new Dictionary<Vector3Int, NeighbourData>(), default, point.gameObject);
        point.name = $"Point idx: {index} as x: {position.x} y: {position.y} z: {position.z}";
        var highlight = go.GetComponent<Highlight>();
        if (highlight != null)
        {
            highlight.SetHighlightColor(color);
            highlight.ToggleHighlight(true);
        }
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
