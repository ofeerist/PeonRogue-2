
using UnityEngine;
using UnityEditor;

using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections.Generic;

namespace Game.Tilemap
{
    [ExecuteInEditMode]
    public class TerrainEditor : MonoBehaviour
    {
        public PlaneController PlaneController;
        public Transform CurrentIndicator;
        public int CurrentTile;
        public string SaveLoadName;
        public TileMap LoadedTilemap;
        public int BrushSize = 1;
        public void Init()
        {
            if (CurrentIndicator == null)
            {
                PlaneController = GetComponentInChildren<PlaneController>();

                var gm = new GameObject();
                gm.transform.parent = transform;
                gm.name = "Selection";
                CurrentIndicator = gm.transform;
            }
        }
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;

            float x = BrushSize == 1 ? 1 : BrushSize * 2 - 1;
            Gizmos.DrawWireCube(CurrentIndicator.position, new Vector3(1f * x, .1f, 1f * x));
        }
        public void SaveTilemap()
        {
            PlaneController.Init();
            if (PlaneController.TileMap == null || PlaneController.TileMap.Tiles == null) throw new Exception("Tilemap is uninitialized");

            var dirPath = Application.dataPath + "/Resources/TileSaves/" + SaveLoadName + ".dat";
            if (File.Exists(dirPath))
            {
                File.Delete(dirPath);
            }

            BinaryFormatter bf = new BinaryFormatter();

            FileStream file = File.Create(dirPath);

            TileMap data = new TileMap(PlaneController.PlaneWidth, PlaneController.PlaneHeight)
            {
                Tiles = PlaneController.TileMap.Tiles
            };

            for (int i = 0; i < PlaneController.PlaneWidth; i++)
                for (int j = 0; j < PlaneController.PlaneHeight; j++)        
                    data.Tiles[i, j].SavedHeight = PlaneController.GetVerticeHeight(i, j); 

            bf.Serialize(file, data);

            file.Close();
#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
            Debug.Log("TileMap saved!");
        }

