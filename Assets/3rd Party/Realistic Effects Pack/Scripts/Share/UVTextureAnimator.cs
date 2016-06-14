using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

internal class UVTextureAnimator : MonoBehaviour
{
  public Material[] AnimatedMaterialsNotInstance = null;
  public int Rows = 4;
  public int Columns = 4;
  public float Fps = 20;
  public int OffsetMat = 0;
  public Vector2 SelfTiling = new Vector2();
  public bool IsLoop = true;
  public bool IsReverse = false;
  public bool IsRandomOffsetForInctance = false;
  public bool IsBump = false;
  public bool IsHeight = false;
  public bool IsCutOut = false;

  private bool isInizialised;
  private int index;
  private int count, allCount;
  private float deltaFps;
  private bool isVisible;
  private bool isCorutineStarted;
  private Renderer currentRenderer;
  private Material instanceMaterial;
  
  #region Non-public methods

  private void Start()
  {
    InitMaterial();
    InitDefaultVariables();
    isInizialised = true;
    isVisible = true;
    StartCoroutine(UpdateCorutine());
  }

  public void SetInstanceMaterial(Material mat, Vector2 offsetMat)
  {
    instanceMaterial = mat;
    InitDefaultVariables();
  }

  private void InitDefaultVariables()
  {
    
    allCount = 0;
    deltaFps = 1f / Fps;
    count = Rows * Columns;
    index = Columns - 1;
    var offset = new Vector2((float) index / Columns - (index / Columns),
      1 - (index / Columns) / (float) Rows);
    OffsetMat = !IsRandomOffsetForInctance
      ? OffsetMat - (OffsetMat / count) * count
      : Random.Range(0, count);
    var size = SelfTiling==Vector2.zero ? new Vector2(1f / Columns, 1f / Rows) : SelfTiling;

    if (AnimatedMaterialsNotInstance.Length > 0)
      foreach (var mat in AnimatedMaterialsNotInstance) {
        mat.SetTextureScale("_MainTex", size);
        mat.SetTextureOffset("_MainTex", Vector2.zero);
        if (IsBump) {
          mat.SetTextureScale("_BumpMap", size);
          mat.SetTextureOffset("_BumpMap", Vector2.zero);
        }
        if (IsHeight) {
          mat.SetTextureScale("_HeightMap", size);
          mat.SetTextureOffset("_HeightMap", Vector2.zero);
        }
        if (IsCutOut){
          mat.SetTextureScale("_CutOut", size);
          mat.SetTextureOffset("_CutOut", Vector2.zero);
        }
      }
    else if (instanceMaterial != null)
    {
      instanceMaterial.SetTextureScale("_MainTex", size);
      instanceMaterial.SetTextureOffset("_MainTex", offset);
      if (IsBump)
      {
        instanceMaterial.SetTextureScale("_BumpMap", size);
        instanceMaterial.SetTextureOffset("_BumpMap", offset);
      }
      if (IsBump)
      {
        instanceMaterial.SetTextureScale("_HeightMap", size);
        instanceMaterial.SetTextureOffset("_HeightMap", offset);
      }
      if (IsCutOut)
      {
        instanceMaterial.SetTextureScale("_CutOut", size);
        instanceMaterial.SetTextureOffset("_CutOut", offset);
      }
    }
    else if(currentRenderer!=null) {
      currentRenderer.material.SetTextureScale("_MainTex", size);
      currentRenderer.material.SetTextureOffset("_MainTex", offset);
      if (IsBump) {
        currentRenderer.material.SetTextureScale("_BumpMap", size);
        currentRenderer.material.SetTextureOffset("_BumpMap", offset);
      }
      if (IsHeight) {
        currentRenderer.material.SetTextureScale("_HeightMap", size);
        currentRenderer.material.SetTextureOffset("_HeightMap", offset);
      }
      if (IsCutOut) {
        currentRenderer.material.SetTextureScale("_CutOut", size);
        currentRenderer.material.SetTextureOffset("_CutOut", offset);
      }
    }
  }

