using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Splines;
using UnityEngine.UIElements;

public enum SplinePointType
{
    FollowMeshCurve,
    LevelTerrain,
    Bridge,
    Tunnel
}



public class RoadPointData
{
    public Vector3 leftRoadPoint;
    public Vector3 rightRoadPoint;
    public Vector3 middleRoadPoint;
    public float roadWidth;
    public SplinePointType leftRoadPointType;
    public SplinePointType rightRoadPointType;
    public SplinePointType middleRoadPointType;
    public float splineValue;
    public int roadSectionIndex;
}
enum RoadSide
{
    Left,
    Right
}

public class EmptyTileData
{
    public int roadSectionIndex;
    public Vector3 perpendicularToRoadVector;
    public Vector3 roadMiddlePoint;
    public Vector3 roadDirectionVector;
    public Vector3 roadStartPoint;
    public Vector3 position;
    public Quaternion rotation;
    public RoadPointData closestRoadPointData;
    public GameObject gameObject;
}

public struct BatchData
{
    public List<List<EmptyTileData>> emptyTileDatas;
    public List<GameObject> tileObjects;
    public int batchIndex;
}
public class RoadConnection
{
    public RoadPointData connectionPoint;
    public string connectingRoadName;
}

public enum PointSide
{
    Right,
    Left
}
public struct GraphNodeForRoadPoint
{
    public GraphNode<SelectableObject> graphNode;
    public PointSide pointSide;
}
public struct RoadData
{
    public GameObject roadMesh;
    public string roadName;
    public List<RoadPointData> roadPoints;
    public Dictionary<RoadPointData, List<GraphNodeForRoadPoint>> graphNodesForRoadPoints;
    public List<List<BatchData>> batchesOnRight;
    public List<List<BatchData>> batchesOnLeft;
    public RoadConnection roadStartConnectionToOtherRoad;
    public RoadConnection roadEndConnectionToOtherRoad;

}

public delegate void RoadCreation(RoadData roadData);

public class RoadDrawer : MonoBehaviour
{
    public event RoadCreation RoadCreated;
    public Material roadNaterial;
    public Camera playerCamera;
    public GameObject point;
    public GameObject emptyTilePrefab;
    private GameObject pointInstance;
    private List<GameObject> emptyTileList = new List<GameObject>();
    public float emptyTileOffset = 1.0f;
    public float emptyTileGutter = 1.0f;
    public float maximumAngleDifferenceForBatch = 20.0f;
    public float minimumAngleToChangeVectorsInBatch = 3.0f;
    private List<GameObject> splineGuidingPointInstances = new List<GameObject>();
    private List<GameObject> splinePointInstances = new List<GameObject>();
    private List<Spline> splines = new List<Spline>();
    Vector3 LastMousePosition = Vector3.zero;
    SplineContainer splineContainer;

    [Range(0, 50)]
    public float heightDelta = 0.2f;
    [Range(0, 50)]
    public float levelTerrainDistanceDelta = 1.0f;
    [Range(0, 50)]
    public float bridgeDistanceDelta = 1.8f;
    [Range(1, 50)]
    public int resolution = 5;
    [Range(0, 50)]
    public float roadWidth = 1.0f;
    public int tilesToPlace = 3;
    public bool autoUpdateEnabled = true;
    public float pointClosenessDelta = 0.0001f;
    private int raycastLayerMask;
    public float raycastDistance = 20.0f;
    List<RoadPointData> splineRoadPoints = new List<RoadPointData>();

    Coroutine rightBatchDrawerCorutine = default;
    Coroutine leftBatchDrawerCorutine = default;

    private static int roadIndex = 0;
    private bool isDrawingRoad = false;
    private List<GameObject> roadMeshes = new List<GameObject>();
    private Dictionary<string, RoadData> roadPointDatasForRoads = new Dictionary<string, RoadData>();

    private GameUIManager uiManager;
    public RoadData GetRoadDataForRoad(string roadName)
    {
        return roadPointDatasForRoads[roadName];
    }

    public void SetRoadDataForRoad(string roadName, RoadData roadData)
    {
        roadPointDatasForRoads[roadName] = roadData;
    }
    private string currentRoadName = GetRoadNameForIndex(roadIndex);
    public float roadPointClosenessDelta = 5.0f;
    public bool closeToOtherRoad = false;

    private static string GetRoadNameForIndex(int index)
    {
        return $"road {index}";
    }
    // Start is called before the first frame update
    void Start()
    {
        splineContainer = gameObject.AddComponent<SplineContainer>();
        raycastLayerMask = LayerMask.GetMask("Ground");
        uiManager = FindObjectOfType<GameUIManager>();
    }

    public void EnableDrawing(int roadWidth)
    {
        isDrawingRoad = true;
        this.roadWidth = roadWidth;
    }

