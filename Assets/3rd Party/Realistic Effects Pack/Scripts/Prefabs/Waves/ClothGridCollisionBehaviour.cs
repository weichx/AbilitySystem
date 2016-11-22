using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using System.Collections;
using Debug = UnityEngine.Debug;

public class ClothGridCollisionBehaviour : MonoBehaviour
{
  public GameObject[] AttachedPoints;
  
  public bool IsLookAt;

  public event EventHandler<CollisionInfo> OnCollision;

  private EffectSettings effectSettings;
  private Transform tRoot, tTarget;
  private Vector3 targetPos;
  private bool onCollision;

  private void GetEffectSettingsComponent(Transform tr)
  {
    var parent = tr.parent;
    if (parent != null)
    {
      effectSettings = parent.GetComponentInChildren<EffectSettings>();
      if (effectSettings == null)
        GetEffectSettingsComponent(parent.transform);
    }
  }
  // Use this for initialization
  private void Start()
  {
    GetEffectSettingsComponent(transform);
    if (effectSettings==null)
      Debug.Log("Prefab root have not script \"PrefabSettings\"");
    tRoot = effectSettings.transform;
    InitDefaultVariables();
  }

  private void InitDefaultVariables()
  {
    tTarget = effectSettings.Target.transform;
    if (IsLookAt)
      tRoot.LookAt(tTarget);

    var point = CenterPoint();
    targetPos = point + Vector3.Normalize(tTarget.position - point) * effectSettings.MoveDistance;
    var force = Vector3.Normalize(tTarget.position - point) * effectSettings.MoveSpeed / 100f;
    for (int i = 0; i < AttachedPoints.Length; i++) {
      var ap = AttachedPoints[i];
      var rig = ap.GetComponent<Rigidbody>();
      rig.useGravity = false;
      rig.AddForce(force, ForceMode.Impulse);
      ap.SetActive(true);
    }
    
  }

  Vector3 CenterPoint()
  {
    return (transform.TransformPoint(AttachedPoints[0].transform.localPosition) + transform.TransformPoint(AttachedPoints[2].transform.localPosition)) / 2;
  }

  private void Update()
  {
    if (tTarget == null || onCollision)
      return;
   
    var point = CenterPoint();
    var raycastHit = new RaycastHit();
    var distance = Vector3.Distance(point, targetPos);Debug.DrawLine(point, targetPos);
    var distanceNextFrame = effectSettings.MoveSpeed * Time.deltaTime;
    if (distanceNextFrame > distance)
      distanceNextFrame = distance;
    if (distance <= effectSettings.ColliderRadius)
    {
      DeactivateAttachedPoints(raycastHit);
    }

    var direction = (targetPos - point).normalized;
    if (Physics.Raycast(point, direction, out raycastHit, distanceNextFrame + effectSettings.ColliderRadius)) {
      targetPos = raycastHit.point - direction * effectSettings.ColliderRadius;
      DeactivateAttachedPoints(raycastHit);
    }
  }

  void DeactivateAttachedPoints(RaycastHit hit)
  {
    for (int i = 0; i < AttachedPoints.Length; i++) {
      var ap = AttachedPoints[i];
      ap.SetActive(false);
    }
    var collInfo = new CollisionInfo { Hit = hit };
    effectSettings.OnCollisionHandler(collInfo);
    if (hit.transform != null)
    {
      var shield = hit.transform.GetComponent<ShieldCollisionBehaviour>();
      if (shield != null) shield.ShieldCollisionEnter(collInfo);
    }
    onCollision = true;
  }
}