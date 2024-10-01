using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PointDrawer))]
public class RoadDrawerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        PointDrawer pd = (PointDrawer)target;

        if (DrawDefaultInspector())
        {
            if (pd.autoUpdateEnabled)
            {
                pd.DrawRoadCurve();
            }
        }
    }

}
