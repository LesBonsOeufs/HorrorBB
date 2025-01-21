using System;
using System.Collections.Generic;

public static class SimpleAGreedy<TSpaceType> where TSpaceType : IEquatable<TSpaceType>
{
    public static List<TSpaceType> Execute(TSpaceType origin, TSpaceType target,
            Func<TSpaceType, TSpaceType[]> getNeighborsFunc,
            Func<TSpaceType, bool> isWalkableFunc,
            Func<TSpaceType, TSpaceType, float> heuristicFunc)
    {
        if (!isWalkableFunc(origin) || !isWalkableFunc(target))
            return null;

        var lOpenSet = new SimplePriorityQueue<PathTile>();
        var lClosedSet = new HashSet<TSpaceType>();
        var lStartTile = new PathTile(origin, null, 0, heuristicFunc(origin, target));

        lOpenSet.Enqueue(lStartTile, lStartTile.fScore);

        while (lOpenSet.Count > 0)
        {
            PathTile currentTile = lOpenSet.Dequeue();

            if (EqualityComparer<TSpaceType>.Default.Equals(currentTile.position, target))
                return currentTile.GetPath();

            lClosedSet.Add(currentTile.position);

            foreach (TSpaceType neighbor in getNeighborsFunc(currentTile.position))
            {
                if (!isWalkableFunc(neighbor) || lClosedSet.Contains(neighbor))
                    continue;

                float lTentativeGScore = currentTile.gScore + 1; // Assuming uniform cost of 1
                float lHScore = heuristicFunc(neighbor, target);
                PathTile lNeighborTile = new PathTile(neighbor, currentTile, lTentativeGScore, lHScore);

                if (!lOpenSet.Contains(lNeighborTile) || lTentativeGScore < lNeighborTile.gScore)
                {
                    lOpenSet.Enqueue(lNeighborTile, lNeighborTile.fScore);
                }
            }
        }

        return null; // No path found
    }

    private class PathTile : IComparable<PathTile>
    {
        public TSpaceType position;
        public PathTile previousTile;
        public float gScore;
        public float fScore;

        public PathTile(TSpaceType position, PathTile previousTile, float gScore, float hScore)
        {
            this.position = position;
            this.previousTile = previousTile;
            this.gScore = gScore;
            this.fScore = gScore + hScore; // f(n) = g(n) + h(n)
        }

        public List<TSpaceType> GetPath()
        {
            var path = new List<TSpaceType>();
            var current = this;

            while (current != null)
            {
                path.Add(current.position);
                current = current.previousTile;
            }

            path.Reverse();
            return path;
        }

        public int CompareTo(PathTile other)
        {
            return fScore.CompareTo(other.fScore);
        }
    }
}