/*
http://www.cgsoso.com/forum-257-1.html

CG搜搜 Unity3d 插件团购

CGSOSO 主打游戏开发，影视设计等CG资源素材。

每日Unity3d插件免费更新，仅限下载试用，如若商用，请务必官网购买！
*/

using UnityEngine;
using System.Collections;

public class AnimatorBehaviour : MonoBehaviour {

	public Animator anim;

	private EffectSettings effectSettings;
	private bool isInitialized;
	private float oldSpeed;

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
		oldSpeed = anim.speed;
		GetEffectSettingsComponent(transform);
		if (effectSettings!=null)
			effectSettings.CollisionEnter += prefabSettings_CollisionEnter;

		isInitialized = true; 
	}

	void OnEnable()
	{
		if(isInitialized) anim.speed = oldSpeed;
	}

	void prefabSettings_CollisionEnter(object sender, CollisionInfo e)
	{
		anim.speed = 0;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