  private void InitMaterial()
  {
    
    if (GetComponent<Renderer>()!=null)
      currentRenderer = GetComponent<Renderer>();
    else {
      var projector = GetComponent<Projector>(); 
      if (projector!=null) {
        if (!projector.material.name.EndsWith("(Instance)"))
          projector.material = new Material(projector.material) { name = projector.material.name + " (Instance)" };
        instanceMaterial = projector.material;
      }
    }
  }

  #region CorutineCode

  private void OnEnable()
  {
    if (!isInizialised)
      return;
    InitDefaultVariables();
    isVisible = true;
    if (!isCorutineStarted)
      StartCoroutine(UpdateCorutine());
  }

  private void OnDisable()
  {
    isCorutineStarted = false;
    isVisible = false;
    StopAllCoroutines();
  }

  private void OnBecameVisible()
  {
    isVisible = true;
    if (!isCorutineStarted)
      StartCoroutine(UpdateCorutine());
  }

  private void OnBecameInvisible()
  {
    isVisible = false;
  }

  private IEnumerator UpdateCorutine()
  {
    isCorutineStarted = true;
    while (isVisible && (IsLoop || allCount!=count)) {
      UpdateCorutineFrame();
      if (!IsLoop && allCount==count)
        break;
      yield return new WaitForSeconds(deltaFps);
    }
    isCorutineStarted = false;
  }

  #endregion CorutineCode

  private void UpdateCorutineFrame()
  {
    if (currentRenderer==null && instanceMaterial==null && AnimatedMaterialsNotInstance.Length==0)
      return;

    ++allCount;
    if (IsReverse)
      --index;
    else
      ++index;
    if (index >= count)
      index = 0;
    
    if (AnimatedMaterialsNotInstance.Length > 0)
      for (int i = 0; i < AnimatedMaterialsNotInstance.Length; i++) {
        var idx = i * OffsetMat + index + OffsetMat;
        idx = idx - (idx / count) * count;
        var offset = new Vector2((float) idx / Columns - (idx / Columns),
          1 - (idx / Columns) / (float) Rows);
        AnimatedMaterialsNotInstance[i].SetTextureOffset("_MainTex", offset);
        if (IsBump)
          AnimatedMaterialsNotInstance[i].SetTextureOffset("_BumpMap", offset);
        if (IsHeight)
          AnimatedMaterialsNotInstance[i].SetTextureOffset("_HeightMap", offset);
        if (IsCutOut)
          AnimatedMaterialsNotInstance[i].SetTextureOffset("_CutOut", offset);
      }
    else {
      Vector2 offset;
      if (IsRandomOffsetForInctance) {
        var idx = index + OffsetMat;
        offset = new Vector2((float) idx / Columns - (idx / Columns),
          1 - (idx / Columns) / (float) Rows);
      }
      else
      {
        offset = new Vector2((float) index / Columns - (index / Columns),
          1 - (index / Columns) / (float) Rows);
        //offset += offsetGlobal;
        //offset.x += 0.05f;
      }

      if (instanceMaterial!=null) {
        instanceMaterial.SetTextureOffset("_MainTex", offset);
        if (IsBump)
          instanceMaterial.SetTextureOffset("_BumpMap", offset);
        if (IsHeight)
          instanceMaterial.SetTextureOffset("_HeightMap", offset);
        if (IsCutOut)
          instanceMaterial.SetTextureOffset("_CutOut", offset);
      }
      else if(currentRenderer!=null){
        currentRenderer.material.SetTextureOffset("_MainTex", offset);
        if (IsBump)
          currentRenderer.material.SetTextureOffset("_BumpMap", offset);
        if (IsHeight)
          currentRenderer.material.SetTextureOffset("_HeightMap", offset);
        if (IsCutOut)
          currentRenderer.material.SetTextureOffset("_CutOut", offset);
      }
    }
  }

  #endregion
}