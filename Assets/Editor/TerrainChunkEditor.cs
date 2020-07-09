using Minecraft;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TerrainChunk))]
public sealed class TerrainChunkEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var terrainChunk = (TerrainChunk)target;

        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.Vector3IntField("Index", new Vector3Int(terrainChunk.Index.X, terrainChunk.Index.Y, terrainChunk.Index.Z));
        EditorGUI.EndDisabledGroup();
    }
}