    public void DisableDrawing()
    {
        ClearRoads();
        isDrawingRoad = false;
    }
    (string roadName, RoadPointData closestRoadPointData) lastRoadNameAndClosestRoadPointData = default;
    (string roadName, RoadPointData closestRoadPointData) firstRoadNameAndClosestRoadPointData = default;
    // Update is called once per frame
    int pointIdx = 0;
    void Update()
    {

        if (uiManager != null && uiManager.IsMouseOverUI())
        {
            Debug.Log("Mouse over UI");
        }
     
        if (isDrawingRoad && uiManager != null && !uiManager.IsMouseOverUI())
        {
            LastMousePosition = Input.mousePosition;
            Vector2 view = FindObjectOfType<Camera>().ScreenToViewportPoint(Input.mousePosition);
            bool isOutside = view.x < 0 || view.x > 1 || view.y < 0 || view.y > 1;
            if (isOutside)
            {
                return;
            }
            Ray ray = playerCamera.ScreenPointToRay(LastMousePosition);


            if (Physics.Raycast(ray, out RaycastHit rayHit))
            {

                if (rayHit.collider.gameObject.GetComponent<TimeManager>() != null)
                {

                    if (pointInstance == default)
                    {
                        var tempPoint = Instantiate(point);
                        tempPoint.transform.position = rayHit.point;
                        pointInstance = tempPoint;
                        pointInstance.name = $"Point #{pointIdx++}";
                    }
                    if ((pointInstance.transform.position - rayHit.point).magnitude > 3.0f)
                    {
                        if (splineGuidingPointInstances.Count >= 1)
                        {
                            splineGuidingPointInstances.RemoveAt(splineGuidingPointInstances.Count - 1);
                        }
                        Destroy(pointInstance);
                        var tempPoint = Instantiate(point);
                        var tempPointPosition = rayHit.point;
                        foreach (var (roadkey, roadDatas) in roadPointDatasForRoads)
                        {
                            var closestMiddleRoadPoint = roadDatas.roadPoints.Select(roadPointData =>
                            {
                                var magnitude = (roadPointData.middleRoadPoint - tempPointPosition).magnitude;
                                return (roadPointData, roadPointData.middleRoadPoint, magnitude);
                            })
                            .Where((roadPointData) => roadPointData.magnitude <= roadPointClosenessDelta)
                            .OrderBy((rd) => rd.magnitude)
                            .FirstOrDefault();

                            if (closestMiddleRoadPoint == default)
                            {
                                closeToOtherRoad = false;
                                lastRoadNameAndClosestRoadPointData = default;
                                continue;
                            }
                            else
                            {
                                lastRoadNameAndClosestRoadPointData = (roadkey, closestMiddleRoadPoint.roadPointData);
                                tempPointPosition = closestMiddleRoadPoint.middleRoadPoint;
                                closeToOtherRoad = true;
                                break;
                            }
                        }
                        tempPoint.transform.position = tempPointPosition;
                        pointInstance = tempPoint;


                        splineGuidingPointInstances.Add(pointInstance);
                        if (splineGuidingPointInstances.Count > 1)
                        {
                            var splinePoints = splineGuidingPointInstances.Select(sp => sp.transform.position).ToList();

                            var (splineRoadPoints, batchesOnRight, batchesOnLeft) = GetCurveBetweenPoints(splinePoints, roadWidth, lastRoadNameAndClosestRoadPointData, firstRoadNameAndClosestRoadPointData);
                            DrawRoadCurve(splineRoadPoints, batchesOnRight, batchesOnLeft);
                        }

                    }
                }

                if (Input.GetMouseButtonDown(0) && pointInstance != default)
                {

                    var distanceBetweenLastPointAndMousePoint = splineGuidingPointInstances.Count > 1 ? (splineGuidingPointInstances[splineGuidingPointInstances.Count - 2].transform.position - pointInstance.transform.position).magnitude : float.MaxValue;
                    if (distanceBetweenLastPointAndMousePoint > 5.0f && lastRoadNameAndClosestRoadPointData == default)
                    {
                        splineGuidingPointInstances.Add(pointInstance);
                        pointInstance = default;
                    }
                    else if (splineGuidingPointInstances.Count <= 1 && lastRoadNameAndClosestRoadPointData != default)
                    {
                        firstRoadNameAndClosestRoadPointData = lastRoadNameAndClosestRoadPointData;
                        lastRoadNameAndClosestRoadPointData = default;

                        splineGuidingPointInstances.Add(pointInstance);
                        pointInstance = default;
                    }
                    else if (splineGuidingPointInstances.Count >= 2)
                    {

                        var splinePoints = splineGuidingPointInstances.Select(sp => sp.transform.position).ToList();
                        if (splinePoints.Count > 2 && lastRoadNameAndClosestRoadPointData == default)
                        {
                            splinePoints.RemoveAt(splinePoints.Count - 1);
                        }
                        var (splineRoadPoints, batchesOnRight, batchesOnLeft) = GetCurveBetweenPoints(splinePoints, roadWidth, lastRoadNameAndClosestRoadPointData, firstRoadNameAndClosestRoadPointData, true);

                        roadPointDatasForRoads.Remove(currentRoadName);

                        var roadData = new RoadData
                        {
                            roadMesh = roadMeshes[0],
                            roadName = currentRoadName,
                            batchesOnLeft = batchesOnLeft,
                            batchesOnRight = batchesOnRight,
                            roadPoints = new List<RoadPointData>(splineRoadPoints),
                            roadStartConnectionToOtherRoad = default,
                            roadEndConnectionToOtherRoad = default

                        };


                        if (firstRoadNameAndClosestRoadPointData != default)
                        {
                            roadData.roadStartConnectionToOtherRoad = new RoadConnection()
                            {
                                connectingRoadName = firstRoadNameAndClosestRoadPointData.roadName,
                                connectionPoint = firstRoadNameAndClosestRoadPointData.closestRoadPointData

                            };
                        }

                        if (lastRoadNameAndClosestRoadPointData != default)
                        {
                            roadData.roadEndConnectionToOtherRoad = new RoadConnection()
                            {
                                connectingRoadName = lastRoadNameAndClosestRoadPointData.roadName,
                                connectionPoint = lastRoadNameAndClosestRoadPointData.closestRoadPointData

                            };
                        }


                        roadPointDatasForRoads[currentRoadName] = roadData;
                        StartCoroutine(FireRoadCreatedEvent(currentRoadName, roadMeshes[0], splineRoadPoints, batchesOnRight, batchesOnLeft));
                        firstRoadNameAndClosestRoadPointData = default;
                        lastRoadNameAndClosestRoadPointData = default;
                        roadMeshes.RemoveAt(0);
                        currentRoadName = GetRoadNameForIndex(++roadIndex);
                        ClearRoads();
                        //DrawRoadCurve(splineRoadPoints, batchesOnRight, batchesOnLeft);
                    }
                }

            }


        }

        if (Input.GetMouseButtonDown(1))
        {
            //DrawRoadMesh();
            isDrawingRoad = false;
            ClearRoads();
        }

    }

