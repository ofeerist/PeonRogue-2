using System;
using UnityEngine;

namespace _Scripts.TerrainScripts
{
    [Serializable]
    public class Cell : IComparable
    {
        public int Tile;
        public int X;
        public int Y;
        public CornerPoints CornerPoints;

        public int CompareTo(object o)
        {
            var p = o as Cell;
            if (p != null)
                return Tile.CompareTo(p.Tile);
            else
                throw new Exception("Невозможно сравнить два объекта");
        }

        public bool CompareFully(Cell cell)
        {
            return Tile == cell.Tile && X == cell.X && Y == cell.Y;
        }

        public Cell(int tile, int x, int y)
        {
            Tile = tile;
            X = x;
            Y = y;

            CornerPoints = TileMapConstants.GetCornerPointsFromCell(this);
        }

        public Vector2[] GetChangedUV(Vector2[] _uv, int x, int y, int planeWidth, int highPolyMod, Vector2 _bf, float _ux, float _uy)
        {
            x *= highPolyMod;
            y *= highPolyMod;
            _ux /= highPolyMod;
            _uy /= highPolyMod;

            for (int i = 0; i < highPolyMod; i++)
                for (int j = 0; j < highPolyMod; j++)
                {
                    int tx = x + i;
                    int ty = y + j;

                    var tux = _ux * i;
                    var tuy = _uy * j;
                    var tbf = _bf + new Vector2(tux, tuy);

                    var halfPixelw = (_ux / 256) / 2;
                    var halfPixelh = (_uy / 256) / 2;

                    var tempVectors = new Vector2[4];
                    tempVectors[0] = tbf + new Vector2(0 + halfPixelw, 0 + halfPixelh);
                    tempVectors[1] = tbf + new Vector2(_ux - halfPixelw, 0 + halfPixelh);
                    tempVectors[2] = tbf + new Vector2(0 + halfPixelw, _uy - halfPixelh);
                    tempVectors[3] = tbf + new Vector2(_ux - halfPixelw, _uy - halfPixelh);

                    int offset = tx * 4 + (ty * 4 * planeWidth * highPolyMod);
                    _uv[0 + offset] = tempVectors[0]; // bl
                    _uv[1 + offset] = tempVectors[1]; // br
                    _uv[2 + offset] = tempVectors[2]; // tl
                    _uv[3 + offset] = tempVectors[3]; // tr
                }
            
            return _uv;
        }
    }
}