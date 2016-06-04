
public struct InfluenceMapSection {
    public InfluenceMapNode[] nodes;
    public int length;

    public InfluenceMapSection(int expectedLength) {
        nodes = new InfluenceMapNode[expectedLength];
        length = 0;
    }

    public void Clear() {
        for (int i = 0; i < nodes.Length; i++) {
            nodes[i].influence = 0;
        }
    }
}