    IEnumerator FireRoadCreatedEvent(string currentRoadName, GameObject roadMesh, List<RoadPointData> splineRoadPoints, List<List<BatchData>> batchesOnRight, List<List<BatchData>> batchesOnLeft)
    {
        var emptyTilesOnRight = new List<GameObject>();
        yield return DrawBathces(batchesOnRight, emptyTilesOnRight, roadMesh, "right");
        var emptyTilesOnLeft = new List<GameObject>();
        yield return DrawBathces(batchesOnLeft, emptyTilesOnLeft, roadMesh, "left");


        RoadCreated?.Invoke(roadPointDatasForRoads[currentRoadName]);
        yield return null;
    }
    private void RemoveRoadMeshes()
    {
        roadMeshes.ForEach(rm => Destroy(rm));
        roadMeshes.Clear();
    }
    private void DrawRoadMesh(List<RoadPointData> splineRoadPoints)
    {

        RemoveRoadMeshes();
        var road = new GameObject(currentRoadName);
        road.tag = "RoadMeshes";
        roadMeshes.Add(road);
        var mesh = road.AddComponent<RoadMesh>();

        mesh.DrawMesh(MeshGenerator.GenerateRoadMesh(splineRoadPoints), roadNaterial);

    }

    private void RemoveSplinePoints()
    {
        splinePointInstances.ForEach(mp => Destroy(mp));
        splinePointInstances.Clear();
    }


    public void ClearRoads()
    {
        if (rightBatchDrawerCorutine != null)
        {
            StopCoroutine(rightBatchDrawerCorutine);
        }
        if (leftBatchDrawerCorutine != null)
        {
            StopCoroutine(leftBatchDrawerCorutine);
        }
        splineGuidingPointInstances.ForEach(r => Destroy(r));
        splineGuidingPointInstances.Clear();
        lastRoadNameAndClosestRoadPointData = default;
        firstRoadNameAndClosestRoadPointData = default;
        RemoveSplinePoints();
        RemoveRoadMeshes();
        if (pointInstance != default)
        {
            Destroy(pointInstance);
            pointInstance = default;
        }
    }


    public void DrawRoadCurve(List<RoadPointData> splineRoadPoints = default, List<List<BatchData>> batchesOnRight = default, List<List<BatchData>> batchesOnLeft = default)
    {
        RemoveSplinePoints();
        if (splineRoadPoints == default)
        {
            (splineRoadPoints, batchesOnRight, batchesOnLeft) = GetCurveBetweenPoints(splineGuidingPointInstances.Select(sp => sp.transform.position).ToList(), roadWidth);
        }
        if (splineRoadPoints != default)
        {
            DrawRoadWithEmptyTiles(splineRoadPoints, batchesOnRight, batchesOnLeft);
        }
    }

    private void DrawRoadWithEmptyTiles(List<RoadPointData> splineRoadPoints, List<List<BatchData>> batchesOnRight, List<List<BatchData>> batchesOnLeft)
    {
        DrawRoadMesh(splineRoadPoints);

        if (rightBatchDrawerCorutine != default)
        {
            StopCoroutine(rightBatchDrawerCorutine);
        }

        if (leftBatchDrawerCorutine != default)
        {
            StopCoroutine(leftBatchDrawerCorutine);
        }
        rightBatchDrawerCorutine = StartCoroutine(DrawBathces(batchesOnRight, emptyTileList, roadMeshes[0], "right"));
        leftBatchDrawerCorutine = StartCoroutine(DrawBathces(batchesOnLeft, emptyTileList, roadMeshes[0], "left"));

    }

