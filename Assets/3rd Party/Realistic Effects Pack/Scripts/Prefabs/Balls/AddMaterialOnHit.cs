using System.Linq;
using UnityEngine;
using System.Collections;

public class AddMaterialOnHit : MonoBehaviour
{
  public float RemoveAfterTime = 5;
  public bool RemoveWhenDisable;
  public EffectSettings EffectSettings;
  public Material Material;
  public bool UsePointMatrixTransform;
  public Vector3 TransformScale = Vector3.one;
  
  private FadeInOutShaderColor[] fadeInOutShaderColor;
  private FadeInOutShaderFloat[] fadeInOutShaderFloat;
  private UVTextureAnimator uvTextureAnimator;
  private Renderer renderParent;
  private Material instanceMat;
  private int materialQueue = -1;
  private bool waitRemove;
  private float timeToDelete;

  void Update()
  {
    if (EffectSettings==null) return;
    if (EffectSettings.IsVisible) {
      timeToDelete = 0;
    }
    else {
      timeToDelete += Time.deltaTime;
      if (timeToDelete > RemoveAfterTime) Destroy(gameObject);
    }
  }

  public void UpdateMaterial(RaycastHit hit)
  {
    var hitGO = hit.transform;
    if (hitGO!=null) {
      if (!RemoveWhenDisable) Destroy(gameObject, RemoveAfterTime);
      fadeInOutShaderColor = GetComponents<FadeInOutShaderColor>();
      fadeInOutShaderFloat = GetComponents<FadeInOutShaderFloat>();
      uvTextureAnimator = GetComponent<UVTextureAnimator>();
      renderParent = transform.parent.GetComponent<Renderer>();

      var materials = renderParent.sharedMaterials;
      var length = materials.Length + 1;
      var newMaterials = new Material[length];

      materials.CopyTo(newMaterials, 0);
      renderParent.material = Material;
      instanceMat = renderParent.material;
      newMaterials[length - 1] = instanceMat;
      renderParent.sharedMaterials = newMaterials;
      
      if (UsePointMatrixTransform) {
        var m = Matrix4x4.TRS(hit.transform.InverseTransformPoint(hit.point), Quaternion.Euler(180, 180, 0f), TransformScale);
        //m *= transform.localToWorldMatrix;
        instanceMat.SetMatrix("_DecalMatr", m);
      }
      if (materialQueue!=-1)
        instanceMat.renderQueue = materialQueue;
      
      if (fadeInOutShaderColor!=null) {
        foreach (var inOutShaderColor in fadeInOutShaderColor) {
          inOutShaderColor.UpdateMaterial(instanceMat);
        }
      }
     
      if (fadeInOutShaderFloat!=null) {
        foreach (var inOutShaderFloat in fadeInOutShaderFloat) {
          inOutShaderFloat.UpdateMaterial(instanceMat);
        }
      }
     
      if (uvTextureAnimator!=null)
        uvTextureAnimator.SetInstanceMaterial(instanceMat, hit.textureCoord);
    }
  }

  public void UpdateMaterial(Transform transformTarget)
  {
    if (transformTarget!=null) {
      if (!RemoveWhenDisable) Destroy(gameObject, RemoveAfterTime);
      fadeInOutShaderColor = GetComponents<FadeInOutShaderColor>();
      fadeInOutShaderFloat = GetComponents<FadeInOutShaderFloat>();
      uvTextureAnimator = GetComponent<UVTextureAnimator>();
      renderParent = transform.parent.GetComponent<Renderer>();

      var materials = renderParent.sharedMaterials;
      var length = materials.Length + 1;
      var newMaterials = new Material[length];

      materials.CopyTo(newMaterials, 0);
      renderParent.material = Material;
      instanceMat = renderParent.material;
      newMaterials[length - 1] = instanceMat;
      renderParent.sharedMaterials = newMaterials;

      if (materialQueue!=-1)
        instanceMat.renderQueue = materialQueue;

      if (fadeInOutShaderColor!=null) {
        foreach (var inOutShaderColor in fadeInOutShaderColor) {
          inOutShaderColor.UpdateMaterial(instanceMat);
        }
      }

      if (fadeInOutShaderFloat!=null) {
        foreach (var inOutShaderFloat in fadeInOutShaderFloat) {
          inOutShaderFloat.UpdateMaterial(instanceMat);
        }
      }

      if (uvTextureAnimator!=null)
        uvTextureAnimator.SetInstanceMaterial(instanceMat, Vector2.zero);
    }
  }

  public void SetMaterialQueue(int matlQueue)
  {
    materialQueue = matlQueue;
  }

  public int GetDefaultMaterialQueue()
  {
    return instanceMat.renderQueue;
  }

  void OnDestroy()
  {
    if (renderParent==null)
      return;
    var materials = renderParent.sharedMaterials.ToList();
    materials.Remove(instanceMat);
    renderParent.sharedMaterials = materials.ToArray();
  }
}
