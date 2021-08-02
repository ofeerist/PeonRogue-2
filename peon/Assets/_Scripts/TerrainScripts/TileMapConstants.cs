
using UnityEngine;

namespace _Scripts.TerrainScripts
{
    [System.Serializable]
    public class CornerPoints
    {
        public bool TR;
        public bool TL;
        public bool BR;
        public bool BL;

        public CornerPoints(bool tr, bool tl, bool br, bool bl)
        {
            TR = tr;
            TL = tl;
            BR = br;
            BL = bl;
        }

        public override bool Equals(object obj)
        {
            if (obj is CornerPoints cp)
                return TR == cp.TR && TL == cp.TL && BR == cp.BR && BL == cp.BL;
            else
                return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return TR + ", " + TL + ", " + BR + ", " + BL;
        }

        public CornerPoints Summarize(CornerPoints cornerPoints)
        {
            var cp = new CornerPoints(false, false, false, false);
            cp.TR = cornerPoints.TR || TR;
            cp.TL = cornerPoints.TL || TL;
            cp.BR = cornerPoints.BR || BR;
            cp.BL = cornerPoints.BL || BL;
            return cp;
        }

        public CornerPoints Subtract(CornerPoints cornerPoints)
        {
            var cp = new CornerPoints(false, false, false, false);
            cp.TR = TR && cornerPoints.TR ? false : TR;
            cp.TL = TL && cornerPoints.TL ? false : TL;
            cp.BR = BR && cornerPoints.BR ? false : BR;
            cp.BL = BL && cornerPoints.BL ? false : BL;
            return cp;
        }
    }

    static class TileMapConstants
    {
        public static Cell GetCellFromCornerPoints(CornerPoints cornerPoints, int tileIndex, int lastIndex)
        {
            if (cornerPoints.Equals(new CornerPoints(true, true, true, true))) // full rects
            {
                if(new Cell(tileIndex, 0, 4).CompareFully(new Cell(lastIndex, 0, 7)))
                    return new Cell(tileIndex, Random.Range(0, 4), Random.Range(4, 8));
                return new Cell(tileIndex, 0, 3);
            }

            if (cornerPoints.Equals(new CornerPoints(true, true, false, false))) // semi rects
            {
                return new Cell(tileIndex, 0, 0);
            }
            else if (cornerPoints.Equals(new CornerPoints(false, false, true, true)))
            {
                return new Cell(tileIndex, 3, 3);
            }
            else if (cornerPoints.Equals(new CornerPoints(true, false, true, false)))
            {
                return new Cell(tileIndex, 1, 2);
            }
            else if (cornerPoints.Equals(new CornerPoints(false, true, false, true)))
            {
                return new Cell(tileIndex, 2, 1);
            }

            if (cornerPoints.Equals(new CornerPoints(false, true, false, false))) // empty corners
            {
                return new Cell(tileIndex, 0, 1);
            }
            else if (cornerPoints.Equals(new CornerPoints(true, false, false, false)))
            {
                return new Cell(tileIndex, 0, 2);
            }
            else if (cornerPoints.Equals(new CornerPoints(false, false, true, false)))
            {
                return new Cell(tileIndex, 1, 3);
            }
            else if (cornerPoints.Equals(new CornerPoints(false, false, false, true)))
            {
                return new Cell(tileIndex, 2, 3);
            }

            if (cornerPoints.Equals(new CornerPoints(true, true, true, false))) // fill corners
            {
                return new Cell(tileIndex, 1, 0);
            }
            else if (cornerPoints.Equals(new CornerPoints(true, true, false, true)))
            {
                return new Cell(tileIndex, 2, 0);
            }
            else if (cornerPoints.Equals(new CornerPoints(false, true, true, true)))
            {
                return new Cell(tileIndex, 3, 1);
            }
            else if (cornerPoints.Equals(new CornerPoints(true, false, true, true)))
            {
                return new Cell(tileIndex, 3, 2);
            }

            if (cornerPoints.Equals(new CornerPoints(false, true, true, false))) // tunnels
            {
                return new Cell(tileIndex, 1, 1);
            }
            else if (cornerPoints.Equals(new CornerPoints(true, false, false, true)))
            {
                return new Cell(tileIndex, 2, 2);
            }

            return null; 
        }

        public static CornerPoints GetCornerPointsFromCell(Cell cell)
        {
            int x = cell.X;
            int y = cell.Y;

            if ( ((x == 0 && y == 3) || (x == 3 && y == 0)) || (y > 3)) // full rects
            {
                return new CornerPoints(true, true, true, true);
            }
            
            if (x == 0 && y == 0) // semi rects
            {
                return new CornerPoints(true, true, false, false);
            }
            else if (x == 3 && y == 3)
            {
                return new CornerPoints(false, false, true, true);
            }
            else if (x == 1 && y == 2)
            {
                return new CornerPoints(true, false, true, false);
            }
            else if (x == 2 && y == 1)
            {
                return new CornerPoints(false, true, false, true);
            }

            if (x == 0 && y == 1) // empty corners
            {
                return new CornerPoints(false, true, false, false);
            }
            else if (x == 0 && y == 2) 
            {
                return new CornerPoints(true, false, false, false);
            }
            else if (x == 1 && y == 3)
            {
                return new CornerPoints(false, false, true, false);
            }
            else if (x == 2 && y == 3)
            {
                return new CornerPoints(false, false, false, true);
            }

            if (x == 1 && y == 0) // fill corners
            {
                return new CornerPoints(true, true, true, false);
            }
            else if (x == 2 && y == 0) 
            {
                return new CornerPoints(true, true, false, true);
            }
            else if (x == 3 && y == 1)
            {
                return new CornerPoints(false, true, true, true);
            }
            else if (x == 3 && y == 2)
            {
                return new CornerPoints(true, false, true, true);
            }

            if (x == 1 && y == 1) // tunnels
            {
                return new CornerPoints(false, true, true, false);
            }
            else if (x == 2 && y == 2)
            {
                return new CornerPoints(true, false, false, true);
            }

            return null;
        }

    }
}