    public (List<RoadPointData>, List<List<BatchData>>, List<List<BatchData>>) GetCurveBetweenPoints(List<Vector3> points,
    float roadWidth,
    (string roadName, RoadPointData closestRoadPoint) lastRoadNameAndClosesRoadPointData = default,
    (string roadName, RoadPointData closestRoadPoint) firstRoadNameAndClosestRoadPointData = default,
    bool roadReady = false)
    {
        if (points.Count < 2)
        {
            return (default, default, default);
        }
        for (int i = 0; i < splines.Count; i++)
        {
            splineContainer.RemoveSplineAt(i);
        }
        if (roadReady)
        {
            Debug.Log("Readt");
        }
        splines.Clear();
        emptyTileList.ForEach(et => Destroy(et));
        emptyTileList.Clear();
        Spline leftSpline = splineContainer.AddSpline();
        Spline rightSpline = splineContainer.AddSpline();
        Spline middleSpline = splineContainer.AddSpline();
        splineRoadPoints.Clear();
        RoadPointData lastRoadData = default;
        RoadPointData firstRoadData = default;
        bool shouldSwitchSides = false;
        if (lastRoadNameAndClosesRoadPointData != default)
        {
            bool shouldSwitch;
            Vector3 middleRoadPoint;
            GetRoadPointAndMiddlePointForPointConnectingToOtherRoad(points, roadWidth, lastRoadNameAndClosesRoadPointData, true, out lastRoadData, out middleRoadPoint, out shouldSwitch);
            points.Add(middleRoadPoint);
            shouldSwitchSides = shouldSwitchSides || shouldSwitch;
        }

        if (firstRoadNameAndClosestRoadPointData != default)
        {
            bool shouldSwitch;
            Vector3 middleRoadPoint;
            GetRoadPointAndMiddlePointForPointConnectingToOtherRoad(points, roadWidth, firstRoadNameAndClosestRoadPointData, false, out firstRoadData, out middleRoadPoint, out shouldSwitch);
            points.Insert(0, middleRoadPoint);
            shouldSwitchSides = shouldSwitchSides || shouldSwitch;
        }

        GetSplineForTerrainBetweenPoints(points, out splineRoadPoints, roadWidth);

        if (lastRoadData != default)
        {
            splineRoadPoints.RemoveAt(splineRoadPoints.Count - 1);
            lastRoadData.roadSectionIndex = splineRoadPoints.Max(sp => sp.roadSectionIndex);
            splineRoadPoints.Add(lastRoadData);
        }
        if (firstRoadData != default)
        {
            splineRoadPoints.RemoveAt(0);
            firstRoadData.roadSectionIndex = 0;
            splineRoadPoints.Insert(0, firstRoadData);
        }
        if (shouldSwitchSides)
        {
            var maxRoadSectionIndexValue = splineRoadPoints.Max(sp => sp.roadSectionIndex);
            for (int i = 0; i < splineRoadPoints.Count; i++)
            {
                var tempRoadPoint = splineRoadPoints[i].rightRoadPoint;
                var tempPointType = splineRoadPoints[i].rightRoadPointType;
                splineRoadPoints[i].rightRoadPoint = splineRoadPoints[i].leftRoadPoint;
                splineRoadPoints[i].rightRoadPointType = splineRoadPoints[i].leftRoadPointType;
                splineRoadPoints[i].leftRoadPoint = tempRoadPoint;
                splineRoadPoints[i].leftRoadPointType = tempPointType;
                splineRoadPoints[i].splineValue = 1.0f - splineRoadPoints[i].splineValue;
                splineRoadPoints[i].roadSectionIndex = maxRoadSectionIndexValue - splineRoadPoints[i].roadSectionIndex;

            }
            splineRoadPoints.Reverse();
        }

        splines.Add(leftSpline);
        splines.Add(rightSpline);
        splines.Add(leftSpline);

        var emptyTilesOnRight = new List<List<EmptyTileData>>();
        var emptyTilesOnLeft = new List<List<EmptyTileData>>();

        var roadSectionIndexes = new HashSet<int>();
        var maxRoadSectionIndex = 0;
        for (int i = 0; i < splineRoadPoints.Count; i++)
        {

            //DrawRoadGuidingPoints(splineRoadPoints[i]);
            //leftSpline.Add(new BezierKnot(roadPointData.leftRoadPoint), TangentMode.AutoSmooth);
            //rightSpline.Add(new BezierKnot(roadPointData.rightRoadPoint), TangentMode.AutoSmooth);
            //middleSpline.Add(new BezierKnot(roadPointData.middleRoadPoint), TangentMode.AutoSmooth);

            if (i < splineRoadPoints.Count - 1)
            {
               
                if (splineRoadPoints[i].roadSectionIndex > maxRoadSectionIndex)
                {
                    maxRoadSectionIndex = splineRoadPoints[i].roadSectionIndex;
                }
                if(splineRoadPoints.Where(srp => srp.roadSectionIndex == 0).Count() > 4 && i < 3 && firstRoadNameAndClosestRoadPointData != default) {
                    
                    continue;
                }
                roadSectionIndexes.Add(splineRoadPoints[i].roadSectionIndex);
                emptyTilesOnRight.Add(GetEmptyTilesForRoad(splineRoadPoints[i], splineRoadPoints[i + 1], RoadSide.Right, splineRoadPoints[i].roadSectionIndex));
                emptyTilesOnLeft.Add(GetEmptyTilesForRoad(splineRoadPoints[i], splineRoadPoints[i + 1], RoadSide.Left, splineRoadPoints[i].roadSectionIndex));

                
                
                //(previousLeftForward, previousLeftRoadDirectionVector, previousLeftRoadMiddlePoint) = AddEmptyTiles(splineRoadPoints[i], splineRoadPoints[i + 1], RoadSide.Left, previousLeftForward, previousLeftRoadDirectionVector, previousLeftRoadMiddlePoint);
            }
        }

        if (roadSectionIndexes.Count <= maxRoadSectionIndex)
        {
            throw new Exception("hess");
        }

        var (forwardRightBatches, backwardRightBatches) = BatchEmptyTiles(emptyTilesOnRight, roadSectionIndexes.Count);
        var (forwardLeftBatches, backwardLeftBatches) = BatchEmptyTiles(emptyTilesOnLeft, roadSectionIndexes.Count);

        var batchesOnRight = GetBestBatching(forwardRightBatches, forwardRightBatches);
        var batchesOnLeft = GetBestBatching(forwardLeftBatches, backwardLeftBatches);


        return (splineRoadPoints, batchesOnRight, batchesOnLeft);

    }

    private List<List<BatchData>> RemoveCollidingBatchTiles(List<List<BatchData>> batchDatas)
    {

        LayerMask layerMask = LayerMask.GetMask("ShadowRealm");
        
        List<List<BatchData>> cleanedBatchDatas = new List<List<BatchData>>();
        for (int k = batchDatas.Count - 1; k >= 0; k--)
        {
            var batches = batchDatas[k];
            for (int j = batches.Count - 1; j >= 0; j--)
            {
                var batch = batches[j];
                foreach (var emptyTileDatas in batch.emptyTileDatas)
                {
                    var indiciesToRemove = new List<int>();
                    for (int i = emptyTileDatas.Count - 1; i >= 0; i--)
                    {
                        var emptyTileData = emptyTileDatas[i];
                        var result = new Collider[10];
                        int hitCount = Physics.OverlapBoxNonAlloc(emptyTileData.position, new Vector3(1.8f, 30.0f, 1.8f),result, emptyTileData.rotation, layerMask);
                        var shadowCollider = emptyTileData.gameObject.transform.Find("ShadowCopy").GetComponent<Collider>();
                        foreach(var collider in result)
                        {
                          if (collider == shadowCollider)
                          {
                            hitCount--;
                          }
                        }

                        if (hitCount > 0)
                        {
                            
                            indiciesToRemove.Add(i); 
                        }
                    }
                    
                    foreach (var index in indiciesToRemove)
                    {
                        emptyTileDatas[index].gameObject.SetActive(false);
                        emptyTileDatas.RemoveAt(index);
                    }
                }

                batches[j] = new BatchData
                {
                    batchIndex = batch.batchIndex,
                    emptyTileDatas = new List<List<EmptyTileData>>( batch.emptyTileDatas.Where(etd => etd.Count > 0).ToList()),
                    tileObjects = new List<GameObject>(batch.tileObjects.Where(to => to != null).ToList())
                };
            }
            batches = batches.Where(b => b.emptyTileDatas.Count > 0).ToList();
            batchDatas[k] = batches;
        }
        var returnList = batchDatas.Where(bd => bd.Count > 0).ToList();
        var ses = batchDatas.Where(bd => bd.Count(bdi => bdi.emptyTileDatas.Count > 0) > 0).ToList();
        return returnList;
    }
    
