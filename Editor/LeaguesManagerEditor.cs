using UnityEngine;
using UnityEditor;
using LionStudios.Suite.Leaderboards.Fake;

[CustomEditor(typeof(LeaguesManager))]
public class LeagueManagerEditor : Editor
{
    private bool showFoldout = false;

    private static readonly string[] _propertiesToExclude = new[]
    {
        "m_Script",
        "overrideJoin",
        "animatePlayerOnly",
        "perRankTime",
        "maxPlayerAnimationTime",
        "minPlayerAnimationTime"
    };

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawPropertiesExcluding(serializedObject, _propertiesToExclude);

        EditorGUILayout.Space(20);

        showFoldout = EditorGUILayout.Foldout(showFoldout, "Advanced");

        if (showFoldout)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("overrideJoin"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("animatePlayerOnly"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("perRankTime"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxPlayerAnimationTime"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("minPlayerAnimationTime"));
        }

        serializedObject.ApplyModifiedProperties();
    }
}