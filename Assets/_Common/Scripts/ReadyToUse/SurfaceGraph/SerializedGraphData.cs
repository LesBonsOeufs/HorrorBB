using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using AYellowpaper.SerializedCollections;

namespace Root
{
    [Serializable]

    /// <summary>
    /// 1: Allows serializing GraphPoint's HashSet.
    /// 2: Stores neighbors as ids when serialized, for preventing excessive serialization depth
    /// </summary>
    public class SerializedGraphData : ISerializationCallbackReceiver
    {
        [field: SerializeField] public List<GraphPoint> Points { get; private set; }

        public SerializedGraphData(List<GraphPoint> data)
        {
            Points = data;
        }

        #region Serializer

        [SerializeField, HideInInspector] private SerializedDictionary<GraphPoint, int> pointsIds;
        [SerializeField, HideInInspector] private SerializedDictionary<int, List<int>> neighborsSerializer;
        public void OnBeforeSerialize()
        {
            pointsIds = new();

            for (int i = Points.Count - 1; i >= 0; i--)
                pointsIds.Add(Points[i], i);

            neighborsSerializer = new();

            foreach (GraphPoint lPoint in Points)
            {
                neighborsSerializer.Add(pointsIds[lPoint],
                    lPoint.neighbors.Select(neighbor => pointsIds[neighbor]).ToList());
            }
        }

        public void OnAfterDeserialize()
        {
            Dictionary<int, GraphPoint> lIdsPoints = pointsIds.ToDictionary(key => key.Value, value => value.Key);

            foreach (KeyValuePair<GraphPoint, int> lKeyPair in pointsIds)
            {
                lKeyPair.Key.neighbors = neighborsSerializer[lKeyPair.Value]
                    .Select(id => lIdsPoints[id]).ToHashSet();
            }

            Points = pointsIds.Keys.ToList();
        }

        #endregion
    }
}