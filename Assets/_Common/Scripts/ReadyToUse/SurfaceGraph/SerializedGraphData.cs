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
        [SerializeField, HideInInspector] private SerializedDictionary<GraphPoint, int> pointsIds;
        [SerializeField, HideInInspector] private SerializedDictionary<int, List<int>> neighborsSerializer;

        public void SetPoints(List<GraphPoint> data)
        {
            pointsIds = new();

            for (int i = data.Count - 1; i >= 0; i--)
                pointsIds.Add(data[i], i);
        }

        public List<GraphPoint> GetPoints() => pointsIds.Keys.ToList();
        
        public void OnBeforeSerialize()
        {
            neighborsSerializer = new();

            foreach (KeyValuePair<GraphPoint, int> lKeyPair in pointsIds)
            {
                neighborsSerializer.Add(lKeyPair.Value,
                    lKeyPair.Key.neighbors.Select(neighbor => pointsIds[neighbor]).ToList());
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
        }
    }
}