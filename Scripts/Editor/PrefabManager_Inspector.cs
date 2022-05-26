using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EntitiesController))]
public class PrefabManager_Inspector : Editor {
    EntitiesController context = null;
    Color defaultGUIColor;
    private readonly GUIContent processInBatchesGUIContent = new GUIContent(
        text: "Should process in batches",
        tooltip: "Enable this to be able to decrease realism of simulation in exchange for performance");
    private readonly GUIContent rotationBatchGUIContent = new GUIContent("Rotations batch.", "How many rotations to perform per frame. Increase this to make the simulation more realistic in exchange for performance.");
    private readonly GUIContent fadesBatchGUIContent = new GUIContent("Fading batch.", "How many fades to perform per frame. Increase this to make the simulation more realistic in exchange for performance.");
    private bool showEntities = false;
    private bool shouldProcessInBatches = false;
    private bool[] foldoutsStatus = null;
    private void OnEnable() {
        context = target as EntitiesController;
        defaultGUIColor = GUI.color;
        foldoutsStatus = new bool[context.numberOfEntities];
    }


    public override void OnInspectorGUI() {


        base.OnInspectorGUI();
        if (Application.isPlaying == false)
            context.numberOfEntities = EditorGUILayout.IntField("Number of entities: ", context.numberOfEntities);

        shouldProcessInBatches = EditorGUILayout.Toggle(processInBatchesGUIContent, context.ProcessInBatches);
        if (shouldProcessInBatches != context.ProcessInBatches && Application.isPlaying) {
            if (context.ProcessInBatches == true) {
                context.StopCoroutine(context.rotationCoroutine);
                context.StopCoroutine(context.fadeCoroutine);
            }
            else {
                context.StartCoroutine(context.rotationCoroutine);
                context.StartCoroutine(context.fadeCoroutine);
            }
        }
        context.ProcessInBatches = shouldProcessInBatches;
        if (shouldProcessInBatches) {
            EditorGUILayout.Space(25);
            EditorGUILayout.LabelField("Batching Options", EditorStyles.whiteLargeLabel);
            context.rotationProcessBatch = EditorGUILayout.IntSlider(rotationBatchGUIContent, context.rotationProcessBatch, 1, context.numberOfEntities);
            context.fadingProcessBatch = EditorGUILayout.IntSlider(fadesBatchGUIContent, context.fadingProcessBatch, 1, context.numberOfEntities);
        }
        EditorGUILayout.Space(25);
        EditorGUILayout.LabelField("Cube color range", EditorStyles.whiteLargeLabel);
        MinMaxLayout("Color Range Red", ref context.ColorRangeMinRed, ref context.ColorRangeMaxRed, 0, 1, Color.red);
        MinMaxLayout("Color Range Green", ref context.ColorRangeMinGreen, ref context.ColorRangeMaxGreen, 0, 1, Color.green);
        MinMaxLayout("Color Range Blue", ref context.ColorRangeMinBlue, ref context.ColorRangeMaxBlue, 0, 1, Color.blue);

        EditorGUILayout.Space(25);
        EditorGUILayout.LabelField("Entities speed and life range", EditorStyles.whiteLargeLabel);
        RangeLayout(ref context.SpeedRangeMin, ref context.SpeedRangeMax, "speed");
        MinMaxLayout("Life-time range", ref context.LifeRangeMin, ref context.LifeRangeMax, 1, 100, Color.white);
        context.LifeRangeMin = (int)context.LifeRangeMin;
        context.LifeRangeMax = (int)context.LifeRangeMax;
        EditorGUILayout.Space(20);
        if (context.Transforms != null && context.Transforms.Length > 0) {
            showEntities = EditorGUILayout.Foldout(showEntities, "Show entities inspector (enabling this will decrease editor performance)");
            if (showEntities) {
                for (int i = 0; i < context.numberOfEntities; i++) {
                    Transform trans = context.Transforms[i];
                    foldoutsStatus[i] = EditorGUILayout.Foldout(foldoutsStatus[i], trans.name);
                    if (foldoutsStatus[i]) {
                        context.Angles[i] = EditorGUILayout.Vector3Field("Rotating angle: ", context.Angles[i]);
                        context.Speeds[i] = EditorGUILayout.FloatField("Speed", context.Speeds[i]);
                        context.LifeSpan[i] = EditorGUILayout.FloatField("Life-time: ", context.LifeSpan[i]);
                        context.PrefabsRenderers[i].material.color = EditorGUILayout.ColorField("Color: ", context.PrefabsRenderers[i].material.color);
                        context.NeedToFade[i] = EditorGUILayout.Toggle("Is fading: ", context.NeedToFade[i]);
                    }
                }
            }
        }
    }

    private void RangeLayout(ref float value1, ref float value2, string valueName, bool showHorizontal = true) {
        if (showHorizontal) EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Min " + valueName + ':', GUILayout.Width(100 /*+ valueName.Length * 4*/));
        if (showHorizontal) EditorGUILayout.Space(15, false);
        value1 = EditorGUILayout.FloatField(value1, GUILayout.Width(100));
        if (showHorizontal) EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Max " + valueName + ':', GUILayout.Width(50 + valueName.Length * 4));
        value2 = EditorGUILayout.FloatField(value2, GUILayout.Width(100));
        if (showHorizontal) EditorGUILayout.EndHorizontal();
    }

    private void MinMaxLayout(string mainLable, ref float minValue, ref float maxValue, float minLimit, float maxLimit, Color color) {
        EditorGUILayout.LabelField(mainLable, EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Min: " + minValue, GUILayout.Width(60));
        GUI.color = color;
        EditorGUILayout.MinMaxSlider(ref minValue, ref maxValue, minLimit, maxLimit, GUILayout.Width(EditorGUIUtility.currentViewWidth - 150));
        GUI.color = defaultGUIColor;
        EditorGUILayout.LabelField("Max: " + maxValue);
        EditorGUILayout.EndHorizontal();
        minValue = (float)Math.Round(minValue, 2);
        maxValue = (float)Math.Round(maxValue, 2);
    }
}
