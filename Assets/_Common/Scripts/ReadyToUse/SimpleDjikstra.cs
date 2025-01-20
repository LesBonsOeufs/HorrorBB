using System;
using System.Collections.Generic;

/// <typeparam name="TSpaceType">Example: Vector2Int will allow using SimpleDjikstra for a 2D grid. Vector3Int, for a 3D grid.</typeparam>
public static class SimpleDjikstra<TSpaceType> where TSpaceType : IEquatable<TSpaceType>
{
    public static List<TSpaceType> Execute(TSpaceType origin, TSpaceType target, Func<TSpaceType, TSpaceType[]> getNeighborsFunc, Func<TSpaceType, bool> isWalkableFunc)
    {
        if (!isWalkableFunc(origin) || !isWalkableFunc(target))
            return null;

        List<PathTile> lTilesToTest = new() { new PathTile(origin, null) };
        List<PathTile> lNextTilesToTest = new();
        List<TSpaceType> lVisitedTiles = new() { origin };
        PathTile lPathTile;

        while (lTilesToTest.Count > 0)
        {
            for (int i = 0; i < lTilesToTest.Count; i++)
            {
                lPathTile = lTilesToTest[i];

                if (EqualityComparer<TSpaceType>.Default.Equals(lPathTile.position, target))
                    return lPathTile.GetPath();

                foreach (TSpaceType lNeighbor in getNeighborsFunc(lPathTile.position))
                {
                    if (!lVisitedTiles.Contains(lNeighbor) && isWalkableFunc(lNeighbor))
                    {
                        lNextTilesToTest.Add(new PathTile(lNeighbor, lPathTile));
                        lVisitedTiles.Add(lNeighbor);
                    }
                }
            }

            lTilesToTest = new(lNextTilesToTest);
            lNextTilesToTest.Clear();
        }

        return null;
    }

    private class PathTile
    {
        public TSpaceType position;
        public PathTile previousTile;

        public PathTile(TSpaceType position, PathTile previousTile)
        {
            this.position = position;
            this.previousTile = previousTile;
        }

        public List<TSpaceType> GetPath()
        {
            List<TSpaceType> lPath = new ();

            PathTile lPathTile = this;

            while (lPathTile != null)
            {
                lPath.Add(lPathTile.position);
                lPathTile = lPathTile.previousTile;
            }

            lPath.Reverse();
            return lPath;
        }
    }
}