    private string StringifyList<T>(List<T> list)
    {
        var stringList = new List<string>();
        foreach (var item in list)
        {
            stringList.Add(item.ToString());
        }
        return string.Join(", ", stringList);
    }

    private void GetRoadPointAndMiddlePointForPointConnectingToOtherRoad(List<Vector3> points, float roadWidth, (string roadName, RoadPointData closestRoadPoint) roadNameAndClosesRoadPointData, bool isLastPoint, out RoadPointData lastRoadData, out Vector3 middleRoadPoint, out bool shouldSwitchSides)
    {
        shouldSwitchSides = false;
        points.RemoveAt(isLastPoint ? points.Count - 1 : 0);
        var roadData = roadPointDatasForRoads[roadNameAndClosesRoadPointData.roadName];
        var idx = roadData.roadPoints.IndexOf(roadNameAndClosesRoadPointData.closestRoadPoint);
        var closestRoadPointData = roadData.roadPoints[idx];
        var splinePointToCompare = points[isLastPoint ? points.Count - 1 : 0];
        var distanceToLeftPoint = (splinePointToCompare - roadNameAndClosesRoadPointData.closestRoadPoint.leftRoadPoint).magnitude;
        var distanceToRightPoint = (splinePointToCompare - roadNameAndClosesRoadPointData.closestRoadPoint.rightRoadPoint).magnitude;
        bool isLeftCloserToPoint = distanceToLeftPoint < distanceToRightPoint;
        Vector3 leftRoadPoint;
        Vector3 rightRoadPoint;
        if (idx != 0 && idx + 1 < roadData.roadPoints.Count)
        {
            var point1 = isLeftCloserToPoint ? closestRoadPointData.leftRoadPoint : closestRoadPointData.rightRoadPoint;
            var point2 = isLeftCloserToPoint ? roadData.roadPoints[idx + 1].leftRoadPoint : roadData.roadPoints[idx + 1].rightRoadPoint;
            leftRoadPoint = isLeftCloserToPoint ? point2 : point1;
            rightRoadPoint = isLeftCloserToPoint ? point1 : point2;
            if (!isLastPoint)
            {
                var temp = rightRoadPoint;
                rightRoadPoint = leftRoadPoint;
                leftRoadPoint = temp;
            }
            middleRoadPoint = point1 + (point2 - point1) / 2;
        }
        else
        {
            if (idx == 0 && !isLastPoint)
            {
                shouldSwitchSides = true;
            }
            else if (idx == roadData.roadPoints.Count - 1 && isLastPoint)
            {
                shouldSwitchSides = true;
            }
            leftRoadPoint = closestRoadPointData.leftRoadPoint;
            middleRoadPoint = closestRoadPointData.middleRoadPoint;
            rightRoadPoint = closestRoadPointData.rightRoadPoint;
            if (shouldSwitchSides)
            {
                var temp = rightRoadPoint;
                rightRoadPoint = leftRoadPoint;
                leftRoadPoint = temp;
            }

        }

        
        lastRoadData = new RoadPointData
        {
            leftRoadPoint = leftRoadPoint,
            rightRoadPoint = rightRoadPoint,
            middleRoadPoint = middleRoadPoint,
            roadWidth = roadWidth,
            leftRoadPointType = closestRoadPointData.leftRoadPointType,
            rightRoadPointType = closestRoadPointData.rightRoadPointType,
            middleRoadPointType = closestRoadPointData.middleRoadPointType,
            splineValue = isLastPoint ? 1.0f : 0.0f,
            roadSectionIndex = isLastPoint ? points.Count : 0
        };
    }

    IEnumerator DrawBathces(List<List<BatchData>> listOfBatchDatas, List<GameObject> emptyTileList, GameObject roadMesh, string side)
    {
        var colors = new List<Color> { Color.red, Color.blue, Color.black, Color.white, Color.grey, Color.green, Color.yellow };
        Unity.Mathematics.Random random = new Unity.Mathematics.Random((uint)DateTime.Now.Millisecond + 1);
        foreach (var batchDatas in listOfBatchDatas)
        {

            for (int i = 0; i < batchDatas.Count; i++)
            {
                var batch = batchDatas[i];
                
                foreach (var emptyTileDatas in batch.emptyTileDatas)
                {
                    SetPositionOfEmptyTiles(emptyTileDatas);
                    batch = AddTilesToGame(emptyTileList, roadMesh, batch, emptyTileDatas);
                    //yield return new WaitForSeconds(0.05f);
                }
            }
        }
        yield return null;
        listOfBatchDatas = RemoveCollidingBatchTiles(listOfBatchDatas);
        foreach (Transform child in roadMesh.transform)
        {
            if (child.gameObject.tag == side)
            {
                Destroy(child.gameObject);
            }
        }
        yield return null;
        foreach (var batchDatas in listOfBatchDatas)
        {
             for (int i = 0; i < batchDatas.Count; i++)
            {
                var batch = batchDatas[i];
                Color color = colors.ElementAt(random.NextInt(0, colors.Count));
                foreach (var emptyTileDatas in batch.emptyTileDatas)
                {
                    batch = AddTilesToGame(emptyTileList, roadMesh, batch, emptyTileDatas, "right", color);
                    //yield return new WaitForSeconds(0.2f);
                }
                yield return null;
            }
        }
    }