        public void LoadTilemap()
        {
            PlaneController.Init();
            var dirPath = Application.dataPath + "/TileSaves/" + SaveLoadName + ".dat";
            if (File.Exists(dirPath))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(dirPath, FileMode.Open);

                TileMap data = (TileMap)bf.Deserialize(file);

                file.Close();

                LoadedTilemap = data;
            }
            else
            {
                throw new Exception("Invalid saved tilemap name!");
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(TerrainEditor))]
    [CanEditMultipleObjects]
    public class TerrainEditorEditor : Editor
    {
        private void Init()
        {
            var terrainEditor = (TerrainEditor)target;
            int heightSize = terrainEditor.PlaneController.FatherTexture.height / 2048;
            int widthSize = terrainEditor.PlaneController.FatherTexture.width / 1024;
            _toolbarTextures = new Texture2D[heightSize * widthSize + 1];
            for (int i = 0; i < heightSize; i++)
            {
                for (int j = 0; j < widthSize; j++)
                {
                    Color[] main = terrainEditor.PlaneController.FatherTexture.GetPixels(j * 1024, i * 2048 + 768, 256, 256);
                    if (main[0].a != 0)
                    {
                        _toolbarTextures[j * heightSize + i] = new Texture2D(256, 256);
                        _toolbarTextures[j * heightSize + i].SetPixels(main);
                        _toolbarTextures[j * heightSize + i].Apply();
                    }
                }
            }
            _toolbarTextures[_toolbarTextures.Length - 1] = new Texture2D(256, 256);
            Color32[] colors = new Color32[256 * 256];
            for (int i = 0; i < colors.Length; i++) { colors[i].r = 0; colors[i].g = 0; colors[i].b = 0; colors[i].a = 255; }
            _toolbarTextures[_toolbarTextures.Length - 1].SetPixels32(colors);
            _toolbarTextures[_toolbarTextures.Length - 1].Apply();

            _planeManageModes = new Texture2D[4];

            _planeManageModes[0] = Resources.Load<Texture2D>("EditorIcons/heightbrush00");
            _planeManageModes[1] = Resources.Load<Texture2D>("EditorIcons/heightbrush04");
            _planeManageModes[2] = Resources.Load<Texture2D>("EditorIcons/heightbrush02");
            _planeManageModes[3] = Resources.Load<Texture2D>("EditorIcons/heightbrush01");
        }

        bool keyDown;
        float currentHeight;
        void OnSceneGUI()
        {
            var terrainEditor = (TerrainEditor)target;
            if (terrainEditor.CurrentIndicator == null)
                terrainEditor.Init();

            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                hit.point = new Vector3(Mathf.Round(hit.point.x), 0, Mathf.Round(hit.point.z));
                hit.point = new Vector3(hit.point.x, terrainEditor.PlaneController.GetVerticeHeight((int)hit.point.x, (int)hit.point.y), hit.point.z);
                terrainEditor.CurrentIndicator.position = hit.point;

                int x = Mathf.RoundToInt(terrainEditor.CurrentIndicator.localPosition.x);
                int y = Mathf.RoundToInt(terrainEditor.CurrentIndicator.localPosition.z);

                terrainEditor.CurrentIndicator.position = new Vector3(hit.point.x, terrainEditor.PlaneController.GetVerticeHeight(x, y) + .1f, hit.point.z);
            }


            Event e = Event.current;
            switch (e.type)
            {
                case EventType.KeyDown:              
                    keyDown = true;
                    break;
                    
                case EventType.KeyUp:
                    keyDown = false;
                    currentHeight = Mathf.Infinity;
                    break;
            }

            if (keyDown)
            {
                if (Event.current.keyCode == KeyCode.Space || (Event.current.keyCode == KeyCode.X))
                    if (terrainEditor.PlaneController.TileMap == null || terrainEditor.PlaneController.TileMap.Tiles == null) throw new Exception("Tilemap is uninitialized");

                int x = Mathf.RoundToInt(terrainEditor.CurrentIndicator.localPosition.x);
                int y = Mathf.RoundToInt(terrainEditor.CurrentIndicator.localPosition.z);

                if (Event.current.keyCode == KeyCode.Space)
                {
                    int size = terrainEditor.BrushSize;
                    int modx = size - 1;
                    int mody = size - 1;

                    for (int i = x - modx; i < x + modx + 1; i++)                 
                        for (int j = y - mody; j < y + mody + 1; j++)                       
                            terrainEditor.PlaneController.SetTileCorner(i, j, terrainEditor.CurrentTile);
                }

                if (Event.current.keyCode == KeyCode.X)
                {
                    if (_planeManageModeIndex == 0)
                        terrainEditor.PlaneController.AddCornerHeight(x, y, _amplitudeSlider, terrainEditor.BrushSize);

                    if (_planeManageModeIndex == 1)
                        terrainEditor.PlaneController.AddCornerHeight(x, y, -_amplitudeSlider, terrainEditor.BrushSize);

                    if (_planeManageModeIndex == 2)
                        terrainEditor.PlaneController.LerpCorner(x, y, terrainEditor.BrushSize);

                    if(_planeManageModeIndex == 3)
                    {
                        if (currentHeight == Mathf.Infinity) currentHeight = terrainEditor.PlaneController.GetVerticeHeight(x, y);
                        terrainEditor.PlaneController.SetCornerHeight(x, y, currentHeight, terrainEditor.BrushSize);
                    }
                }

            }
                
        }

        private int _palleteIndex = 0;
        private Texture2D[] _toolbarTextures;

        private float _sliderValue = 1f;

        private int _planeManageModeIndex;
        private Texture2D[] _planeManageModes;
        private float _amplitudeSlider = .1f;
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var terrainEditor = (TerrainEditor)target;
            terrainEditor.Init();

            using (var verticalScope = new GUILayout.VerticalScope("", EditorStyles.helpBox))
            {
                GUILayout.Label("Pallete", EditorStyles.boldLabel);

                _sliderValue = GUILayout.HorizontalSlider(_sliderValue, 1, 5, GUILayout.MinHeight(15), GUILayout.MaxWidth(200));
                _sliderValue = Mathf.RoundToInt(_sliderValue);
                terrainEditor.BrushSize = (int)_sliderValue;
                GUI.Label(new Rect(250, GUILayoutUtility.GetLastRect().y - 2.5f, 20, 20), _sliderValue.ToString(), EditorStyles.boldLabel);

                if (_toolbarTextures == null || _planeManageModes == null) Init();
                var textures = new List<Texture2D>();
                foreach (var texture in _toolbarTextures)
                {
                    if (texture != null) textures.Add(texture);
                }
                _palleteIndex = GUILayout.SelectionGrid(_palleteIndex, textures.ToArray(), 4, EditorStyles.objectFieldThumb, GUILayout.MaxHeight(280), GUILayout.MaxWidth(256));
                terrainEditor.CurrentTile = _palleteIndex;
                if (_palleteIndex == textures.Count - 1) terrainEditor.CurrentTile = _toolbarTextures.Length - 1;

                if (GUILayout.Button("Update Pallete", EditorStyles.miniButton, GUILayout.MaxWidth(125)))
                {
                    AssetDatabase.ImportAsset("Assets/Resources/TerrainMaterial/R_FatherNormalTexture.png", ImportAssetOptions.ImportRecursive);
                    AssetDatabase.ImportAsset("Assets/Resources/TerrainMaterial/R_FatherOrmTexture.png", ImportAssetOptions.ImportRecursive);
                    AssetDatabase.ImportAsset("Assets/Resources/TerrainMaterial/R_FatherTexture.png", ImportAssetOptions.ImportRecursive);
                    Init();
                }
            }

            using (var verticalScope = new GUILayout.VerticalScope("", EditorStyles.helpBox))
            {
                _amplitudeSlider = GUILayout.HorizontalSlider(_amplitudeSlider, 0, 1, GUILayout.MinHeight(15), GUILayout.MaxWidth(200));
                GUI.Label(new Rect(250, GUILayoutUtility.GetLastRect().y - 2.5f, 30, 20), _amplitudeSlider.ToString(), EditorStyles.boldLabel);

                _planeManageModeIndex = GUILayout.SelectionGrid(_planeManageModeIndex, _planeManageModes, 4, EditorStyles.objectFieldThumb, GUILayout.MaxHeight(50), GUILayout.MaxWidth(200));
            }

            using (var verticalScope = new GUILayout.VerticalScope("", EditorStyles.helpBox))
            {
                terrainEditor.SaveLoadName = EditorGUILayout.DelayedTextField("Save Name", terrainEditor.SaveLoadName);
                if (GUILayout.Button("Load", EditorStyles.miniButton, GUILayout.MaxWidth(125)))
                {
                    terrainEditor.PlaneController.InitFatherConstants();
                    terrainEditor.LoadTilemap();
                    terrainEditor.PlaneController.InitTilemap(terrainEditor.LoadedTilemap);
                    terrainEditor.LoadedTilemap = null;
                }
                if (GUILayout.Button("Save", EditorStyles.miniButton, GUILayout.MaxWidth(125)))
                {
                    terrainEditor.PlaneController.InitFatherConstants();
                    terrainEditor.SaveTilemap();
                }
                if (GUILayout.Button("New", EditorStyles.miniButton, GUILayout.MaxWidth(125)))
                {
                    terrainEditor.PlaneController.Init();
                    terrainEditor.PlaneController.InitFatherConstants();
                    terrainEditor.PlaneController.InitTilemap();
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}