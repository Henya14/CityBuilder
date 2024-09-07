using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

public enum PointType
{
    FollowMeshCurve,
    LevelTerrain,
    Bridge,
    Tunnel
}

public struct RoadPointData
{
    public BezierKnot roadPoint;
    public PointType roadPointType;
}

public class PointDrawer : MonoBehaviour
{
    public Camera playerCamera;
    public GameObject point;
    private GameObject pointInstance;
    private List<GameObject> splinePointInstances = new List<GameObject>();
    private List<GameObject> splineMiddlePointInstances = new List<GameObject>();
    private List<Spline> splines = new List<Spline>();
    Vector3 LastMousePosition = Vector3.zero;
    SplineContainer splineContainer;
    public float samplingDistance = 1.0f;
    public float heightDelta = 0.3f;
    public float levelTerrainDistanceDelta = 0.6f;
    public float bridgeDistanceDelta = 0.8f;
    public int resolution = 100;
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
                splinePointInstances.Add(pointInstance);
                if (splinePointInstances.Count > 1)
                {
                    splineMiddlePointInstances.ForEach(mp => Destroy(mp));
                    splineMiddlePointInstances.Clear();
                    CreateCurveBetweenPoints(splinePointInstances.Select(sp => sp.transform.position).ToList());
                }
                splinePointInstances.RemoveAt(splinePointInstances.Count - 1);
            }
        }
        if (Input.GetMouseButtonDown(0))
        {
            splinePointInstances.Add(pointInstance);
            pointInstance = null;
        }

    }

    void CreateCurveBetweenPoints(List<Vector3> points)
    {
        splines.ForEach(s =>
        {
            splineContainer.RemoveSpline(s);
        });
        var spline = splineContainer.AddSpline();
        splines.Add(spline);
        List<RoadPointData> splineRoadPoints;
        List<RoadPointData> splineMiddleRoadPoints;
        GetSplineForTerrainBetweenPoints(points, out splineRoadPoints, out splineMiddleRoadPoints);
        foreach (RoadPointData roadPointData in splineMiddleRoadPoints)
        {
            spline.Add(roadPointData.roadPoint, TangentMode.AutoSmooth);
            var pointInstance = Instantiate(point);
            pointInstance.transform.position = roadPointData.roadPoint.Position;
            var highlight = pointInstance.GetComponent<Highlight>();
            switch (roadPointData.roadPointType)
            {
                case PointType.FollowMeshCurve:
                    highlight.SetHighlightColor(Color.gray);
                    break;
                case PointType.LevelTerrain:
                    highlight.SetHighlightColor(Color.magenta);
                    break;
                case PointType.Bridge:
                    highlight.SetHighlightColor(Color.white);
                    break;
                case PointType.Tunnel:
                    highlight.SetHighlightColor(Color.yellow);
                    break;
            }
            highlight.ToggleHighlight(true);
            splineMiddlePointInstances.Add(pointInstance);
        }

    }

    private void GetSplineForTerrainBetweenPoints(List<Vector3> splinePoints, out List<RoadPointData> splineRoadPoints, out List<RoadPointData> splineMiddleRoadpoints)
    {
        var spline = splineContainer.AddSpline();
        splineRoadPoints = new List<RoadPointData>();
        splineMiddleRoadpoints = new List<RoadPointData>();
        foreach (var point in splinePoints)
        {
            var splineRoadPointData = GetRoadPointDataForPoint(point);
            splineRoadPoints.Add(splineRoadPointData);
            spline.Add(splineRoadPointData.roadPoint, TangentMode.AutoSmooth);
        }

        float step = 1.0f / (resolution * splinePoints.Count);
        for (float i = step; i < 1.0f; i += step)
        {
            float3 splineEvalResult;
            float3 tangent;
            float3 upVector;
            SplineUtility.Evaluate(spline, i, out splineEvalResult, out tangent, out upVector);
            var pointOnCurve = new Vector3(splineEvalResult.x, splineEvalResult.y, splineEvalResult.z);
            if (splinePoints.Any(sp => (sp - pointOnCurve).magnitude < pointClosenessDelta))
            {
                continue;
            }
            splineMiddleRoadpoints.Add(GetRoadPointDataForPoint(pointOnCurve));
        }

        splineContainer.RemoveSpline(spline);
    }

    RoadPointData GetRoadPointDataForPoint(Vector3 pointOnCurve)
    {
        var rayCastPoint = new Vector3(pointOnCurve.x, pointOnCurve.y + heightDelta, pointOnCurve.z);
        Ray rayDown = new Ray(rayCastPoint, Vector3.down);

        if (Physics.Raycast(rayDown, out RaycastHit hitDown))
        {
            if (hitDown.collider.GetComponent<Highlight>())
            {
                var type = PointType.FollowMeshCurve;
                var distanceBetweenMeshPointAndStraightLinePoint = Math.Abs(hitDown.point.y - pointOnCurve.y);
                if (distanceBetweenMeshPointAndStraightLinePoint > bridgeDistanceDelta)
                {
                    type = PointType.Bridge;
                }
                else if (distanceBetweenMeshPointAndStraightLinePoint > levelTerrainDistanceDelta)
                {
                    type = PointType.LevelTerrain;
                }
                return new RoadPointData
                {
                    roadPoint = type == PointType.FollowMeshCurve ? new BezierKnot(hitDown.point) : new BezierKnot(pointOnCurve),
                    roadPointType = type
                };
            }
        }

        return new RoadPointData
        {
            roadPoint = new BezierKnot(pointOnCurve),
            roadPointType = PointType.Tunnel
        };

    }
}