    private BatchData AddTilesToGame(List<GameObject> emptyTileList, GameObject roadMesh, BatchData batch, List<EmptyTileData> emptyTileDatas, string side = "right", Color color = default)
    {
        foreach (var emptyTileData in emptyTileDatas)
        {

            var emptyTileInstance = Instantiate(emptyTilePrefab, roadMesh.transform);
            //emptyTileInstance.par
            emptyTileInstance.transform.position = emptyTileData.position;
            emptyTileInstance.transform.rotation = emptyTileData.rotation;
            emptyTileInstance.gameObject.tag = side;
            // var shadowCopy = emptyTileInstance.gameObject.transform.Find("ShadowCopy");
            // if (shadowCopy != null)
            // {
            //     shadowCopy.transform.position = new Vector3(emptyTileData.position.x, 115.0f, emptyTileData.position.z);
            // }
            emptyTileInstance.GetComponentInChildren<Highlight>().SetHighlightColor(color);
            emptyTileInstance.GetComponentInChildren<Highlight>().ToggleHighlight(true);
            if (batch.tileObjects == default)
            {
                batch.tileObjects = new List<GameObject>();
            }
            batch.tileObjects.Add(emptyTileInstance);
            emptyTileData.gameObject = emptyTileInstance;
            emptyTileList.Add(emptyTileInstance);
        }

        return batch;
    }

    List<List<BatchData>> GetBestBatching(List<List<BatchData>> forwardBatches, List<List<BatchData>> backwardBatches)
    {
        List<List<BatchData>> batchData = new List<List<BatchData>>();
        var batchIndex = 0;
        for (int i = 0; i < forwardBatches.Count; i++)
        {
            var batchToTake = forwardBatches[i].Count < backwardBatches[i].Count ? forwardBatches[i] : backwardBatches[i];

            for (int j = 0; j < batchToTake.Count; j++)
            {
                batchToTake[j] = new BatchData
                {
                    emptyTileDatas = batchToTake[j].emptyTileDatas,
                    batchIndex = batchIndex++,
                    tileObjects = new List<GameObject>()
                };
            }

            batchData.Add(batchToTake.Where(b => b.emptyTileDatas.Count > 1).ToList());
        }
        return batchData;
    }

    private List<EmptyTileData> GetEmptyTilesForRoad(RoadPointData roadPointData1, RoadPointData roadPointData2, RoadSide side, int roadSectionIndex)
    {
        var emptyTiles = new List<EmptyTileData>();
        var point1 = side == RoadSide.Right ? roadPointData1.rightRoadPoint : roadPointData1.leftRoadPoint;
        var point2 = side == RoadSide.Right ? roadPointData2.rightRoadPoint : roadPointData2.leftRoadPoint;
        var roadDirectionVector = point2 - point1;
        var forward = Vector3.Cross(roadDirectionVector, side == RoadSide.Right ? Vector3.down : Vector3.up).normalized;


        var roadHalfVector = roadDirectionVector / 2;
        var roadHalfPoint = point1 + roadHalfVector;

        for (int i = 0; i < tilesToPlace; i++)
        {
            emptyTiles.Add(new EmptyTileData
            {
                perpendicularToRoadVector = forward,
                roadDirectionVector = roadDirectionVector,
                roadMiddlePoint = roadHalfPoint,
                roadSectionIndex = roadSectionIndex,
                roadStartPoint = point1,
                closestRoadPointData = roadPointData1
            });

        }

        return emptyTiles;

    }

    void SetPositionOfEmptyTiles(List<EmptyTileData> emptyTileDatas)
    {
        for (int i = 0; i < emptyTileDatas.Count; i++)
        {
            var roadMiddlePoint = emptyTileDatas[i].roadMiddlePoint;
            var forward = emptyTileDatas[i].perpendicularToRoadVector;
            var position = roadMiddlePoint + (forward * emptyTileOffset) + (forward * i * emptyTileGutter);
            position.y = position.y + 0.1f;
            var rotation = Quaternion.LookRotation(forward);

            emptyTileDatas[i].position = position;
            emptyTileDatas[i].rotation = rotation;

        }
    }

    private (List<List<BatchData>>, List<List<BatchData>>) BatchEmptyTiles(List<List<EmptyTileData>> emptyTiles, int numberOfSections)
    {
        List<BatchData>[] forwardBatches = new List<BatchData>[numberOfSections];
        forwardBatches[0] = new List<BatchData>() { new BatchData { emptyTileDatas = new List<List<EmptyTileData>> { emptyTiles[0] }, batchIndex = 0, tileObjects = new List<GameObject>() } };
        List<BatchData>[] backwardBatches = new List<BatchData>[numberOfSections];
        backwardBatches[numberOfSections - 1] = new List<BatchData>() { new BatchData { emptyTileDatas = new List<List<EmptyTileData>> { emptyTiles[emptyTiles.Count - 1] }, batchIndex = 0, tileObjects = new List<GameObject>() } };
        if (emptyTiles.Count < 2)
        {
            return (forwardBatches.ToList(), backwardBatches.ToList());
        }

        var sectionIndex = emptyTiles[0][0].roadSectionIndex;
        var batchIndex = 0;
        for (int i = 1; i < emptyTiles.Count; i++)
        {
            var previousRoadTiles = emptyTiles[i - 1];
            var currentToadTiles = emptyTiles[i];
            (batchIndex, sectionIndex) = AddBatchData(previousRoadTiles, currentToadTiles, forwardBatches, batchIndex, sectionIndex);
        }

        sectionIndex = emptyTiles[emptyTiles.Count - 1][0].roadSectionIndex; ;
        batchIndex = 0;
        for (int i = emptyTiles.Count - 2; i >= 0; i--)
        {
            var previousRoadTiles = emptyTiles[i + 1];
            var currentToadTiles = emptyTiles[i];
            (batchIndex, sectionIndex) = AddBatchData(previousRoadTiles, currentToadTiles, backwardBatches, batchIndex, sectionIndex);
        }

        if (forwardBatches.ToList().Where(f => f == default).ToList().Count != 0)
        {
            throw new Exception("baj van");
        }

        return (forwardBatches.ToList(), backwardBatches.ToList());
    }

