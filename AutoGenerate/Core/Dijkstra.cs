using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Auto.Dijkstra
{
    public class Dijkstra
    {
/*        public Vector3 FindPath(IEnumerable<Node> nodes,Node start,Node end)
        {
            start.SetWeight(0);
            
            var nodeList = nodes.ToList();
            for (var i = 0; i < nodeList.Count(); i++)
            {
                var node = nodeList[i];         
                
                if(!node.IsWalkable() || node.GetWeight() < 0)
                    continue;

                for (var p = 0; p < node.GetNeighborsCount(); p++)
                {
                    var neighbor = node.GetNeighbor(p);

                    foreach (var data in neighbor.GetNeighborNodes())
                    {
                        var weight = Utility.Distance3D(neighbor.GetPosition(), data.GetPosition());
                        data.SetWeight(weight);
                        Debug.Log(weight.ToString());
                    }
                }
            }
            
            return Vector3.zero;
        }*/
    }
}
