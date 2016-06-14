/*
http://www.cgsoso.com/forum-257-1.html

CG搜搜 Unity3d 插件团购

CGSOSO 主打游戏开发，影视设计等CG资源素材。

每日Unity3d插件免费更新，仅限下载试用，如若商用，请务必官网购买！
*/

using UnityEngine;
using System.Collections;

public class AnimationSpeedDebuff : MonoBehaviour
{
  public AnimationCurve AnimationSpeenOnTime;
  public float MaxTime = 1;

  private Animator myAnimator;
  private Transform root;
  private float oldSpeed;
  private float time;
  void GetAnimatorOnParent(Transform t)
  {
    var anim = t.parent.GetComponent<Animator>();
    if (anim==null) {
      if (root == t.parent) return;
      GetAnimatorOnParent(t.parent);
    }
    else
      myAnimator = anim;
  }

	// Use this for initialization
	void Start ()
	{
	  root = transform.root;
    GetAnimatorOnParent(transform);
    if(myAnimator==null) return;
    oldSpeed= myAnimator.speed;
	}
	
	// Update is called once per frame
	void Update () {
	  if(myAnimator==null || AnimationSpeenOnTime.length == 0) return;
	  time += Time.deltaTime;
    myAnimator.speed = AnimationSpeenOnTime.Evaluate(time / MaxTime) * oldSpeed;
	}
}
