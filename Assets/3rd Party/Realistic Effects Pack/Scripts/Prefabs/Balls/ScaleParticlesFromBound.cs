/*
http://www.cgsoso.com/forum-257-1.html

CG搜搜 Unity3d 插件团购

CGSOSO 主打游戏开发，影视设计等CG资源素材。

每日Unity3d插件免费更新，仅限下载试用，如若商用，请务必官网购买！
*/

using UnityEngine;
using System.Collections;

public class ScaleParticlesFromBound : MonoBehaviour
{

  private Collider targetCollider;

  void GetMeshFilterParent(Transform t)
  {
    var coll = t.parent.GetComponent<Collider>();
    if (coll == null)
      GetMeshFilterParent(t.parent);
    else
      targetCollider = coll;
  }

	// Use this for initialization
	void Start ()
	{
	  GetMeshFilterParent(transform);
    if (targetCollider == null) return;
	  var boundSize = targetCollider.bounds.size;
	  transform.localScale = boundSize;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
