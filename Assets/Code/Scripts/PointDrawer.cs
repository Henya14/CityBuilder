using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

public enum SplinePointType
{
    FollowMeshCurve,
    LevelTerrain,
    Bridge,
    Tunnel
}

public struct RoadPointData
{
    public Vector3 leftRoadPoint;
    public Vector3 rightRoadPoint;
    public Vector3 middleRoadPoint;
    public SplinePointType leftRoadPointType;
    public SplinePointType rightRoadPointType;
    public SplinePointType middleRoadPointType;
    public float splineValue;
}

public class PointDrawer : MonoBehaviour
{
    public Material roadNaterial;
    public Camera playerCamera;
    public GameObject point;

    private GameObject pointInstance;
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
    public bool autoUpdateEnabled = true;
    public float pointClosenessDelta = 0.0001f;
    private int raycastLayerMask;
    public float raycastDistance = 20.0f;
    List<RoadPointData> splineRoadPoints = new List<RoadPointData>();

    private int roadIndex = 0;
    bool roadShit = true;
    private List<GameObject> roadMeshes = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        splineContainer = gameObject.AddComponent<SplineContainer>();
        raycastLayerMask = LayerMask.GetMask("Ground");

    }

    // Update is called once per frame
    void Update()
    {
        LastMousePosition = Input.mousePosition;
        Ray ray = playerCamera.ScreenPointToRay(LastMousePosition);

        if (roadShit && Physics.Raycast(ray, out RaycastHit rayHit))
        {
            if (rayHit.collider.gameObject.GetComponent<TimeManager>() != null)
            {
                Destroy(pointInstance);
                var tempPoint = Instantiate(point);
                tempPoint.transform.position = rayHit.point;
                pointInstance = tempPoint;
                splineGuidingPointInstances.Add(pointInstance);
                if (splineGuidingPointInstances.Count > 1)
                {
                    DrawRoadCurve();
                }
                splineGuidingPointInstances.RemoveAt(splineGuidingPointInstances.Count - 1);
            }
        }
        if (roadShit && Input.GetMouseButtonDown(0) )
        {
            splineGuidingPointInstances.Add(pointInstance);
            pointInstance = null;
        }
        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log("Nyasgem");
            
            //DrawRoadMesh();
            roadShit = !roadShit;
            splineGuidingPointInstances.RemoveAt(splineGuidingPointInstances.Count - 1);
            DrawRoadCurve();
        }

    }

    private void DrawRoadMesh()
    {
        var road = new GameObject($"road {++roadIndex}");
        roadMeshes.Add(road);
        var mesh = road.AddComponent<RoadMesh>();

        mesh.DrawMesh(MeshGenerator.GenerateRoadMesh(splineRoadPoints), roadNaterial);
        
    }

    private void RemoveSplinePoints()
    {
        splinePointInstances.ForEach(mp => Destroy(mp));
        splinePointInstances.Clear();
    }

    public void DrawRoadCurve()
    {
        RemoveSplinePoints();
        CreateCurveBetweenPoints(splineGuidingPointInstances.Select(sp => sp.transform.position).ToList());
    }


    public void CreateCurveBetweenPoints(List<Vector3> points)
    {
        if (points.Count < 2)
        {
            return;
        }
        for (int i = 0; i < splines.Count; i++)
        {
            splineContainer.RemoveSplineAt(i);
        }
        splines.Clear();
        Spline leftSpline = splineContainer.AddSpline();
        Spline rightSpline = splineContainer.AddSpline();
        Spline middleSpline = splineContainer.AddSpline();
        splineRoadPoints.Clear();
        GetSplineForTerrainBetweenPoints(points, out splineRoadPoints);
        roadMeshes.ForEach(rm => Destroy(rm));
        roadMeshes.Clear();
        DrawRoadMesh();
        splines.Add(leftSpline);
        splines.Add(rightSpline);
        splines.Add(leftSpline);
        for (int i = 0; i < splineRoadPoints.Count; i++)
        {

            DrawRoadGuidingPoints(splineRoadPoints[i]);
            //leftSpline.Add(new BezierKnot(roadPointData.leftRoadPoint), TangentMode.AutoSmooth);
            //rightSpline.Add(new BezierKnot(roadPointData.rightRoadPoint), TangentMode.AutoSmooth);
            //middleSpline.Add(new BezierKnot(roadPointData.middleRoadPoint), TangentMode.AutoSmooth);

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

    private void GetSplineForTerrainBetweenPoints(List<Vector3> splinePoints, out List<RoadPointData> splineRoadPoints)
    {
        Spline tempSpline = splineContainer.AddSpline();
        Spline splineMiddle = splineContainer.AddSpline();


        var splinePointDatas = new List<SplinePointData>();
        splineRoadPoints = new List<RoadPointData>();
        foreach (var point in splinePoints)
        {
            tempSpline.Add(new BezierKnot(point), TangentMode.AutoSmooth);
        }

        splinePointDatas.Add(GetSplinePointDataForSplinePoint(splinePoints[0], 0.0f));
        splineMiddle.Add(new BezierKnot(splinePoints[0]), TangentMode.AutoSmooth);
        float step = 1.0f / (tempSpline.GetLength() / resolution);

        for (float i = step; i < 1.0f; i += step)
        {

            SplineUtility.Evaluate(tempSpline, i, out float3 splineEvalResult, out float3 forward, out float3 upVector);
            if (splineEvalResult.y < 0) {
                splineEvalResult.y = splinePointDatas[splinePointDatas.Count -1].splinePoint.y;
            }
            
            var pointOnCurve = new Vector3(splineEvalResult.x, splineEvalResult.y + heightDelta < 0 ? heightDelta : splineEvalResult.y + heightDelta, splineEvalResult.z);

            var splinePointData = GetSplinePointDataForSplinePoint(pointOnCurve, i);
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
            Vector3 forwardVector = nextSplinePointData.Equals(default(SplinePointData)) ? forward : splinePointData.splinePoint - nextSplinePointData.splinePoint;
            splineRoadPoints.Add(GetRoadPointDataForPoint(splinePointData, splinePointData.splineValue, forwardVector, Vector3.up));

        }

        splineContainer.RemoveSpline(tempSpline);
        splineContainer.RemoveSpline(splineMiddle);
    }

    SplinePointData GetSplinePointDataForSplinePoint(Vector3 pointOnCurve, float splineValue)
    {
        Ray rayDowm = new Ray(pointOnCurve, Vector3.down);

        RaycastHit rayHit;
        var res = Physics.Raycast(rayDowm, out rayHit, raycastDistance, raycastLayerMask);
        if (res == false)
        {
            Debug.Log("Ayuda");
        }
        return GetSplinePointDataForHit(rayHit, pointOnCurve, splineValue);
    }

    RoadPointData GetRoadPointDataForPoint(SplinePointData roadMiddlePointData, float splineValue, float3 forward, float3 upVector)
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
        Debug.Log($"{roadMiddlePoint} ${hit1Down.point}");
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
            splineValue = splineValue
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
}