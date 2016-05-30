using UnityEngine;
using UnityEditor;
using Intelligence;
using System;

public class V {
    public float value0;
}

public class V1 : V {
    public float value1;
}

public class EvaluatorPage_GeneralSection : EvaluatorPage_SectionBase {
    private class Vec {
        public int i;
        public float x;
        public float y;
        public float z;
        public string str;
        public Vector3 vec;
        public Color c;
        public bool toggle;
        public TextureFormat format;
        public Vector2I myvec;
        public GameObject obj;
        public Entity ent;
        public string[] list;
        public AnimationCurve curve;
        public Bounds bounds;
        public V[] vectors;
    }

	private Type[] contextTypes;
	private string[] contextTypeNames;
    public Bounds bounds;
    public SerializedObjectX root;
   // private Vec vec;

	public EvaluatorPage_GeneralSection() {
	    //vec = new Vec();
     //   vec.vectors = new V[2];
     //   vec.vectors[0] = new V();
	    //vec.vectors[1] = new V1();

     //   root = new SerializedObjectX(vec);
        contextTypes = Reflector.FindSubClasses<Context>(true).ToArray();
		contextTypeNames = new string[contextTypes.Length];
		for(int i = 0; i < contextTypes.Length; i++) {
			contextTypeNames[i] = Util.SplitAndTitlize(contextTypes[i].Name);
		}
	}

	public override void Render() {
		if(targetItem == null) return;
	    //EditorGUILayoutX.DrawProperties(root);
	//	EditorGUILayout.Popup("Context", 0, contextTypeNames);
	}

}
