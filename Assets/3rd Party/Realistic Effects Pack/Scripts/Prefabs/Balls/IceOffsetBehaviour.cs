/*
http://www.cgsoso.com/forum-257-1.html

CG搜搜 Unity3d 插件团购

CGSOSO 主打游戏开发，影视设计等CG资源素材。

每日Unity3d插件免费更新，仅限下载试用，如若商用，请务必官网购买！
*/

using System;
using UnityEngine;
using System.Collections;

public class IceOffsetBehaviour : MonoBehaviour {

	// Use this for initialization
	void Start ()
	{
	  var fiof = GetComponent<FadeInOutShaderFloat>();
    if(fiof == null) return;
	  var parent = transform.parent;
	  var skinnedMesh = parent.GetComponent<SkinnedMeshRenderer>();
	  Mesh mesh;
    if (skinnedMesh != null) mesh = skinnedMesh.sharedMesh;
    else {
      var meshFilter = parent.GetComponent<MeshFilter>();
      if (meshFilter == null) return;
      mesh = meshFilter.sharedMesh;
    }
	  if (!mesh.isReadable) {
      fiof.MaxFloat = 0.0f;
      return;
	  }
	  var length = mesh.triangles.Length;
	  if (length < 1000) {
	    if (length > 500)
	      fiof.MaxFloat = length / 1000f - 0.5f;
	    else
	      fiof.MaxFloat = 0.0f;
	  }
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