    (int, int) AddBatchData(List<EmptyTileData> previousEmptyTiles, List<EmptyTileData> currentEmptyTiles, List<BatchData>[] batches, int batchIndex, int sectionIndex)
    {

        var perpendicularToRoadVector = currentEmptyTiles[0].perpendicularToRoadVector;
        var previousPerpendicularToRoadVector = previousEmptyTiles[0].perpendicularToRoadVector;

        var currentSectionIndex = currentEmptyTiles[0].roadSectionIndex;
        var previousSectionIndex = previousEmptyTiles[0].roadSectionIndex;

        if (previousSectionIndex != currentSectionIndex)
        {
            batchIndex++;
            batches[currentSectionIndex] = new List<BatchData>() { new BatchData { emptyTileDatas = new List<List<EmptyTileData>> { currentEmptyTiles }, batchIndex = batchIndex, tileObjects = new List<GameObject>() } };
            return (batchIndex, currentSectionIndex);
        }
        var angle = Vector3.Angle(previousPerpendicularToRoadVector, perpendicularToRoadVector);

        if (angle < maximumAngleDifferenceForBatch)
        {
            if (angle > minimumAngleToChangeVectorsInBatch)
            {
                var previousRoadDirectionVector = previousEmptyTiles[0].roadDirectionVector;
                var previousRoadMiddlePoint = previousEmptyTiles[0].roadMiddlePoint;
                for (int j = 0; j < currentEmptyTiles.Count; j++)
                {
                    currentEmptyTiles[j].perpendicularToRoadVector = previousPerpendicularToRoadVector;
                    currentEmptyTiles[j].roadDirectionVector = previousRoadDirectionVector;
                    currentEmptyTiles[j].roadMiddlePoint = previousRoadMiddlePoint + previousRoadDirectionVector;
                }

            }
            batches[currentSectionIndex][batches[sectionIndex].Count - 1].emptyTileDatas.Add(currentEmptyTiles);
            return (batchIndex, currentSectionIndex);
        }
        else
        {
            batchIndex++;
            if (batches[currentSectionIndex] == default)
            {
                batches[currentSectionIndex] = new List<BatchData>();
            }
            batches[currentSectionIndex].Add(new BatchData { emptyTileDatas = new List<List<EmptyTileData>> { currentEmptyTiles }, batchIndex = batchIndex, tileObjects = new List<GameObject>() });
            return (batchIndex, currentSectionIndex);
        }


    }

    private void DrawRoadGuidingPoints(RoadPointData roadPointData)
    {
        var leftPointInstance = Instantiate(point);
        var rightPointInstance = Instantiate(point);
        var middlePointInstance = Instantiate(point);
        leftPointInstance.transform.position = roadPointData.leftRoadPoint;
        rightPointInstance.transform.position = roadPointData.rightRoadPoint;
        middlePointInstance.transform.position = roadPointData.middleRoadPoint;
        var leftHighlight = leftPointInstance.GetComponent<Highlight>();
        var rightHighlight = rightPointInstance.GetComponent<Highlight>();
        var middleHighlight = middlePointInstance.GetComponent<Highlight>();
        SetHighlightColor(roadPointData.leftRoadPointType, leftHighlight);
        SetHighlightColor(roadPointData.rightRoadPointType, rightHighlight);
        SetHighlightColor(roadPointData.middleRoadPointType, middleHighlight);
        leftHighlight.ToggleHighlight(true);
        rightHighlight.ToggleHighlight(true);
        middleHighlight.ToggleHighlight(true);
        splinePointInstances.Add(rightPointInstance);
        splinePointInstances.Add(leftPointInstance);
        splinePointInstances.Add(middlePointInstance);

    }

    private void SetHighlightColor(SplinePointType pointType, Highlight highlight)
    {
        switch (pointType)
        {
            case SplinePointType.FollowMeshCurve:
                highlight.SetHighlightColor(Color.gray);
                break;
            case SplinePointType.LevelTerrain:
                highlight.SetHighlightColor(Color.magenta);
                break;
            case SplinePointType.Bridge:
                highlight.SetHighlightColor(Color.white);
                break;
            case SplinePointType.Tunnel:
                highlight.SetHighlightColor(Color.yellow);
                break;
        }
    }

