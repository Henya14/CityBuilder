using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

public class PointDrawer : MonoBehaviour
{
    public Camera playerCamera;
    public GameObject point;
    private GameObject pointInstance;
    private List<GameObject> pointInstances = new List<GameObject>();
    Vector3 LastMousePosition = Vector3.zero;
    SplineContainer splineContainer;
    Spline spline;
    float samplingDistance = 1.0f;
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

            }
        }
        if (Input.GetMouseButtonDown(0))
        {
            pointInstances.Add(pointInstance);
            pointInstance = null;
            if (pointInstances.Count >= 2)
            {
                CreateCurve();
                pointInstances.ForEach(p => Destroy(p));
                pointInstances.Clear();
            }
        }

    }

    void CreateCurve()
    {
        spline = splineContainer.AddSpline();
        spline.Add(new BezierKnot(pointInstances[0].transform.position), TangentMode.AutoSmooth);
        foreach (BezierKnot knot in GetInbetweenPoints(pointInstances[0].transform.position, pointInstances[1].transform.position))
        {
            spline.Add(knot, TangentMode.AutoSmooth);
        }
        spline.Add(new BezierKnot(pointInstances[1].transform.position), TangentMode.AutoSmooth);

    }

    private IEnumerable<BezierKnot> GetInbetweenPoints(Vector3 position1, Vector3 position2)
    {
        var knots = new List<BezierKnot>();
        var direction = position2 - position1;
        var middlePoints = Math.Floor(direction.magnitude / samplingDistance) - 1;

        for (int i = 1; i <= middlePoints; i++)
        {
            var point = position1 + i * direction.normalized;
            point.y += 700.0f;
            Ray rayDown = new Ray(point, Vector3.down);
           
            if (Physics.Raycast(rayDown, out RaycastHit hitDown))
            {
                
                if (hitDown.collider.GetComponent<Highlight>())
                {
                    Debug.Log("Down");
                    knots.Add(new BezierKnot(hitDown.point));
                    continue;
                }

            } 

            
        }

        return knots;
    }
}
