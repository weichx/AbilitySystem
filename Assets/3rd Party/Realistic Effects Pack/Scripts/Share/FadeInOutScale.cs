using System.Text;
using UnityEngine;
using System.Collections;

public class FadeInOutScale : MonoBehaviour
{
  public FadeInOutStatus FadeInOutStatus = FadeInOutStatus.In;
  public float Speed = 1;
  public float MaxScale = 2;

  private Vector3 oldScale;
  private float time, oldSin;
  private bool updateTime = true, canUpdate = true;
  private Transform t;
  private EffectSettings effectSettings;
  private bool isInitialized;
  private bool isCollisionEnter;

  private void Start()
  {
    t = transform;
    oldScale = t.localScale;
    isInitialized = true;

    GetEffectSettingsComponent(transform);
    if (effectSettings != null)
      effectSettings.CollisionEnter += prefabSettings_CollisionEnter;
  }

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

  public void InitDefaultVariables()
  {
    if (FadeInOutStatus==FadeInOutStatus.OutAfterCollision) {
      t.localScale = oldScale;
      canUpdate = false;
      
    }
    else {
      t.localScale = Vector3.zero;
      canUpdate = true;
    }
    updateTime = true;
    time = 0;
    oldSin = 0;
    
    isCollisionEnter = false;
  }

  void prefabSettings_CollisionEnter(object sender, CollisionInfo e)
  {
    isCollisionEnter = true;
    canUpdate = true;
  }

  void OnEnable()
  {
    if(isInitialized) InitDefaultVariables();
  }

  private void Update()
  {
    if (!canUpdate)
      return;

    if (updateTime)
    {
      time = Time.time;
      updateTime = false;
    }
    var sin = Mathf.Sin((Time.time - time) / Speed);
    float scale;
    if (oldSin > sin) {
      canUpdate = false;
      scale = MaxScale;
    }
    else {
      scale = sin * MaxScale;
    }

    if (FadeInOutStatus==FadeInOutStatus.In) {
      if(scale < MaxScale) t.localScale = new Vector3(oldScale.x * scale, oldScale.y * scale, oldScale.z * scale);
      else t.localScale = new Vector3(MaxScale, MaxScale, MaxScale);
    }
    if (FadeInOutStatus==FadeInOutStatus.Out) {
      if (scale > 0)
        t.localScale = new Vector3(
          MaxScale * oldScale.x - oldScale.x * scale,
          MaxScale * oldScale.y - oldScale.y * scale,
          MaxScale * oldScale.z - oldScale.z * scale);
      else
        t.localScale = Vector3.zero;
    }
    if (FadeInOutStatus == FadeInOutStatus.OutAfterCollision && isCollisionEnter)
    {
      if (scale > 0)
        t.localScale = new Vector3(
          MaxScale * oldScale.x - oldScale.x * scale,
          MaxScale * oldScale.y - oldScale.y * scale,
          MaxScale * oldScale.z - oldScale.z * scale);
      else
        t.localScale = Vector3.zero;
    }
    
      oldSin = sin;
  }
}