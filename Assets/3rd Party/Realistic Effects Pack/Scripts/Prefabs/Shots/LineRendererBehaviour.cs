using UnityEngine;
using System.Collections;

public class LineRendererBehaviour : MonoBehaviour
{
  public bool IsVertical = false;
  public float LightHeightOffset = 0.3f;
  public float ParticlesHeightOffset = 0.2f;
  public float TimeDestroyLightAfterCollision = 4f;
  public float TimeDestroyThisAfterCollision = 4;
  public float TimeDestroyRootAfterCollision = 4f;
  public GameObject EffectOnHitObject;
  public GameObject Explosion;
  public GameObject StartGlow;
  public GameObject HitGlow;
  public GameObject Particles;
  public GameObject GoLight;

  private EffectSettings effectSettings;
  private Transform tRoot, tTarget;
  private bool isInitializedOnStart;
  private LineRenderer line;
  private int currentShaderIndex;
  RaycastHit hit;

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
	void Start () {
    GetEffectSettingsComponent(transform);
    if (effectSettings == null)
      Debug.Log("Prefab root have not script \"PrefabSettings\"");
    tRoot = effectSettings.transform;
    line = GetComponent<LineRenderer>();
	  InitializeDefault();
	  isInitializedOnStart = true;
	}

  void InitializeDefault()
  {
    GetComponent<Renderer>().material.SetFloat("_Chanel", currentShaderIndex);
    ++currentShaderIndex;
    if (currentShaderIndex == 3) currentShaderIndex = 0;
    line.SetPosition(0, tRoot.position);
    if (IsVertical)
    {
      if (Physics.Raycast(tRoot.position, Vector3.down, out hit))
      {
        line.SetPosition(1, hit.point);
        if (StartGlow != null) StartGlow.transform.position = tRoot.position;
        if (HitGlow != null) HitGlow.transform.position = hit.point;
        if (GoLight != null) GoLight.transform.position = hit.point + new Vector3(0, LightHeightOffset, 0);
        if (Particles != null) Particles.transform.position = hit.point + new Vector3(0, ParticlesHeightOffset, 0);
        if (Explosion != null) Explosion.transform.position = hit.point + new Vector3(0, ParticlesHeightOffset, 0);
      }
    }
    else
    {
      if (effectSettings.Target != null) tTarget = effectSettings.Target.transform; 
      else if (!effectSettings.UseMoveVector) { Debug.Log("You must setup the the target or the motion vector"); }
      Vector3 targetDirection;
      if (!effectSettings.UseMoveVector) {
        targetDirection = (tTarget.position - tRoot.position).normalized;
      }
      else {
        targetDirection = tRoot.position + effectSettings.MoveVector * effectSettings.MoveDistance;
      }
      var direction = tRoot.position + targetDirection * effectSettings.MoveDistance;
      if (Physics.Raycast(tRoot.position, targetDirection, out hit, effectSettings.MoveDistance + 1, effectSettings.LayerMask)) {
        direction = (tRoot.position + Vector3.Normalize(hit.point - tRoot.position) * (effectSettings.MoveDistance + 1)).normalized;
      } 
      line.SetPosition(1, hit.point - effectSettings.ColliderRadius * direction);
      var particlesOffsetPos = hit.point - direction * ParticlesHeightOffset;
      if (StartGlow!=null) StartGlow.transform.position = tRoot.position;
      if (HitGlow!=null) HitGlow.transform.position = particlesOffsetPos;
      if (GoLight!=null) GoLight.transform.position = hit.point - direction * LightHeightOffset;
      if (Particles!=null) Particles.transform.position = particlesOffsetPos;
      if (Explosion!=null) {
        Explosion.transform.position = particlesOffsetPos;
        Explosion.transform.LookAt(particlesOffsetPos + hit.normal);
      }
    }

    var collInfo = new CollisionInfo { Hit = hit };
    effectSettings.OnCollisionHandler(collInfo);
    if (hit.transform != null)
    {
      var shield = hit.transform.GetComponent<ShieldCollisionBehaviour>();
      if (shield != null) shield.ShieldCollisionEnter(collInfo);
    }
  }

  void OnEnable()
  {
    if (isInitializedOnStart) InitializeDefault();
  }
}