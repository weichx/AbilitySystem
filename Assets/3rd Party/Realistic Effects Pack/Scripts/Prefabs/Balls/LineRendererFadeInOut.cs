using System.Timers;
using UnityEngine;
using System.Collections;

public class LineRendererFadeInOut : MonoBehaviour
{
  public EffectSettings EffectSettings;
  public float FadeInSpeed;
  public float FadeOutSpeed;
  public float Length = 2, StartWidth = 1, EndWidth = 1;

  private FadeInOutStatus fadeInOutStatus;
  private LineRenderer lineRenderer;
  private float currentStartWidth, currentEndWidth;
  private float currentLength;
  private bool isInit;
  private bool canUpdate = true;
	// Use this for initialization
	void Start () {
    EffectSettings.CollisionEnter += EffectSettings_CollisionEnter;
	  lineRenderer = GetComponent<LineRenderer>();
    fadeInOutStatus = FadeInOutStatus.In;
    lineRenderer.SetPosition(1, new Vector3(0, 0, 0));
    lineRenderer.SetWidth(0, 0);
    lineRenderer.enabled = true;
	  isInit = true;
	}

  void EffectSettings_CollisionEnter(object sender, CollisionInfo e)
  {
    fadeInOutStatus = FadeInOutStatus.Out;
    canUpdate = true;
  }

  void OnEnable()
  {
    if (isInit) {
      fadeInOutStatus = FadeInOutStatus.In;
      canUpdate = true;
      lineRenderer.enabled = true;
    }
  }

	// Update is called once per frame
  private void Update()
  {
    switch (fadeInOutStatus) {
    case FadeInOutStatus.In: {
      if (!canUpdate)
        return;
     
      currentStartWidth += Time.deltaTime * (StartWidth / FadeInSpeed);
      currentEndWidth += Time.deltaTime * (EndWidth / FadeInSpeed);
      currentLength += Time.deltaTime * (Length / FadeInSpeed);
      if (currentStartWidth >= StartWidth) {
        canUpdate = false;
        currentStartWidth = StartWidth;
        currentEndWidth = EndWidth;
        currentLength = Length;
      }
      lineRenderer.SetPosition(1, new Vector3(0, 0, currentLength));
      lineRenderer.SetWidth(currentStartWidth, currentEndWidth);
      break;
    }
    case FadeInOutStatus.Out: {
      if (!canUpdate)
        return;
      
      currentStartWidth -= Time.deltaTime * (StartWidth / FadeOutSpeed);
      currentEndWidth -= Time.deltaTime * (EndWidth / FadeOutSpeed);
      currentLength -= Time.deltaTime * (Length / FadeOutSpeed);
      if (currentStartWidth <=0)
      {
        canUpdate = false;
        currentStartWidth = 0;
        currentEndWidth = 0;
        currentLength = 0;
      } 
      lineRenderer.SetPosition(1, new Vector3(0, 0, currentLength));
      lineRenderer.SetWidth(currentStartWidth, currentEndWidth);
      if (!canUpdate) lineRenderer.enabled = false;
      break;
    }
    }
  }
}
