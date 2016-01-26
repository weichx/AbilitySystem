using UnityEngine;
using KDTree;
using System.Collections.Generic;
//using Vectrosity;

public class AINodeManager : MonoBehaviour {

    const string objectName = "AINodeRoot";
    public AINode[] nodes;
    public bool drawNodes;
    public float originX;
    public float originZ;
    protected KDTree<AINode> kdTree;
    private static AINodeManager _instance;

    void Awake() {
        if (_instance != null && _instance != this) {
            throw new System.Exception("Multiple AINodeRoots are not allowed");
        }
        BuildNeighbors();
        kdTree = new KDTree<AINode>(3);
        for (int i = 0; i < nodes.Length; i++) {
            Vector3 position = nodes[i].position;
            var point = new double[] { position.x, position.y, position.z };
            kdTree.AddPoint(point, nodes[i]);
        }
    }

    //void Update() {
    //    if (Input.GetMouseButtonDown(0)) {
    //        Ray worldRay = Camera.main.ScreenPointToRay(Input.mousePosition);
    //        RaycastHit hit;
    //        if(Physics.Raycast(worldRay, out hit)) {
    //            Vector3 worldPoint = hit.point;
    //            AINode nearest = NearestNode(hit.point);
    //            if (nearest != null) {
    //                nearest.color = Color.blue;
    //                nearest.selected = true;
    //            }
    //        }
    //    }
    //}

    public static AINodeManager Instance {
        get {
            if (_instance == null) {
                var obj = GameObject.Find(objectName) ?? new GameObject(objectName);
                _instance = obj.GetComponent<AINodeManager>() ?? obj.AddComponent<AINodeManager>();
            }
            return _instance;
        }
    }

    public static float OriginX { 
        get {return Instance.originX; }
    }

    public static float OriginZ {
        get { return Instance.originZ; }
    }

    public AINode NearestNode(Vector3 point, float range = -1) {
        var searchPoint = new double[] { point.x, point.y, point.z };
        var iterator = kdTree.NearestNeighbors(searchPoint, 1, range);
        iterator.MoveNext();
        return iterator.Current;
    }

    public List<AINode> NearestNodes(Vector3 point, int max, float range = -1) {
        var retn = new List<AINode>(max);
        var searchPoint = new double[] { point.x, point.y, point.z };
        var iterator = kdTree.NearestNeighbors(searchPoint, 1, range);
        while (iterator.MoveNext()) {
            retn.Add(iterator.Current);
        }
        return retn;
    }

    public List<AINode> NodesInRange(Vector3 point, float range) {
        var retn = new List<AINode>(100);
        var searchPoint = new double[] { point.x, point.y, point.z };
        var iterator = kdTree.NearestNeighbors(searchPoint, 1, range);
        while (iterator.MoveNext()) {
            retn.Add(iterator.Current);
        }
        return retn;
    }

    public NearestNeighbour<AINode> NearestNeighborIterator(Vector3 point, int max, float range = -1f) {
        var searchPoint = new double[] { point.x, point.y, point.z };
        return kdTree.NearestNeighbors(searchPoint, 1, range);
    }

    public static void SetNodes(AINode[] nodes) {
        Instance.nodes = nodes;
        Instance.BuildNeighbors();
    }

    private void BuildNeighbors() {
        if (nodes[0].neighbors == null || nodes[0].neighbors.Length == 0) {
            for (int i = 0; i < nodes.Length; i++) {
                nodes[i].GatherNeighbors(nodes);
            }
        }
    }

    void OnDrawGizmos() {
        if (!drawNodes || nodes == null) return;
        BuildNeighbors();
        //double drawing, but its just a debug view
        for (int i = 0; i < nodes.Length; i++) {
            Gizmos.color = nodes[i].color;
            Gizmos.DrawWireCube(nodes[i].position, Vector3.one);
            AINode[] neighbors = nodes[i].neighbors;
            for (int j = 0; j < neighbors.Length; j++) {
                Gizmos.DrawLine(nodes[i].position, neighbors[j].position);
            }
        }

        for(int i = 0; i < nodes.Length; i++) {
            AINode node = nodes[i];
            if(node.selected) {
                AINode[] neighbors = nodes[i].neighbors;
                Gizmos.color = node.color;
                for (int j = 0; j < neighbors.Length; j++) {
                    Gizmos.DrawLine(nodes[i].position, neighbors[j].position);
                }
            }
        }
    }

}

/*
Minimum Requirements to start AI

Nodes annotated with cover
pathfinding
annotated choke points

    start with the brute since its the easiest


Influence Map
    Project each unit's influence

    Tension region focal points (if more than one)
    Vulnerability Map
    Scout Map


Tactically Important Terrain

In / Near Cover (from what?, in general?)
Flank Protection
Wide range of view
Lower Visibility (Trees)?
Special (Marked in editor)
Line of Sight
Line of Fire
Focus

States
    Scout
    Combat
    Recovery
    Survive

General Engagement
    Camp
    Joust
    Circle Of Death
    Guerilla
    BackStabber
    Opportunist
    Fist Fight / Brute
    Peek-a-boo
    Rush - joust
    Sniper
            
Position change -- taken lots of damage, dealt little
var open = set of nodes to evaluate (priority queue)
var closed = set of nodes already evaluated

    add root node to open

    loop
        current = lowest f cost node in open set
        remove current from open
        add current to closed

        if current is target node return (path found)

        for each neighbor of current
            if not traversable || in closed set; continue

        if new path to neighbor is shorter or neighbor not in open
            set cost of neighbor
            set parent of neighbor to current
            if neighbor is not in open
                add neighbor to open



*/