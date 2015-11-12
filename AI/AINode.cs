using UnityEngine;
using System;
using System.Collections.Generic;

public class AINodeBuilder : AINode {

    public List<AINodeBuilder> neighborList;

    public AINodeBuilder(int index, Vector3 position) : base(index, position) {
        neighborList = new List<AINodeBuilder>(8);
    }

    public void AddNeighbor(AINodeBuilder other) {
        neighborList.Add(other);
        other.neighborList.Add(this);
    }

    public void RemoveNeighbors() {
        for (int i = 0; i < neighborList.Count; i++) {
            neighborList[i].neighborList.Remove(this);
        }
        neighborList.Clear();
    }

    public AINode BuildAINode() {
        AINode node = new AINode(index, position);
        node.groupId = groupId;
        node.groupOffset = (1 << groupOffset);
        node.sightTable = sightTable;
        node.neighborIndices = new int[neighborList.Count];
        for(int i = 0; i < neighborList.Count; i++) {
            node.neighborIndices[i] = neighborList[i].index;
        }
        return node;
    }
}

[Serializable]
public class AINode {
    public int index;
    public int groupId;
    public int groupOffset;
    public int[] sightTable;
    public int[] neighborIndices;
    public Vector3 position;

    //todo add some sort of state machine for debugging
    public Color color; //temp field
    public bool selected;//temp field

    [NonSerialized]
    public AINode[] neighbors;

    public AINode(int index, Vector3 position) {
        this.index = index;
        this.position = position;
        color = Color.white;
        neighbors = new AINode[0];
    }

    public void GatherNeighbors(AINode[] nodes) {
        neighbors = new AINode[neighborIndices.Length];
        for(int i = 0; i < neighbors.Length; i++) {
            neighbors[i] = nodes[neighborIndices[i]];
        }
    }

    public bool isNodeInSight(AINode node) {
        return (sightTable[node.groupId] & node.groupOffset) != 0;
    }

}