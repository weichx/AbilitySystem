using UnityEngine;
using KDTree;
using System.Collections.Generic;

public class AINodeManager : MonoBehaviour {

    const string objectName = "AINodeRoot";
    public AINode[] nodes;
    public bool drawNodes;
    protected KDTree<AINode> kdTree;
    private static AINodeManager _instance;

    void Awake() {
        if (_instance != null && _instance != this) {
            throw new System.Exception("Multiple AINodeRoots are not allowed");
        }
        BuildNeighbors();
        kdTree = new KDTree<AINode>(3);
        for(int i = 0; i < nodes.Length; i++) {
            Vector3 position = nodes[i].position;
            var point = new double[] { position.x, position.y, position.z };
            kdTree.AddPoint(point, nodes[i]);
        }
    }

    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            Ray worldRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if(Physics.Raycast(worldRay, out hit)) {
                Vector3 worldPoint = hit.point;
                AINode nearest = NearestNode(hit.point);
                if (nearest != null) {
                    nearest.color = Color.blue;
                    nearest.selected = true;
                }
            }
        }
    }

    public static AINodeManager Instance {
        get {
            if (_instance == null) {
                var obj = GameObject.Find(objectName) ?? new GameObject(objectName);
                _instance = obj.GetComponent<AINodeManager>() ?? obj.AddComponent<AINodeManager>();
            }
            return _instance;
        }
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
            Gizmos.DrawSphere(nodes[i].position, 1f);
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
