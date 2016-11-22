/*
http://www.cgsoso.com/forum-257-1.html

CG搜搜 Unity3d 插件团购

CGSOSO 主打游戏开发，影视设计等CG资源素材。

每日Unity3d插件免费更新，仅限下载试用，如若商用，请务必官网购买！
*/

using UnityEngine;
using System.Collections;

public class OffsetOnNormal : MonoBehaviour
{
  public float offset = 1;
  public GameObject offsetGameObject;

  private Vector3 startPosition;

  void Awake()
  {
    startPosition = transform.position;
  }

	// Use this for initialization
	void OnEnable () {
    RaycastHit verticalHit;
    Physics.Raycast(startPosition, Vector3.down, out verticalHit);
    if(offsetGameObject!=null) transform.position = offsetGameObject.transform.position + verticalHit.normal * offset;
    else {
      transform.position = verticalHit.point + verticalHit.normal * offset;
    }
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
