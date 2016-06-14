/*
http://www.cgsoso.com/forum-257-1.html

CG搜搜 Unity3d 插件团购

CGSOSO 主打游戏开发，影视设计等CG资源素材。

每日Unity3d插件免费更新，仅限下载试用，如若商用，请务必官网购买！
*/

using UnityEngine;
using System.Collections;

public class RotateAround : MonoBehaviour
{

  public float Speed = 1;
  public float LifeTime = 1;
  public float TimeDelay = 0;
  public float SpeedFadeInTime = 0;
  public bool UseCollision;
  public EffectSettings EffectSettings;

  private bool canUpdate;
  private float currentSpeedFadeIn;
  private float allTime;
	// Use this for initialization
	void Start ()
	{
    if(UseCollision) EffectSettings.CollisionEnter += EffectSettings_CollisionEnter;
	  if (TimeDelay > 0)
	    Invoke("ChangeUpdate", TimeDelay);
	  else
	    canUpdate = true;
	}

  void OnEnable()
  {
    canUpdate = true;
    allTime = 0;
  }

  void EffectSettings_CollisionEnter(object sender, CollisionInfo e)
  {
    canUpdate = false;
  }

  void ChangeUpdate()
  {
    canUpdate = true;
  }
	
	// Update is called once per frame
  private void Update()
  {
    if (!canUpdate)
      return;

    allTime += Time.deltaTime;
    if (allTime >= LifeTime && LifeTime > 0.0001f)
      return;
    if (SpeedFadeInTime > 0.001f) {
      if (currentSpeedFadeIn < Speed)
        currentSpeedFadeIn += (Time.deltaTime / SpeedFadeInTime) * Speed;
      else
        currentSpeedFadeIn = Speed;
    }
    else
      currentSpeedFadeIn = Speed;

    transform.Rotate(Vector3.forward * Time.deltaTime * currentSpeedFadeIn);
  }
}
