﻿using UnityEditor;
using UnityEngine;

/// <summary>
/// Custom Editor for Edge Objects
/// Allows user to manipulate controls points of bezier splin using Unity Handles
/// </summary>
[CustomEditor(typeof(Edge))]
public class EdgeInspector : Editor
{
    private Edge spline;
    private Transform handleTransform;
    private Quaternion handleRotation;

    private const float handleSize = 0.04f;
    private const float pickSize = 0.06f;

    private int selectedIndex = -1;

    private static Color[] modeColors = {
        Color.blue,
        Color.cyan,
        Color.yellow
    };

    private void OnScene(SceneView sceneview)
    {
        OnSceneGUI();
    }

    void OnEnable()
    {
        SceneView.onSceneGUIDelegate -= OnScene;
        SceneView.onSceneGUIDelegate += OnScene;
    }


    public override void OnInspectorGUI()
    {
        spline = target as Edge;

        GUILayout.Label("N1: " + spline.Nodes[0] + " , N2: " + spline.Nodes[1]);

        EditorGUI.BeginChangeCheck();
        bool loop = EditorGUILayout.Toggle("Loop", spline.Loop);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(spline, "Toggle Loop");
            EditorUtility.SetDirty(spline);
            spline.Loop = loop;
        }

        EditorGUI.BeginChangeCheck();
        GUILayout.Label("Line Steps");
        int lineSteps = EditorGUILayout.IntSlider(spline.LineSteps, 10, 20);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(spline, "Set Line Steps");
            EditorUtility.SetDirty(spline);
            spline.LineSteps = lineSteps;
        }

        EditorGUI.BeginChangeCheck();
        GUILayout.Label("Direction Vector Scale");
        float directionScale = EditorGUILayout.Slider(spline.DirectionScale, 0.5f, 5.0f);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(spline, "Set Direction Scale");
            EditorUtility.SetDirty(spline);
            spline.DirectionScale = directionScale;
        }

        if (selectedIndex >= 0 && selectedIndex < spline.ControlPointCount)
        {
            DrawSelectedPointInspector();
        }

        if (GUILayout.Button("Add Curve"))
        {
            Undo.RecordObject(spline, "Add Curve");
            spline.AddCurve();
            EditorUtility.SetDirty(spline);
        }
    }

    private void DrawSelectedPointInspector()
    {
        GUILayout.Label("Selecting Point: " + selectedIndex);
        EditorGUI.BeginChangeCheck();
        Vector3 point = EditorGUILayout.Vector3Field("Position", spline.GetControlPoint(selectedIndex));
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(spline, "Move Point");
            EditorUtility.SetDirty(spline);
            spline.SetControlPoint(selectedIndex, point);
        }

        EditorGUI.BeginChangeCheck();
        BezierControlPointMode mode = (BezierControlPointMode)EditorGUILayout.EnumPopup("Mode", spline.GetControlPointMode(selectedIndex));
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(spline, "Change Point Mode");
            spline.SetControlPointMode(selectedIndex, mode);
            EditorUtility.SetDirty(spline);
        }
    }

    private void OnSceneGUI()
    {
        spline = target as Edge;

        if (spline == null)
            return;

        handleTransform = spline.transform;
        //Debug.Log(spline.transform.position);
        handleRotation = Tools.pivotRotation == PivotRotation.Local ?
            handleTransform.rotation : Quaternion.identity;

        Vector3 p0 = ShowPoint(0);
        for (int i = 1; i < spline.ControlPointCount; i += 3)
        {
            Vector3 p1 = ShowPoint(i);
            Vector3 p2 = ShowPoint(i + 1);
            Vector3 p3 = ShowPoint(i + 2);

            Handles.color = Color.gray;
            Handles.DrawLine(p0, p1);
            Handles.DrawLine(p2, p3);

            Handles.DrawBezier(p0, p3, p1, p2, Color.magenta, null, 2f);
            p0 = p3;
        }
        ShowDirections();
    }

    private void ShowDirections()
    {
        Handles.color = Color.green;
        Vector3 point = spline.GetPoint(0f);
        Handles.DrawLine(point, point + spline.GetDirection(0f) * spline.DirectionScale);
        int steps = spline.LineSteps * spline.CurveCount;
        for (int i = 1; i <= steps; i++)
        {
            point = spline.GetPoint(i / (float)steps);
            Handles.DrawLine(point, point + spline.GetDirection(i / (float)steps) * spline.DirectionScale);
        }
    }

    private Vector3 ShowPoint(int index)
    {
        Vector3 point = handleTransform.TransformPoint(spline.GetControlPoint(index));
        float size = HandleUtility.GetHandleSize(point);
        if (index == 0)
        {
            size *= 2f;
        }
        Handles.color = modeColors[(int)spline.GetControlPointMode(index)];
        if (Handles.Button(point, handleRotation, size * handleSize, size * pickSize, Handles.DotCap))
        {
            selectedIndex = index;
            Repaint();
        }
        if (selectedIndex == index)
        {
            EditorGUI.BeginChangeCheck();
            point = Handles.DoPositionHandle(point, handleRotation);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(spline, "Move Point");
                EditorUtility.SetDirty(spline);
                spline.SetControlPoint(index, handleTransform.InverseTransformPoint(point));
            }
        }
        return point;
    }
}