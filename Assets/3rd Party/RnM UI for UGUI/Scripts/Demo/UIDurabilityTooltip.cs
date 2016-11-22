using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI.Tweens;
using System;
using System.Collections;

namespace UnityEngine.UI
{
	[RequireComponent(typeof(RectTransform)), RequireComponent(typeof(CanvasGroup)), RequireComponent(typeof(UIIgnoreRaycast))]
	public class UIDurabilityTooltip : UIBehaviour {
		
		protected static UIDurabilityTooltip mInstance;
		
		private RectTransform m_Rect;
		private Canvas m_Canvas;
		private CanvasGroup m_CanvasGroup;
		
		public Text percentText;
		public bool fading = true;
		public float fadeDuration = 0.15f;
		public TweenEasing fadeEasing = TweenEasing.InOutQuint;
		public Vector2 offset = Vector2.zero;
		
		/// <summary>
		/// Gets the current alpha.
		/// </summary>
		/// <value>The current alpha.</value>
		public float currentAlpha
		{
			get { return (this.m_CanvasGroup != null) ? this.m_CanvasGroup.alpha : 0f; }
		}
		
		/// <summary>
		/// Gets a value indicating whether this <see cref="UnityEngine.UI.UIDurabilityTooltip"/> is visible.
		/// </summary>
		/// <value><c>true</c> if is visible; otherwise, <c>false</c>.</value>
		public bool isVisible
		{
			get { return (this.m_CanvasGroup.alpha > 0f); }
		}
		
		/// <summary>
		/// Gets the camera responsible for the tooltip.
		/// </summary>
		/// <value>The camera.</value>
		public Camera uiCamera
		{
			get
			{
				if (this.m_Canvas == null)
					return null;
				
				if (this.m_Canvas.renderMode == RenderMode.ScreenSpaceOverlay || (this.m_Canvas.renderMode == RenderMode.ScreenSpaceCamera && this.m_Canvas.worldCamera == null))
				{
					return null;
				}
				
				return (!(this.m_Canvas.worldCamera != null)) ? Camera.main : this.m_Canvas.worldCamera;
			}
		}
		
		[NonSerialized] private readonly TweenRunner<FloatTween> m_FloatTweenRunner;
		
		// Called by Unity prior to deserialization, 
		// should not be called by users
		protected UIDurabilityTooltip()
		{
			if (this.m_FloatTweenRunner == null)
				this.m_FloatTweenRunner = new TweenRunner<FloatTween>();
			
			this.m_FloatTweenRunner.Init(this);
		}
		
		protected override void Awake()
		{
			mInstance = this;
			
			// Get the rect transform
			this.m_Rect = (this.transform as RectTransform);
			
			// Get the canvas
			this.m_Canvas = UIUtility.FindInParents<Canvas>(this.gameObject);
			
			// Get the canvas group
			this.m_CanvasGroup = this.gameObject.GetComponent<CanvasGroup>();
		}
		
		protected override  void OnDestroy()
		{
			mInstance = null;
		}
		
		protected override void Start()
		{
			// Hide the tooltip
			this.SetAlpha(0f);
		}
		
		protected override void OnCanvasGroupChanged()
		{
			// Get the canvas responsible for the tooltip
			this.m_Canvas = UIUtility.FindInParents<Canvas>(this.gameObject);
		}
		
		protected void Update()
		{
			if (this.isVisible)
			{
				// Update the position of the tooltip
				Vector2 anchorPos = Vector2.zero;
				RectTransformUtility.ScreenPointToLocalPointInRectangle((this.transform.parent as RectTransform), Input.mousePosition, this.uiCamera, out anchorPos);
				this.m_Rect.anchoredPosition = anchorPos;
				
				// Round up and apply offset
				this.m_Rect.anchoredPosition = new Vector2(Mathf.Round((this.m_Rect.anchoredPosition.x / 2f) * 2f), Mathf.Round((this.m_Rect.anchoredPosition.y / 2f) * 2f));
				this.m_Rect.anchoredPosition += this.offset;
			}
		}
		
		/// <summary>
		/// Sets the durability string.
		/// </summary>
		/// <param name="durability">Durability.</param>
		public static void SetDurability(string durabilityPct)
		{
			if (mInstance != null)
				mInstance._SetDurability(durabilityPct);
		}
		
		private void _SetDurability(string durabilityPct)
		{
			if (this.percentText != null)
				this.percentText.text = durabilityPct + "%";
		}
		
		/// <summary>
		/// Show the tooltip.
		/// </summary>
		public static void Show()
		{
			if (mInstance != null)
				mInstance._Show();
		}
		
		private void _Show()
		{
			// Transition
			this.EvaluateAndTransitionToState(true, false);
		}
		
		/// <summary>
		/// Hide the tooltip.
		/// </summary>
		public static void Hide()
		{
			if (mInstance != null)
				mInstance._Hide();
		}
		
		private void _Hide()
		{
			// Transition
			this.EvaluateAndTransitionToState(false, false);
		}
		
		/// <summary>
		/// Evaluates and transitions to the given state.
		/// </summary>
		/// <param name="state">If set to <c>true</c> transition to shown <c>false</c> otherwise.</param>
		/// <param name="instant">If set to <c>true</c> instant.</param>
		private void EvaluateAndTransitionToState(bool state, bool instant)
		{
			// Do the transition
			if (this.fading)
			{
				this.StartAlphaTween((state ? 1f : 0f), (instant ? 0f : this.fadeDuration));
			}
			else
			{
				this.SetAlpha(state ? 1f : 0f);
			}
		}
		
		/// <summary>
		/// Sets the alpha of the tooltip.
		/// </summary>
		/// <param name="alpha">Alpha.</param>
		public void SetAlpha(float alpha)
		{
			this.m_CanvasGroup.alpha = alpha;
		}
		
		/// <summary>
		/// Starts a alpha tween on the tooltip.
		/// </summary>
		/// <param name="targetAlpha">Target alpha.</param>
		public void StartAlphaTween(float targetAlpha, float duration)
		{
			var floatTween = new FloatTween { duration = duration, startFloat = this.m_CanvasGroup.alpha, targetFloat = targetAlpha };
			floatTween.AddOnChangedCallback(SetAlpha);
			floatTween.ignoreTimeScale = true;
			floatTween.easing = this.fadeEasing;
			this.m_FloatTweenRunner.StartTween(floatTween);
		}
	}
}