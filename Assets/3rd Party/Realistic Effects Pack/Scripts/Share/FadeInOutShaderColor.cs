using System.Diagnostics;
using System.Security;
using UnityEngine;
using System.Collections;
using Debug = UnityEngine.Debug;

public class FadeInOutShaderColor : MonoBehaviour
{
  public string ShaderColorName = "_Color";
  public float StartDelay = 0;
  public float FadeInSpeed = 0;
  public float FadeOutDelay = 0;
  public float FadeOutSpeed = 0;
  public bool UseSharedMaterial;
  public bool FadeOutAfterCollision;
  public bool UseHideStatus;
  
  private Material mat;
  private Color oldColor, currentColor;
  private float oldAlpha, alpha;
  private bool canStart, canStartFadeOut, fadeInComplited, fadeOutComplited;
  private bool isCollisionEnter;
  private bool isStartDelay, isIn, isOut;
  private EffectSettings effectSettings;
  private bool isInitialized;

  #region Non-public methods

  private void GetEffectSettingsComponent(Transform tr)
  {
    var parent = tr.parent;
    if (parent!=null) {
      effectSettings = parent.GetComponentInChildren<EffectSettings>();
      if (effectSettings==null)
        GetEffectSettingsComponent(parent.transform);
    }
  }

  public void UpdateMaterial(Material instanceMaterial)
  {
    mat = instanceMaterial;
    InitMaterial();
  }

  private void Start()
  {
    GetEffectSettingsComponent(transform);
    if (effectSettings!=null)
      effectSettings.CollisionEnter += prefabSettings_CollisionEnter;

    InitMaterial();
  }

  private void InitMaterial()
  {
    if (isInitialized) return;
    if (GetComponent<Renderer>()!=null) mat = GetComponent<Renderer>().material;
    else {
      var lineRenderer = GetComponent<LineRenderer>();
      if (lineRenderer!=null) mat = lineRenderer.material;
      else {
        var projector = GetComponent<Projector>();
        if (projector!=null) {
          if (!projector.material.name.EndsWith("(Instance)"))
            projector.material = new Material(projector.material) { name = projector.material.name + " (Instance)" };
          mat = projector.material;
        }
      }
    }
    
    if(mat == null) return;
   
    oldColor = mat.GetColor(ShaderColorName);
    isStartDelay = StartDelay > 0.001f;
    isIn = FadeInSpeed > 0.001f;
    isOut = FadeOutSpeed > 0.001f;
    InitDefaultVariables();
    isInitialized = true;

  }

  private void InitDefaultVariables()
  {
    fadeInComplited = false;
    fadeOutComplited = false;
    canStartFadeOut = false;
    isCollisionEnter = false;
    oldAlpha = 0;
    alpha = 0;
    canStart = false;

    currentColor = oldColor;
    if (isIn) currentColor.a = 0;
    mat.SetColor(ShaderColorName, currentColor);
    if (isStartDelay) Invoke("SetupStartDelay", StartDelay);
    else canStart = true;
    if (!isIn) {
      if (!FadeOutAfterCollision)
        Invoke("SetupFadeOutDelay", FadeOutDelay);
      oldAlpha = oldColor.a;
    }
  }

  private void prefabSettings_CollisionEnter(object sender, CollisionInfo e)
  {
    isCollisionEnter = true;
    if (!isIn && FadeOutAfterCollision) Invoke("SetupFadeOutDelay", FadeOutDelay);
  }

  void OnEnable()
  {
    if (isInitialized) InitDefaultVariables();
  }

  private void SetupStartDelay()
  {
    canStart = true;
  }

  private void SetupFadeOutDelay()
  {
    canStartFadeOut = true;
  }

  private void Update()
  {
    if (!canStart)
      return;

    if (effectSettings != null && UseHideStatus)
    {
      if (!effectSettings.IsVisible && fadeInComplited)
        fadeInComplited = false;
      if (effectSettings.IsVisible && fadeOutComplited)
        fadeOutComplited = false;
    }

    if (UseHideStatus) {
      if (isIn) {
        if (effectSettings!=null && effectSettings.IsVisible && !fadeInComplited)
          FadeIn();
      }
      if (isOut) {
        if (effectSettings!=null && !effectSettings.IsVisible && !fadeOutComplited)
          FadeOut();
      }
    }
    else if (!FadeOutAfterCollision) {
      if (isIn) {
        if (!fadeInComplited)
          FadeIn();
      }
      if (isOut && canStartFadeOut) {
        if (!fadeOutComplited)
          FadeOut();
      }
    }
    else {
      if (isIn) {
        if (!fadeInComplited)
          FadeIn();
      }
      if (isOut && isCollisionEnter && canStartFadeOut && !fadeOutComplited)
        FadeOut();
    }
  }


  private void FadeIn()
  {
    alpha = oldAlpha + Time.deltaTime / FadeInSpeed;
    if (alpha >= oldColor.a) {
      fadeInComplited = true; 
      alpha = oldColor.a;
      Invoke("SetupFadeOutDelay", FadeOutDelay);
    } 
    currentColor.a = alpha;
    mat.SetColor(ShaderColorName, currentColor);
    oldAlpha = alpha;
  }

  private void FadeOut()
  {
    alpha = oldAlpha - Time.deltaTime / FadeOutSpeed;
    if (alpha <= 0) {
      alpha = 0;
      fadeOutComplited = true;
    }
    currentColor.a = alpha;
    mat.SetColor(ShaderColorName, currentColor);
    oldAlpha = alpha;
  }

  #endregion
}