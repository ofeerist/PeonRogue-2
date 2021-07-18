using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Tilemap
{
    [System.Serializable]
    public class Tile
    {
        public Cell Cell1;
        public Cell Cell2;
        public Cell Cell3;
        public Cell Cell4;
        public int X;
        public int Y;

        public float SavedHeight;

        public Tile(int x, int y)
        {
            X = x;
            Y = y;
        }

        public void SetCell(Cell cell, int index)
        {
            switch (index)
            {
                case 1:
                    Cell1 = cell;
                    break;
                case 2:
                    Cell2 = cell;
                    break;
                case 3:
                    Cell3 = cell;
                    break;
                case 4:
                    Cell4 = cell;
                    break;
                default:
                    throw new UnityException("Invalid index in SetCell");
            }
        }
        public Cell GetCell(int index)
        {
            switch (index)
            {
                case 1:
                    return Cell1;
                case 2:
                    return Cell2;
                case 3:
                    return Cell3; 
                case 4:
                    return Cell4;
                default:
                    throw new UnityException("Invalid index in SetCell");
            }
        }
    }
    [System.Serializable]
    public class TileMap
    {
        public Tile[,] Tiles;
        public TileMap(int width, int height)
        {
            Tiles = new Tile[width, height];

            for (int x = 0; x < width; x++)  
                for (int y = 0; y < height; y++)
                    Tiles[x, y] = new Tile(x, y);
        }
    }
}
