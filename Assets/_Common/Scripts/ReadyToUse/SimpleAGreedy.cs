using System;
using System.Collections.Generic;

public static class SimpleAGreedy<TSpaceType> where TSpaceType : IEquatable<TSpaceType>
{
    public static List<TSpaceType> Execute(TSpaceType origin, TSpaceType target,
            Func<TSpaceType, IEnumerable<TSpaceType>> getNeighborsFunc,
            Func<TSpaceType, bool> isWalkableFunc,
            Func<TSpaceType, TSpaceType, float> heuristicFunc)
    {
        if (!isWalkableFunc(origin) || !isWalkableFunc(target))
            return null;

        var lOpenSet = new SimplePriorityQueue<PathTile>();
        var lAllTiles = new Dictionary<TSpaceType, PathTile>();
        var lStartTile = new PathTile(origin, null, 0, heuristicFunc(origin, target));

        lOpenSet.Enqueue(lStartTile, lStartTile.FScore);
        lAllTiles[origin] = lStartTile;

        while (lOpenSet.Count > 0)
        {
            PathTile lCurrentTile = lOpenSet.Dequeue();

            if (lCurrentTile.Position.Equals(target))
                return lCurrentTile.GetPath();

            foreach (TSpaceType lNeighbor in getNeighborsFunc(lCurrentTile.Position))
            {
                if (!isWalkableFunc(lNeighbor))
                    continue;

                float lTentativeGScore = lCurrentTile.GScore + 1; // Assuming uniform cost of 1

                if (!lAllTiles.TryGetValue(lNeighbor, out PathTile lNeighborTile))
                {
                    lNeighborTile = new PathTile(lNeighbor, lCurrentTile, lTentativeGScore, heuristicFunc(lNeighbor, target));
                    lAllTiles[lNeighbor] = lNeighborTile;
                    lOpenSet.Enqueue(lNeighborTile, lNeighborTile.FScore);
                }
                else if (lTentativeGScore < lNeighborTile.GScore)
                {
                    lNeighborTile.UpdateTile(lCurrentTile, lTentativeGScore);
                    // Re-enqueue with updated priority
                    lOpenSet.Enqueue(lNeighborTile, lNeighborTile.FScore);
                }
            }
        }

        return null; // No path found
    }

    private class PathTile : IComparable<PathTile>
    {
        public TSpaceType Position { get; }
        public PathTile PreviousTile { get; private set; }
        public float GScore { get; private set; }
        public float FScore { get; private set; }

        public PathTile(TSpaceType position, PathTile previousTile, float gScore, float hScore)
        {
            Position = position;
            PreviousTile = previousTile;
            GScore = gScore;
            FScore = gScore + hScore;
        }

        public void UpdateTile(PathTile previousTile, float gScore)
        {
            float lHScore = FScore - GScore;  // Extract the heuristic score
            PreviousTile = previousTile;
            GScore = gScore;
            FScore = GScore + lHScore;  // Recalculate FScore with the new GScore
        }

        public int CompareTo(PathTile other) => FScore.CompareTo(other.FScore);

        public List<TSpaceType> GetPath()
        {
            List<TSpaceType> lPath = new();
            PathTile lCurrent = this;

            while (lCurrent != null)
            {
                lPath.Add(lCurrent.Position);
                lCurrent = lCurrent.PreviousTile;
            }

            lPath.Reverse();
            return lPath;
        }
    }
}