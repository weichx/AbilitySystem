/*
http://www.cgsoso.com/forum-257-1.html

CG搜搜 Unity3d 插件团购

CGSOSO 主打游戏开发，影视设计等CG资源素材。

每日Unity3d插件免费更新，仅限下载试用，如若商用，请务必官网购买！
*/

using System;
using UnityEngine;
using System.Collections;

public class MoveOnGround : MonoBehaviour
{
  public event EventHandler<CollisionInfo> OnCollision;
  public bool IsRootMove = true;

  private EffectSettings effectSettings;
  private Transform tRoot, tTarget;
  private Vector3 targetPos;
  private bool isInitialized;
  private bool isFinished;
  private ParticleSystem[] particles;

  private void GetEffectSettingsComponent(Transform tr)
  {
    var parent = tr.parent;
    if (parent!=null) {
      effectSettings = parent.GetComponentInChildren<EffectSettings>();
      if (effectSettings==null)
        GetEffectSettingsComponent(parent.transform);
    }
  }

  private void Start()
  {
    GetEffectSettingsComponent(transform);
    if (effectSettings==null)
      Debug.Log("Prefab root have not script \"PrefabSettings\"");
    particles = effectSettings.GetComponentsInChildren<ParticleSystem>();

    InitDefaultVariables();
    isInitialized = true;
  }

  void OnEnable()
  {
    if(isInitialized) InitDefaultVariables();
  }

  private void InitDefaultVariables()
  {
    foreach (var particle in particles) {
      particle.Stop();
    }
    isFinished = false;
    tTarget = effectSettings.Target.transform;
    if (IsRootMove)
      tRoot = effectSettings.transform;
    else {
      tRoot = transform.parent;
      tRoot.localPosition = Vector3.zero;
    }

    targetPos = tRoot.position + Vector3.Normalize(tTarget.position - tRoot.position) * effectSettings.MoveDistance;
    RaycastHit verticalHit;
    Physics.Raycast(tRoot.position, Vector3.down, out verticalHit);
    tRoot.position = verticalHit.point;
    foreach (var particle in particles)
    {
      particle.Play();
    }
  }

  private void Update()
  {
    if (tTarget==null || isFinished)
      return;
    var pos = tRoot.position;
    RaycastHit verticalHit;
    Physics.Raycast(new Vector3(pos.x, 0.5f, pos.z), Vector3.down, out verticalHit);
    tRoot.position = verticalHit.point;
    pos = tRoot.position;

    var endPoint = effectSettings.IsHomingMove ? tTarget.position : targetPos;
    var pointOnGround = new Vector3(endPoint.x, 0, endPoint.z);
    if (Vector3.Distance(new Vector3(pos.x, 0, pos.z), pointOnGround) <= effectSettings.ColliderRadius) {
      effectSettings.OnCollisionHandler(new CollisionInfo());
      isFinished = true;
    }
    tRoot.position = Vector3.MoveTowards(pos, pointOnGround, effectSettings.MoveSpeed * Time.deltaTime);
  }
}
