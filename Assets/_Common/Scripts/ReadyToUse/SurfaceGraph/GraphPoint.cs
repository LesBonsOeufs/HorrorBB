using System.Collections.Generic;
using System;
using UnityEngine;

namespace Root
{
    [Serializable]
    public class GraphPoint : IEquatable<GraphPoint>
    {
        public Vector3 position;
        public Vector3 normal;

        //Should never be serialized.
        //Would create excessive serialization depth, and crash the editor / build.
        [NonSerialized]
        public HashSet<GraphPoint> neighbors;

        public GraphPoint(Vector3 position, Vector3 normal)
        {
            this.position = position;
            this.normal = normal;
            neighbors = new();
        }

        public bool Equals(GraphPoint other)
        {
            return position == other.position;
        }
    }
}