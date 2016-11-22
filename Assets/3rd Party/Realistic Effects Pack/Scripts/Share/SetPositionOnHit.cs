/*
http://www.cgsoso.com/forum-257-1.html

CG搜搜 Unity3d 插件团购

CGSOSO 主打游戏开发，影视设计等CG资源素材。

每日Unity3d插件免费更新，仅限下载试用，如若商用，请务必官网购买！
*/

using System.ComponentModel;
using UnityEngine;
using System.Collections;

public class SetPositionOnHit : MonoBehaviour
{

  public float OffsetPosition;

  private EffectSettings effectSettings;
  private Transform tRoot;
  private bool isInitialized;

  void GetEffectSettingsComponent(Transform tr)
  {
    var parent = tr.parent;
    if (parent != null)
    {
      effectSettings = parent.GetComponentInChildren<EffectSettings>();
      if (effectSettings == null)
        GetEffectSettingsComponent(parent.transform);
    }
  }

  void Start()
  {
    GetEffectSettingsComponent(transform);
    if (effectSettings==null)
      Debug.Log("Prefab root or children have not script \"PrefabSettings\"");
    tRoot = effectSettings.transform;
  }

  void effectSettings_CollisionEnter(object sender, CollisionInfo e)
  {
    var direction = (tRoot.position + Vector3.Normalize(e.Hit.point - tRoot.position) * (effectSettings.MoveDistance + 1)).normalized;
    transform.position = e.Hit.point - direction*OffsetPosition;
  }

  void Update()
  {
    if (!isInitialized) {
      isInitialized = true;
      effectSettings.CollisionEnter += effectSettings_CollisionEnter;
    }
  }
	// Update is called once per frame
	void OnDisable ()
	{
	  transform.position = Vector3.zero;
	}
}
