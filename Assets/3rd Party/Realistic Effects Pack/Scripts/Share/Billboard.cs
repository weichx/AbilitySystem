/*
http://www.cgsoso.com/forum-257-1.html

CG搜搜 Unity3d 插件团购

CGSOSO 主打游戏开发，影视设计等CG资源素材。

每日Unity3d插件免费更新，仅限下载试用，如若商用，请务必官网购买！
*/

using UnityEngine;
using System.Collections;

public class Billboard : MonoBehaviour
{
  public Camera Camera;
  public bool Active = true;
  public bool AutoInitCamera = true;

  private GameObject myContainer;
  private Transform t, camT, contT;

  private void Awake()
  {
    if (AutoInitCamera) {
      Camera = Camera.main;
      Active = true;
    }

    t = transform;
    camT = Camera.transform;
    var parent = t.parent;
    myContainer = new GameObject { name = "Billboard_" + t.gameObject.name };
    contT = myContainer.transform;
    contT.position = t.position;
    t.parent = myContainer.transform;
    contT.parent = parent;
  }

  private void Update()
  {
    if (Active)
      contT.LookAt(contT.position + camT.rotation * Vector3.back, camT.rotation * Vector3.up);
  }
}
