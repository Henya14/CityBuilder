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
    // Start is called before the first frame update
    void Start()
    {
        splineContainer = gameObject.AddComponent<SplineContainer>();
    }

    // Update is called once per frame
    void Update()
    {
        LastMousePosition = Input.mousePosition;
        Ray ray = playerCamera.ScreenPointToRay(LastMousePosition);

        if (Physics.Raycast(ray, out RaycastHit rayHit))
        {
            if (rayHit.collider.gameObject.GetComponent<Highlight>() != null)
            {
                Destroy(pointInstance);
                var tempPoint = Instantiate(point);
                tempPoint.transform.position = rayHit.point;
                pointInstance = tempPoint;
                splineGuidingPointInstances.Add(pointInstance);
                if (splineGuidingPointInstances.Count > 1)
                {
                    RemoveSplinePoints();
                    CreateCurveBetweenPoints(splineGuidingPointInstances.Select(sp => sp.transform.position).ToList());
                }
                splineGuidingPointInstances.RemoveAt(splineGuidingPointInstances.Count - 1);
            }
        }
        if (Input.GetMouseButtonDown(0))
        {
            splineGuidingPointInstances.Add(pointInstance);
            pointInstance = null;
        }

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
        for (int i = 0; i < splines.Count; i++) {
            splineContainer.RemoveSplineAt(i);
        }
        splines.Clear();
        Spline leftSpline = splineContainer.AddSpline();
        Spline rightSpline = splineContainer.AddSpline();
        Spline middleSpline = splineContainer.AddSpline();
        List<RoadPointData> splineRoadPoints;
        GetSplineForTerrainBetweenPoints(points, out splineRoadPoints);
        splines.Add(leftSpline);
        splines.Add(rightSpline);
        splines.Add(leftSpline);
        foreach (RoadPointData roadPointData in splineRoadPoints)
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
            leftSpline.Add(new BezierKnot(roadPointData.leftRoadPoint), TangentMode.AutoSmooth);
            rightSpline.Add(new BezierKnot(roadPointData.rightRoadPoint), TangentMode.AutoSmooth);
            middleSpline.Add(new BezierKnot(roadPointData.middleRoadPoint), TangentMode.AutoSmooth);
            leftHighlight.ToggleHighlight(true);
            rightHighlight.ToggleHighlight(true);
            middleHighlight.ToggleHighlight(true);
            splinePointInstances.Add(rightPointInstance);
            splinePointInstances.Add(leftPointInstance);
            splinePointInstances.Add(middlePointInstance);
        }

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
            tempSpline.Add(new BezierKnot(point), TangentMode.Broken);
        }

        splinePointDatas.Add(GetSplinePointDataForSplinePoint(splinePoints[0], 0.0f));
        splineMiddle.Add(new BezierKnot(splinePoints[0]), TangentMode.AutoSmooth);
        float step = 1.0f / (tempSpline.GetLength() / resolution);

        for (float i = step; i < 1.0f; i += step)
        {

            SplineUtility.Evaluate(tempSpline, i, out float3 splineEvalResult, out float3 forward, out float3 upVector);
            var pointOnCurve = new Vector3(splineEvalResult.x, splineEvalResult.y, splineEvalResult.z);
            
            var splinePointData = GetSplinePointDataForSplinePoint(pointOnCurve, i);
            splinePointDatas.Add(splinePointData);
            splineMiddle.Add(new BezierKnot(splinePointData.splinePoint), TangentMode.AutoSmooth);
        }
        splinePointDatas.Add(GetSplinePointDataForSplinePoint(splinePoints[splinePoints.Count - 1], 1.0f));
        splineMiddle.Add(new BezierKnot(splinePoints[splinePoints.Count - 1]), TangentMode.AutoSmooth);

        for(int i = 0; i < splinePointDatas.Count; i++) {
            var splinePointData = splinePointDatas[i];
            SplinePointData nextSplinePointData = i < splinePointDatas.Count - 1? splinePointDatas[i+1] : default;

            SplineUtility.Evaluate(splineMiddle, splinePointData.splineValue, out float3 splineEvalResult, out float3 forward, out float3 upVector);
            Vector3 forwardVector = nextSplinePointData.Equals(default(SplinePointData))? forward : splinePointData.splinePoint - nextSplinePointData.splinePoint;
            splineRoadPoints.Add(GetRoadPointDataForPoint(splinePointData.splinePoint, splinePointData.splineValue, forwardVector, Vector3.up));
        }

        splineContainer.RemoveSpline(tempSpline);
        splineContainer.RemoveSpline(splineMiddle);
    }

    SplinePointData GetSplinePointDataForSplinePoint(Vector3 pointOnCurve, float splineValue) {
        var clippedPointOnCurve = new Vector3(pointOnCurve.x, pointOnCurve.y + heightDelta < heightDelta ? heightDelta : pointOnCurve.y + heightDelta, pointOnCurve.z);
        Ray rayDowm = new Ray(clippedPointOnCurve, Vector3.down);

        RaycastHit rayHit;
        Physics.Raycast(rayDowm, out rayHit);
        return GetSplinePointDataForHit(rayHit, clippedPointOnCurve, splineValue);
    }

    RoadPointData GetRoadPointDataForPoint(Vector3 roadMiddlePoint, float splineValue, float3 forward, float3 upVector)
    {
        var roadRight = Vector3.Cross(forward, upVector).normalized;

        var rayCastPoint1 = roadMiddlePoint + (roadRight * this.roadWidth);
        var rayCastPoint2 = roadMiddlePoint - (roadRight * this.roadWidth);

        Ray ray1Down = new Ray(rayCastPoint1, Vector3.down);
        Ray ray2Down = new Ray(rayCastPoint2, Vector3.down);

        RaycastHit hit1Down;
        RaycastHit hit2Down;
        Physics.Raycast(ray1Down, out hit1Down);
        Physics.Raycast(ray2Down, out hit2Down);
        Debug.Log(hit1Down);
        var rightPoint = GetSplinePointDataForHit(hit1Down, rayCastPoint1, splineValue);
        var leftPoint = GetSplinePointDataForHit(hit2Down, rayCastPoint2, splineValue);
        return new RoadPointData
        {
            leftRoadPoint = leftPoint.splinePoint,
            leftRoadPointType = leftPoint.splinePointType,
            rightRoadPoint = rightPoint.splinePoint,
            rightRoadPointType = rightPoint.splinePointType,
            middleRoadPoint = roadMiddlePoint,
            splineValue = splineValue
        };


    }


    SplinePointData GetSplinePointDataForHit(RaycastHit hit, Vector3 rayCastPoint, float splineValue)
    {

        if (!hit.Equals(default(RaycastHit)) && hit.collider.GetComponent<Highlight>())
        {
            var type = SplinePointType.FollowMeshCurve;
            var distanceBetweenMeshPointAndStraightLinePoint = rayCastPoint.y - hit.point.y;
            if (distanceBetweenMeshPointAndStraightLinePoint > bridgeDistanceDelta)
            {
                type = SplinePointType.Bridge;
            }
            else if (distanceBetweenMeshPointAndStraightLinePoint > levelTerrainDistanceDelta)
            {
                type = SplinePointType.LevelTerrain;
            }

            return new SplinePointData
            {
                splinePoint = type == SplinePointType.FollowMeshCurve ? hit.point : rayCastPoint,
                hitOnMesh = hit.point,
                splinePointType = type,

            };

        }
        else
        {

            return new SplinePointData
            {
                splinePoint = rayCastPoint,
                hitOnMesh = Vector3.zero,
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