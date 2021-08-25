using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace _Scripts.TerrainScripts
{

    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshCollider))]
    [ExecuteInEditMode]
    public class PlaneController : MonoBehaviour
    {
        public float TileWidth;
        public float TileHeight;
        public int PlaneWidth;
        public int PlaneHeight;
        public readonly int Resolution = 256;
        public int HighPolyModificator;

        public Texture2D FatherTexture;
        public Texture2D FatherNormalTexture;
        public Texture2D FatherOrmTexture;
        public Material FatherMaterial;
        int FatherWidth;
        int FatherHeight;

        public Texture2D[] TileTextures;
        public Texture2D[] TileNormalTextures;
        public Texture2D[] TileOrmTextures;

        MeshFilter MeshFilter;
        MeshCollider MeshCollider;
        public TileMap TileMap;

        public void SetTileCorner(int topRightX, int topRightY, int tileIndex)
        {
            if(TileMap == null) throw new System.Exception("Tilemap is uninitialized");

            if (tileIndex == (FatherTexture.height / 2048) * (FatherTexture.width / 1024))
            {
                for (int i = 1; i < 5; i++)
                {
                    if (topRightX < PlaneWidth && topRightX >= 0 && topRightY < PlaneHeight && topRightY >= 0) 
                        SetTilePart(topRightX, topRightY, i, null);
                    if (topRightX - 1 < PlaneWidth && topRightX - 1 >= 0 && topRightY < PlaneHeight && topRightY >= 0) 
                        SetTilePart(topRightX - 1, topRightY, i, null);
                    if (topRightX < PlaneWidth && topRightX >= 0 && topRightY - 1 < PlaneHeight && topRightY - 1 >= 0) 
                        SetTilePart(topRightX, topRightY - 1, i, null);
                    if (topRightX - 1 < PlaneWidth && topRightX - 1 >= 0 && topRightY - 1 < PlaneHeight && topRightY - 1 >= 0)
                        SetTilePart(topRightX - 1, topRightY - 1, i, null);
                }
                return;
            }

            var cornerPoints = new CornerPoints[4] { new CornerPoints(false, false, false, true), new CornerPoints(false, false, true, false), new CornerPoints(false, true, false, false), new CornerPoints(true, false, false, false) };
            var positions = new Vector2[4] { new Vector2(topRightX, topRightY), new Vector2(topRightX - 1, topRightY), new Vector2(topRightX, topRightY - 1), new Vector2(topRightX - 1, topRightY - 1) };

            for (int i = 0; i < cornerPoints.Length; i++)
            {
                int x = (int) positions[i].x;
                int y = (int) positions[i].y;
                var c = GetCellByTile(x, y, tileIndex);

                var cp = cornerPoints[i];
                if (c != null) c.CornerPoints = c.CornerPoints.Summarize(cp);

                if (x < PlaneWidth && x >= 0 && y < PlaneHeight && y >= 0)
                {
                    if (c == null) SetTilePartByHeight(x, y, TileMapConstants.GetCellFromCornerPoints(cp, tileIndex, 3));
                    else SetTilePartByHeight(x, y, TileMapConstants.GetCellFromCornerPoints(c.CornerPoints, tileIndex, 3));

                    ClearUpperLayers(x, y, tileIndex, cp);
                }
            }
        }

        private void ClearUpperLayers(int x, int y, int tileIndex, CornerPoints cp)
        {
            if (TileMap == null || TileMap.Tiles == null) throw new System.Exception("Tilemap is uninitialized");


            if (x < PlaneWidth && x >= 0 && y < PlaneHeight && y >= 0)

                for (int i = 1; i < 5; i++)
                {
                    Cell c = TileMap.Tiles[x, y].GetCell(i);
                    if (c != null && !c.CompareFully(new Cell(3, 0, 7)) && c.Tile != tileIndex)
                    {
                        SetTilePart(x, y, i, TileMapConstants.GetCellFromCornerPoints(c.CornerPoints.Subtract(cp), c.Tile, 3));
                    }
                }
        }

        public Cell GetCellByTile(int x, int y, int tileIndex)
        {
            if (TileMap == null || TileMap.Tiles == null) throw new System.Exception("Tilemap is uninitialized");

            if (x < PlaneWidth && x >= 0 && y < PlaneHeight && y >= 0)

                for (int i = 1; i < 5; i++)
                {
                    Cell c = TileMap.Tiles[x, y].GetCell(i);
                    if(c != null && !c.CompareFully(new Cell(3, 0, 7)) && c.Tile == tileIndex)
                    {
                        return c;
                    }
                }

            return null;
        }

        private void SetTilePartByHeight(int x, int y, Cell cell)
        {
            var cells = new Cell[4];
            cells[0] = TileMap.Tiles[x, y].GetCell(1);
            cells[1] = TileMap.Tiles[x, y].GetCell(2);
            cells[2] = TileMap.Tiles[x, y].GetCell(3);
            cells[3] = TileMap.Tiles[x, y].GetCell(4);

            for (int i = 0; i < cells.Length; i++)
            {
                if (cells[i] != null && cells[i].Tile == cell.Tile && !cells[i].CompareFully(new Cell(3, 0, 7)))
                {
                    cells[i] = cell;
                    break;
                }

                if (i == cells.Length - 1)
                    for (int j = 0; j < cells.Length; j++)
                    {
                        if (cells[j] == null || cells[j].CompareFully(new Cell(3, 0, 7))) { cells[j] = cell; break; }
                    }
            }

            var unnull = new List<Cell>();
            for (int i = 0; i < cells.Length; i++)
                if (cells[i] != null && !cells[i].CompareFully(new Cell(3, 0, 7))) 
                    unnull.Add(cells[i]);

            var sorted = unnull.ToArray();
            System.Array.Sort(sorted);

            for (int i = 0; i < cells.Length; i++)
            {
                cells[i] = null;
                if (i < sorted.Length)
                    cells[i] = sorted[i];
                SetTilePart(x, y, i + 1, cells[i]);
            }
        }

        private void SetTilePart(int x, int y, int uvIndex, Cell cell)
        {
            int offset = x * 4 + (y * 4 * PlaneWidth * HighPolyModificator);
            float ux = 1.0f / FatherWidth * Resolution;
            float uy = 1.0f / FatherHeight * Resolution;

            var uvArray = GetUVArrayByIndex(uvIndex);

            if (cell != null)
            {
                var uv = GetUV(cell.Tile, cell.X, cell.Y);
                cell.GetChangedUV(uvArray, x, y, PlaneWidth, HighPolyModificator, uv, ux, uy);
            }
            else
            {
                cell = new Cell(3, 0, 7);
                var uv = GetUV(cell.Tile, cell.X, cell.Y);
                cell.GetChangedUV(uvArray, x, y, PlaneWidth, HighPolyModificator, uv, ux, uy);
            }

            TileMap.Tiles[x, y].SetCell(cell, uvIndex);
            SetUVArrayByIndex(uvIndex, uvArray);
        }

        private Vector2[] GetUVArrayByIndex(int index)
        {
            switch (index)
            {
                case 1:
                    return MeshFilter.sharedMesh.uv; 
                case 2:
                    return MeshFilter.sharedMesh.uv2;
                case 3:
                    return MeshFilter.sharedMesh.uv3;
                case 4:
                    return MeshFilter.sharedMesh.uv4;
                default:
                    throw new UnityException("Invalid index in GetUVArrayByIndex");
            }
        }

        private void SetUVArrayByIndex(int index, Vector2[] array)
        {
            switch (index)
            {
                case 1:
                    MeshFilter.sharedMesh.uv = array;
                    break;
                case 2:
                    MeshFilter.sharedMesh.uv2 = array;
                    break;
                case 3:
                    MeshFilter.sharedMesh.uv3 = array;
                    break;
                case 4:
                    MeshFilter.sharedMesh.uv4 = array;
                    break;
                default:
                    throw new UnityException("Invalid index in GetUVArrayByIndex");
            }
        }

        private Vector2 GetUV(int tileIndex, int x, int y)
        {
            float cWidth = 1.0f / (FatherWidth / Resolution);
            float cHeight = 1.0f / (FatherHeight / Resolution);

            int row = (int)((float)tileIndex / 4);
            tileIndex = tileIndex < 4 ? tileIndex : tileIndex - row * 4;
            float dx = (row * 4 * cWidth) + (x * cWidth);
            float dy = (tileIndex * 8 * cHeight) + (y * cHeight);

            var defVec = new Vector2(dx, dy);

            return defVec;
        }

        private void SaveTexture(Texture2D texture, string name)
        {
            byte[] bytes = texture.EncodeToTGA();
            var dirPath = Application.dataPath + "/Resources/TerrainMaterial";

            System.IO.File.WriteAllBytes(dirPath + "/R_" + name + ".png", bytes);
            Debug.Log(bytes.Length / 1024 + "Kb was saved as: " + dirPath);
#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
        }

        public void InitFatherConstants()
        {
            FatherHeight = FatherMaterial.GetTexture("_MainTex").height;
            FatherWidth = FatherMaterial.GetTexture("_MainTex").width;
        }

        public void InitFatherTexture()
        {
            FatherHeight = 2048 * 4;
            FatherWidth = 1024 * ((int)((float)TileTextures.Length / 4) + 1);

            var width = Resolution * 4;

            FatherTexture = new Texture2D(FatherWidth, FatherHeight);
            FatherNormalTexture = new Texture2D(FatherWidth, FatherHeight);
            FatherOrmTexture = new Texture2D(FatherWidth, FatherHeight);

            Color32[] alpha = new Color32[FatherWidth * FatherHeight];
            for (int k = 0; k < alpha.Length; k++)
            { alpha[k].r = 0; alpha[k].g = 0; alpha[k].b = 0; alpha[k].a = 0; }
            FatherTexture.SetPixels32(0, 0, FatherWidth, FatherHeight, alpha);

            int addX = 0;
            int addY = 0;
            for (int i = 0; i < TileTextures.Length; i++)
            {
                if (i % 4 == 0 && i != 0) { addX += 1024; addY = 0; }

                UnityEngine.Color[] main = TileTextures[i].GetPixels(0, 0, width, width);
                FatherTexture.SetPixels(addX, 2048 * addY, width, width, main);
                if (TileTextures[i].width > 1024)
                {
                    UnityEngine.Color[] variations = TileTextures[i].GetPixels(1024, 0, width, width);
                    FatherTexture.SetPixels(addX, 2048 * addY + 1024, width, width, variations);
                }

                addY++;
            }

            for (int k = 0; k < alpha.Length; k++)
            { alpha[k].r = 0; alpha[k].g = 0; alpha[k].b = 0; alpha[k].a = 0; }
            FatherNormalTexture.SetPixels32(0, 0, FatherWidth, FatherHeight, alpha);

            addX = 0;
            addY = 0;
            for (int i = 0; i < TileTextures.Length; i++)
            {
                if (i % 4 == 0 && i != 0) { addX += 1024; addY = 0; }

                UnityEngine.Color[] main = TileNormalTextures[i].GetPixels(0, 0, width, width);
                FatherNormalTexture.SetPixels(addX, 2048 * addY, width, width, main);
                if (TileTextures[i].width > 1024)
                {
                    UnityEngine.Color[] variations = TileNormalTextures[i].GetPixels(1024, 0, width, width);
                    FatherNormalTexture.SetPixels(addX, 2048 * addY + 1024, width, width, variations);
                }

                addY++;
            }


            for (int k = 0; k < alpha.Length; k++)
            { alpha[k].r = 0; alpha[k].g = 0; alpha[k].b = 0; alpha[k].a = 0; }
            FatherOrmTexture.SetPixels32(0, 0, FatherWidth, FatherHeight, alpha);

            addX = 0;
            addY = 0;
            for (int i = 0; i < TileTextures.Length; i++)
            {
                if (i % 4 == 0 && i != 0) { addX += 1024; addY = 0; }

                UnityEngine.Color[] main = TileOrmTextures[i].GetPixels(0, 0, width, width);
                FatherOrmTexture.SetPixels(addX, 2048 * addY, width, width, main);
                if (TileTextures[i].width > 1024)
                {
                    UnityEngine.Color[] variations = TileOrmTextures[i].GetPixels(1024, 0, width, width);
                    FatherOrmTexture.SetPixels(addX, 2048 * addY + 1024, width, width, variations);
                }

                addY++;
            }

            FatherTexture.mipMapBias = -1;
            FatherNormalTexture.mipMapBias = -1;
            FatherOrmTexture.mipMapBias = -1;

            // Applying
            FatherNormalTexture.Apply();
            FatherTexture.Apply();
            FatherOrmTexture.Apply();

            // Saving
            SaveTexture(FatherTexture, "FatherTexture");
            SaveTexture(FatherNormalTexture, "FatherNormalTexture");
            SaveTexture(FatherOrmTexture, "FatherOrmTexture");
        }
        public void Init()
        {
            // Получение глобалок
            MeshFilter = GetComponent<MeshFilter>();
            MeshCollider = GetComponent<MeshCollider>();
        }

        public void InitTilemap(TileMap tileMap = null)
        {
            MeshFilter = CreatePlane(MeshFilter);

            if (tileMap == null)
            {
                TileMap = new TileMap(PlaneWidth, PlaneHeight);

                for (int y = 0; y < PlaneHeight; y++)
                {
                    for (int x = 0; x < PlaneWidth; x++)
                    {
                        var c2 = new Cell(0, Random.Range(0, 3), 4 + Random.Range(0, 3));
                        SetTilePartByHeight(x, y, c2);
                    }
                }
            }
            else
            {
                TileMap = tileMap;

                if (tileMap.Tiles.Length != PlaneHeight * PlaneWidth) throw new System.Exception("invalid tilemap");

                for (int y = 0; y < PlaneHeight; y++)
                {
                    for (int x = 0; x < PlaneWidth; x++)
                    {
                        SetTilePart(x, y, 1, tileMap.Tiles[x, y].Cell1);
                        SetTilePart(x, y, 2, tileMap.Tiles[x, y].Cell2);
                        SetTilePart(x, y, 3, tileMap.Tiles[x, y].Cell3);
                        SetTilePart(x, y, 4, tileMap.Tiles[x, y].Cell4);

                        try
                        {
                            SetCornerHeight(x, y, tileMap.Tiles[x, y].SavedHeight, 1);
                        }
                        catch { }
                    }
                }
            }

            MeshCollider.sharedMesh = MeshFilter.sharedMesh;
        }
        public void ClearTilemap()
        {
            DestroyImmediate(MeshFilter.sharedMesh);
        }

        public void LerpCorner(int x, int y, int size)
        {
            var mn = HighPolyModificator;
            var m = mn * size;

            int offset;
            int dir;

            x *= mn;
            y *= mn;

            Vector3[] veritces = MeshFilter.sharedMesh.vertices;

            for (int i = 0 - m + 1; i < m; i++)
                for (int j = 0 - m + 1; j < m; j++)
                {
                    int tx = x + i;
                    int ty = y + j;
                    for (int a = 0; a < 4; a++)
                    {
                        dir = a;
                        offset = GetOffsetByDirection(tx, ty, a);

                        float average = 0;
                        for (int xd = -1; xd <= 1; xd++)
                        {
                            for (int yd = -1; yd <= 1; yd++)
                            {
                                if(xd != 0 && yd != 0)
                                average += GetVerticeHeight(tx - xd, ty - yd);
                            }
                        }
                        average /= 8;

                        float t = .01f;
                        veritces[dir + offset] = new Vector3(
                            veritces[dir + offset].x,
                            Mathf.Lerp(veritces[dir + offset].y, average, t),
                            veritces[dir + offset].z);
                    }
                }

            MeshFilter.sharedMesh.vertices = veritces;
            MeshFilter.sharedMesh.RecalculateNormals();
            MeshFilter.sharedMesh.RecalculateBounds();
            MeshFilter.sharedMesh.RecalculateTangents();
        }

        public void AddCornerHeight(int x, int y, float height, int size)
        {
            var mn = HighPolyModificator;
            var m = mn * size;

            int offset;
            int dir;

            x *= mn;
            y *= mn;

            Vector3[] veritces = MeshFilter.sharedMesh.vertices;
            float[,] heightMap = new float[m * 2, m * 2];

            float maxHeight = 0;
            for (int i = 0 - m + 1; i < m; i++)
                for (int j = 0 - m + 1; j < m; j++)
                {
                    int tx = x + i;
                    int ty = y + j;
                    for (int z = -m; z <= m; z++)
                        for (int k = -m; k <= m; k++)
                        {
                            float tz = Mathf.Abs((float)z);
                            float tk = Mathf.Abs((float)k);
                            if (tx + z == x && ty + k == y)
                            {
                                float distance = Mathf.Sqrt(Mathf.Pow(tz, 2) + Mathf.Pow(tk, 2));
                                distance = distance == 0 ? .5f : distance; // Зависимость через две строчки, внимательно
                                if(height > 0)
                                    heightMap[i + m - 1, j + m - 1] = Mathf.Abs(distance / ((m * 1.35f) / height) - height);
                                else
                                    heightMap[i + m - 1, j + m - 1] = -Mathf.Abs(distance / ((m * 1.35f) / height) - height);
                                if (distance == .5f) maxHeight = heightMap[i + m - 1, j + m - 1];
                            }

                        }
                }
 
            float startValue = Mathf.Infinity;
            for (int i = 0 - m + 1; i < m; i++)
                for (int j = 0 - m + 1; j < m; j++)
                {
                    int tx = x + i;
                    int ty = y + j;

                    float heightKey = heightMap[i + m - 1, j + m - 1];
                    

                    for (int a = 0; a < 4; a++)
                    {
                        dir = a;
                        offset = GetOffsetByDirection(tx, ty, a);

                        try
                        {
                            if (heightKey == maxHeight) startValue = veritces[dir + offset].y;
                            veritces[dir + offset] = new Vector3(veritces[dir + offset].x, veritces[dir + offset].y += heightKey, veritces[dir + offset].z);
                        }
                        catch { }
                    }
                }

            for (int i = 0 - m + 1; i < m; i++)
                for (int j = 0 - m + 1; j < m; j++)
                {
                    int tx = x + i;
                    int ty = y + j;

                    height = heightMap[i + m - 1, j + m - 1];

                    for (int a = 0; a < 4; a++)
                    {
                        dir = a;
                        offset = GetOffsetByDirection(tx, ty, a);

                        try
                        {
                            if (height > 0)
                                if (MeshFilter.sharedMesh.vertices[dir + offset].y > startValue) { veritces[dir + offset].y -= height; }
                                else
                                if (MeshFilter.sharedMesh.vertices[dir + offset].y < startValue) { veritces[dir + offset].y += height; }
                        }
                        catch { }
                    }
                }
            


            MeshFilter.sharedMesh.vertices = veritces;
            MeshFilter.sharedMesh.RecalculateNormals();
            MeshFilter.sharedMesh.RecalculateBounds();
            MeshFilter.sharedMesh.RecalculateTangents();
        }

        public void SetCornerHeight(int x, int y, float height, int size)
        {
            var mn = HighPolyModificator;
            var m = mn * size;

            int offset;
            int dir;

            x *= mn;
            y *= mn;

            Vector3[] veritces = MeshFilter.sharedMesh.vertices;

            for (int i = 0 - m + 1; i < m; i++)
                for (int j = 0 - m + 1; j < m; j++)
                {
                    int tx = x + i;
                    int ty = y + j;

                    for (int a = 0; a < 4; a++)
                    {
                        dir = a;
                        offset = GetOffsetByDirection(tx, ty, a);
                        try
                        {
                            veritces[dir + offset] = new Vector3(veritces[dir + offset].x, height, veritces[dir + offset].z);
                        }
                        catch { }
                    }
                }

            MeshFilter.sharedMesh.vertices = veritces;
            MeshFilter.sharedMesh.RecalculateNormals();
            MeshFilter.sharedMesh.RecalculateBounds();
            MeshFilter.sharedMesh.RecalculateTangents();
        }

        public float GetVerticeHeight(int x, int y, int dir = 0)
        {
            var m = HighPolyModificator;
            x *= m;
            y *= m;
            int offset = GetOffsetByDirection(x, y, dir);
            try
            {
                return MeshFilter.sharedMesh.vertices[offset].y;
            }
            catch
            {
                return 0;
            }
        }

        private int GetOffsetByDirection(int x, int y, int dir)
        {
            switch (dir)
            {
                case 0:
                    return x * 4 + (y * 4 * PlaneWidth * HighPolyModificator);
                case 1:
                    return (x - 1) * 4 + (y * 4 * PlaneWidth * HighPolyModificator);
                case 2:
                    return x * 4 + ((y - 1) * 4 * PlaneWidth * HighPolyModificator);
                case 3:
                    return (x - 1) * 4 + ((y - 1) * 4 * PlaneWidth * HighPolyModificator);
            }

            throw new System.Exception("Invalid Direction");
        }

        MeshFilter CreatePlane(MeshFilter meshFilter)
        {
            Init();

            var x = HighPolyModificator;
            var mf = meshFilter;
            var mesh = new Mesh();
            mf.sharedMesh = mesh;

            mesh.name = "Terrain Mesh";

            var tempWidth = TileWidth / x;
            var tempHeight = TileHeight / x;

            var vertices = new Vector3[4 * PlaneWidth * x * PlaneHeight * x];
            var tri = new int[6 * PlaneWidth * x * PlaneHeight * x];
            var normals = new Vector3[4 * PlaneWidth * x * PlaneHeight * x];
            var uv = new Vector2[4 * PlaneWidth * x * PlaneHeight * x];

            // Creating a main mesh
            for (int j = 0; j < PlaneHeight * x; j++)
            {
                for (int i = 0; i < PlaneWidth * x; i++)
                {
                    int offset = i * 4 + (j * 4 * PlaneWidth * x);
                    int triOffset = i * 6 + (j * 6 * PlaneWidth * x);
                    float widthOffset = tempWidth - TileWidth / x;
                    float heightOffset = tempHeight - TileHeight / x;

                    vertices[0 + offset] = new Vector3(widthOffset, 0, heightOffset);
                    vertices[1 + offset] = new Vector3(tempWidth, 0, heightOffset);
                    vertices[2 + offset] = new Vector3(widthOffset, 0, tempHeight);
                    vertices[3 + offset] = new Vector3(tempWidth, 0, tempHeight);

                    tri[0 + triOffset] = 0 + offset;
                    tri[1 + triOffset] = 2 + offset;
                    tri[2 + triOffset] = 1 + offset;

                    tri[3 + triOffset] = 2 + offset;
                    tri[4 + triOffset] = 3 + offset;
                    tri[5 + triOffset] = 1 + offset;

                    uv[0 + offset] = Vector2.zero;
                    uv[1 + offset] = Vector2.zero;
                    uv[2 + offset] = Vector2.zero;
                    uv[3 + offset] = Vector2.zero;

                    tempWidth += TileWidth / x;
                }
                tempWidth = TileWidth / x;
                tempHeight += TileHeight / x;
            }

            mesh.vertices = vertices;
            mesh.triangles = tri;
            mesh.normals = normals;
            mesh.uv = uv;
            mesh.uv2 = uv;
            mesh.uv3 = uv;
            mesh.uv4 = uv;

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();

            return mf;
        }
    }

}