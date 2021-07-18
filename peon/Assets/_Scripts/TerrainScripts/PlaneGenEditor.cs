using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
namespace Game.Tilemap
{
    [CustomEditor(typeof(PlaneController))]
    [CanEditMultipleObjects]
    public class PlaneGenEditor : Editor
    {
        SerializedProperty TileTextures;
        SerializedProperty TileNormalTextures;
        SerializedProperty TileOrmTextures;
        SerializedProperty FatherMaterial;

        SerializedProperty FatherAlbedoTexture;
        SerializedProperty FatherNormalTexture;
        SerializedProperty FatherOrmTexture;
        private void OnEnable()
        {
            TileTextures = serializedObject.FindProperty("TileTextures");
            TileNormalTextures = serializedObject.FindProperty("TileNormalTextures");
            TileOrmTextures = serializedObject.FindProperty("TileOrmTextures");
            FatherMaterial = serializedObject.FindProperty("FatherMaterial");

            FatherAlbedoTexture = serializedObject.FindProperty("FatherTexture");
            FatherNormalTexture = serializedObject.FindProperty("FatherNormalTexture");
            FatherOrmTexture = serializedObject.FindProperty("FatherOrmTexture");
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var planeGen = (PlaneController)target;

            GUILayout.Label("Options", EditorStyles.boldLabel);

            using (var verticalScope = new GUILayout.VerticalScope("box"))
            {

                GUILayout.Label("TileMap size", EditorStyles.miniBoldLabel);
                planeGen.PlaneHeight = EditorGUILayout.DelayedIntField("Column count", planeGen.PlaneHeight);
                planeGen.PlaneWidth = EditorGUILayout.DelayedIntField("Row count", planeGen.PlaneWidth);
                planeGen.HighPolyModificator = EditorGUILayout.DelayedIntField("HighPoly Modificator", planeGen.HighPolyModificator);

                GUILayout.Label("Tile size", EditorStyles.miniBoldLabel);
                planeGen.TileHeight = EditorGUILayout.DelayedFloatField("Tile height", planeGen.TileHeight);
                planeGen.TileWidth = EditorGUILayout.DelayedFloatField("Tile width", planeGen.TileWidth);

                GUILayout.Label("Tiles", EditorStyles.miniBoldLabel);
                EditorGUILayout.PropertyField(TileTextures);
                EditorGUILayout.PropertyField(TileNormalTextures);
                EditorGUILayout.PropertyField(TileOrmTextures);

                GUILayout.Label("FatherTextures", EditorStyles.miniBoldLabel);
                EditorGUILayout.PropertyField(FatherAlbedoTexture);
                EditorGUILayout.PropertyField(FatherNormalTexture);
                EditorGUILayout.PropertyField(FatherOrmTexture);

                GUILayout.Label("FatherMaterial", EditorStyles.miniBoldLabel);
                EditorGUILayout.PropertyField(FatherMaterial);

            }


            GUILayout.Label("Instruments", EditorStyles.boldLabel);
            using (var verticalScope = new GUILayout.VerticalScope("box"))
            {
                if (GUILayout.Button("Init Master Constants", EditorStyles.miniButton))
                    planeGen.InitFatherConstants();

                if (GUILayout.Button("Recalculate FatherTexture", EditorStyles.miniButton))
                    planeGen.InitFatherTexture();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