    private void GetSplineForTerrainBetweenPoints(List<Vector3> splinePoints, out List<RoadPointData> splineRoadPoints, float roadWidth)
    {
        Spline tempSpline = splineContainer.AddSpline();
        Spline splineMiddle = splineContainer.AddSpline();


        var splinePointDatas = new List<SplinePointData>();
        splineRoadPoints = new List<RoadPointData>();
        foreach (var point in splinePoints)
        {
            tempSpline.Add(new BezierKnot(point), TangentMode.AutoSmooth);
        }
        var guidingPointSplineValues = new List<float>();
        for (int i = 0; i < splinePoints.Count; i++)
        {
            SplineUtility.GetNearestPoint(tempSpline, splinePoints[i], out float3 nearestPoint, out float splineValue);
            if (i == 0)
            {
                guidingPointSplineValues.Add(0.0f);
                continue;
            } 
            if (i == splinePoints.Count -1 ) {
                guidingPointSplineValues.Add(1.0f);
                continue;
            }
            if (splineValue - guidingPointSplineValues[guidingPointSplineValues.Count - 1] > 0.01)
            {
                guidingPointSplineValues.Add(splineValue);
            }

        }


        splinePointDatas.Add(GetSplinePointDataForSplinePoint(splinePoints[0], 0.0f));
        splineMiddle.Add(new BezierKnot(splinePoints[0]), TangentMode.AutoSmooth);
        float step = 1.0f / (tempSpline.GetLength() / resolution);
        var sectionIndex = 0;
        for (float i = step; i < 1.0f; i += step)
        {

            SplineUtility.Evaluate(tempSpline, i, out float3 splineEvalResult, out float3 forward, out float3 upVector);
            if (splineEvalResult.y < 0)
            {
                splineEvalResult.y = splinePointDatas[splinePointDatas.Count - 1].splinePoint.y;
            }

            for (int j = guidingPointSplineValues.Count - 1; j >= 0; j--)
            {
                if (i > guidingPointSplineValues[j])
                {
                    sectionIndex = j;
                    break;
                }
            }

            var pointOnCurve = new Vector3(splineEvalResult.x, splineEvalResult.y + heightDelta < 0 ? heightDelta : splineEvalResult.y + heightDelta, splineEvalResult.z);

            var splinePointData = GetSplinePointDataForSplinePoint(pointOnCurve, i);
            splinePointData.sectionIndex = sectionIndex;
            splinePointDatas.Add(splinePointData);
            splineMiddle.Add(new BezierKnot(splineEvalResult), TangentMode.AutoSmooth);
        }
        splinePointDatas.Add(GetSplinePointDataForSplinePoint(splinePoints[splinePoints.Count - 1], 1.0f));
        splineMiddle.Add(new BezierKnot(splinePoints[splinePoints.Count - 1]), TangentMode.AutoSmooth);

        for (int i = 0; i < splinePointDatas.Count; i++)
        {
            var splinePointData = splinePointDatas[i];
            SplinePointData nextSplinePointData = i < splinePointDatas.Count - 1 ? splinePointDatas[i + 1] : default;

            SplineUtility.Evaluate(splineMiddle, splinePointData.splineValue, out float3 splineEvalResult, out float3 forward, out float3 upVector);
            Vector3 forwardVector = nextSplinePointData.Equals(default(SplinePointData)) ? splinePointDatas[i - 1].splinePoint - splinePointData.splinePoint : splinePointData.splinePoint - nextSplinePointData.splinePoint;
            splineRoadPoints.Add(GetRoadPointDataForPoint(splinePointData, splinePointData.splineValue, forwardVector, Vector3.up, splinePointData.sectionIndex, roadWidth));

        }

        splineContainer.RemoveSpline(tempSpline);
        splineContainer.RemoveSpline(splineMiddle);
    }

    SplinePointData GetSplinePointDataForSplinePoint(Vector3 pointOnCurve, float splineValue)
    {
        Ray rayDowm = new Ray(pointOnCurve, Vector3.down);

        RaycastHit rayHit;
        var res = Physics.Raycast(rayDowm, out rayHit, raycastDistance, raycastLayerMask);
        return GetSplinePointDataForHit(rayHit, pointOnCurve, splineValue);
    }

    RoadPointData GetRoadPointDataForPoint(SplinePointData roadMiddlePointData, float splineValue, float3 forward, float3 upVector, int roadSectionIndex, float roadWidth)
    {
        var roadMiddlePoint = roadMiddlePointData.splinePoint;
        var roadMiddlePointCapped = new Vector3(roadMiddlePoint.x, roadMiddlePoint.y + heightDelta < 0 ? heightDelta : roadMiddlePoint.y + heightDelta, roadMiddlePoint.z);
        var roadRight = Vector3.Cross(forward, upVector).normalized;

        var rayCastPoint1 = roadMiddlePointCapped + (roadRight * this.roadWidth);
        var rayCastPoint2 = roadMiddlePointCapped - (roadRight * this.roadWidth);

        Ray ray1Down = new Ray(rayCastPoint1, Vector3.down);
        Ray ray2Down = new Ray(rayCastPoint2, Vector3.down);

        RaycastHit hit1Down;
        RaycastHit hit2Down;
        Physics.Raycast(ray1Down, out hit1Down, raycastDistance, raycastLayerMask);
        Physics.Raycast(ray2Down, out hit2Down, raycastDistance, raycastLayerMask);
        var rightPoint = GetSplinePointDataForHit(hit1Down, rayCastPoint1, splineValue);
        var leftPoint = GetSplinePointDataForHit(hit2Down, rayCastPoint2, splineValue);
        return new RoadPointData
        {
            leftRoadPoint = leftPoint.hitOnMesh.Equals(default) ? leftPoint.splinePoint : leftPoint.hitOnMesh,
            leftRoadPointType = leftPoint.splinePointType,
            rightRoadPoint = rightPoint.hitOnMesh.Equals(default) ? rightPoint.splinePoint : rightPoint.hitOnMesh,
            rightRoadPointType = rightPoint.splinePointType,
            middleRoadPoint = roadMiddlePointData.hitOnMesh.Equals(default) ? roadMiddlePointData.splinePoint : roadMiddlePointData.hitOnMesh,
            middleRoadPointType = roadMiddlePointData.splinePointType,
            splineValue = splineValue,
            roadSectionIndex = roadSectionIndex,
            roadWidth = roadWidth
        };
    }


    SplinePointData GetSplinePointDataForHit(RaycastHit hit, Vector3 pointOnCurve, float splineValue)
    {

        if (!hit.Equals(default(RaycastHit)) && hit.collider.GetComponent<TimeManager>())
        {
            var type = SplinePointType.FollowMeshCurve;
            //var distanceBetweenMeshPointAndStraightLinePoint = pointOnCurve.y - hit.point.y;
            // if (distanceBetweenMeshPointAndStraightLinePoint > bridgeDistanceDelta)
            // {
            //     type = SplinePointType.Bridge;
            // }
            // else if (distanceBetweenMeshPointAndStraightLinePoint > levelTerrainDistanceDelta)
            // {
            //     type = SplinePointType.LevelTerrain;
            // }

            return new SplinePointData
            {
                splinePoint = pointOnCurve,
                hitOnMesh = hit.point,
                splinePointType = type,
                splineValue = splineValue

            };

        }
        else
        {

            return new SplinePointData
            {
                splinePoint = pointOnCurve,
                hitOnMesh = default,
                splinePointType = SplinePointType.Tunnel,
                splineValue = splineValue
            };
        }
    }
}

struct SplinePointData
{
    public Vector3 splinePoint;
    public Vector3 hitOnMesh;
    public SplinePointType splinePointType;
    public float splineValue;
    public int sectionIndex;
}