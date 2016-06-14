/*
http://www.cgsoso.com/forum-257-1.html

CG搜搜 Unity3d 插件团购

CGSOSO 主打游戏开发，影视设计等CG资源素材。

每日Unity3d插件免费更新，仅限下载试用，如若商用，请务必官网购买！
*/

using UnityEngine;
using System.Collections;

public class DebuffOnEnemyFromCollision : MonoBehaviour
{

  public EffectSettings EffectSettings;
  public GameObject Effect;
	// Use this for initialization
	void Start () {
    EffectSettings.CollisionEnter += EffectSettings_CollisionEnter;
	}

  void EffectSettings_CollisionEnter(object sender, CollisionInfo e)
  {
    if (Effect==null)
      return;
    var colliders = Physics.OverlapSphere(transform.position, EffectSettings.EffectRadius, EffectSettings.LayerMask);
    foreach (var coll in colliders) {
      var hitGO = coll.transform;
      var renderer = hitGO.GetComponentInChildren<Renderer>();
      var effectInstance = Instantiate(Effect) as GameObject;
      effectInstance.transform.parent = renderer.transform;
      effectInstance.transform.localPosition = Vector3.zero;
      effectInstance.GetComponent<AddMaterialOnHit>().UpdateMaterial(coll.transform);
    }
  }
	
	// Update is called once per frame
	void Update () {
	
	}
}
