/*
http://www.cgsoso.com/forum-257-1.html

CG搜搜 Unity3d 插件团购

CGSOSO 主打游戏开发，影视设计等CG资源素材。

每日Unity3d插件免费更新，仅限下载试用，如若商用，请务必官网购买！
*/

using UnityEngine;
using System.Collections;

public class FadeInOutSound: MonoBehaviour
{
  public float MaxVolume = 1;
  public float StartDelay = 0;
  public float FadeInSpeed = 0;
  public float FadeOutDelay = 0;
  public float FadeOutSpeed = 0;
  public bool FadeOutAfterCollision;
  public bool UseHideStatus;

  private AudioSource audioSource;
  private float oldVolume, currentVolume;
  private bool canStart, canStartFadeOut, fadeInComplited, fadeOutComplited;
  private bool isCollisionEnter, allComplited;
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

  private void Start()
  {
    GetEffectSettingsComponent(transform);
    if (effectSettings!=null)
      effectSettings.CollisionEnter += prefabSettings_CollisionEnter;

    InitSource();
  }

  private void InitSource()
  {
    if (isInitialized) return;
    audioSource = GetComponent<AudioSource>();
    if (audioSource==null)
      return;
   
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
    allComplited = false;
    canStartFadeOut = false;
    isCollisionEnter = false;
    oldVolume = 0;
    currentVolume = MaxVolume;

    if (isIn) currentVolume = 0;
    audioSource.volume = currentVolume;

    if (isStartDelay) Invoke("SetupStartDelay", StartDelay);
    else canStart = true;
    if (!isIn) {
      if (!FadeOutAfterCollision)
        Invoke("SetupFadeOutDelay", FadeOutDelay);
      oldVolume = MaxVolume;
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
    if (!canStart || audioSource==null)
      return;

    if (effectSettings != null && UseHideStatus && allComplited && effectSettings.IsVisible)
    {
      allComplited = false;
      fadeInComplited = false;
      fadeOutComplited = false;
      InitDefaultVariables();
    }

    if (isIn && !fadeInComplited)
    {
      if (effectSettings == null) FadeIn();
      else if ((UseHideStatus && effectSettings.IsVisible) || !UseHideStatus) FadeIn();
    }

    if (!isOut || fadeOutComplited || !canStartFadeOut)
      return;
    if (effectSettings==null || (!UseHideStatus && !FadeOutAfterCollision))
      FadeOut();
    else if ((UseHideStatus && !effectSettings.IsVisible) || isCollisionEnter)
      FadeOut();
  }


  private void FadeIn()
  {
    currentVolume = oldVolume + Time.deltaTime / FadeInSpeed * MaxVolume;
    if (currentVolume >= MaxVolume)
    {
      fadeInComplited = true;
      if (!isOut) allComplited = true;
      currentVolume = MaxVolume;
      Invoke("SetupFadeOutDelay", FadeOutDelay);
    }

    audioSource.volume = currentVolume;
    oldVolume = currentVolume;
  }

  private void FadeOut()
  {
    currentVolume = oldVolume - Time.deltaTime / FadeOutSpeed * MaxVolume;
    if (currentVolume <= 0)
    {
      currentVolume = 0;
      fadeOutComplited = true;
      allComplited = true;
    }

    audioSource.volume = currentVolume;
    oldVolume = currentVolume;
  }

  #endregion
}
