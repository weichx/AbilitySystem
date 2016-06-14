/*
http://www.cgsoso.com/forum-257-1.html

CG搜搜 Unity3d 插件团购

CGSOSO 主打游戏开发，影视设计等CG资源素材。

每日Unity3d插件免费更新，仅限下载试用，如若商用，请务必官网购买！
*/

using UnityEngine;
using System.Collections;

public class FlyDemo : MonoBehaviour
{

  public float Speed = 1;
  public float Height = 1;

  private Transform t;
  private float time;
	// Use this for initialization
	void Start ()
	{
	  t = transform;
	}
	
	// Update is called once per frame
	void Update ()
	{
	  time += Time.deltaTime;
    var sin = Mathf.Cos(time / Speed);
    t.localPosition = new Vector3(0, 0, sin*Height);
	}
}
