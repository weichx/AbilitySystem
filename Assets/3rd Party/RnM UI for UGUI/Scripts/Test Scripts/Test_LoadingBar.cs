using System;
using UnityEngine;
using UnityEngine.UI.Tweens;
using UnityEngine.SceneManagement;

namespace UnityEngine.UI
{
	public class Test_LoadingBar : MonoBehaviour {
		
		public Image imageComponent;
		public Text textComponent;
		public float Duration = 5f;
		public string LoadScene = "";
		
		// Tween controls
		[NonSerialized] private readonly TweenRunner<FloatTween> m_FloatTweenRunner;
		
		// Called by Unity prior to deserialization, 
		// should not be called by users
		protected Test_LoadingBar()
		{
			if (this.m_FloatTweenRunner == null)
				this.m_FloatTweenRunner = new TweenRunner<FloatTween>();
			
			this.m_FloatTweenRunner.Init(this);
		}
		
		protected void Start()
		{
			if (this.imageComponent != null)
				this.imageComponent.fillAmount = 0f;
			
			if (this.textComponent != null)
				this.textComponent.text = "0%";
				
			var floatTween = new FloatTween { duration = this.Duration, startFloat = 0f, targetFloat = 1f };
			floatTween.AddOnChangedCallback(SetFillAmount);
			floatTween.AddOnFinishCallback(OnTweenFinished);
			floatTween.ignoreTimeScale = true;
			this.m_FloatTweenRunner.StartTween(floatTween);
		}
		
		protected void SetFillAmount(float amount)
		{
			if (this.imageComponent != null)
				this.imageComponent.fillAmount = amount;
				
			if (this.textComponent != null)
				this.textComponent.text = (amount * 100).ToString("0") + "%";
		}
		
		protected void OnTweenFinished()
		{
			if (!string.IsNullOrEmpty(this.LoadScene))
				SceneManager.LoadScene(this.LoadScene);
		}
	}
}