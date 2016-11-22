using UnityEngine;
using System.Collections;

public class ShieldCollisionBehaviour : MonoBehaviour
{
  public GameObject EffectOnHit;
  public GameObject ExplosionOnHit;
  public bool IsWaterInstance;
  public float ScaleWave = 0.89f;
  public bool CreateMechInstanceOnHit;
  public Vector3 AngleFix;
  public int currentQueue = 3001;

  public void ShieldCollisionEnter(CollisionInfo e)
  {
    if (e.Hit.transform!=null) {
      if (IsWaterInstance) {
        var go = Instantiate(ExplosionOnHit) as GameObject;
        var t = go.transform;
        t.parent = transform;
        var scale = transform.localScale.x * ScaleWave;
        t.localScale = new Vector3(scale, scale, scale);
        t.localPosition = new Vector3(0, 0.001f, 0);
        t.LookAt(e.Hit.point);
      }
      else  {
        if (EffectOnHit!=null) {
          if (!CreateMechInstanceOnHit) {
            var hitGO = e.Hit.transform;
            var renderer = hitGO.GetComponentInChildren<Renderer>();
            var effectInstance = Instantiate(EffectOnHit) as GameObject;
            effectInstance.transform.parent = renderer.transform;
            effectInstance.transform.localPosition = Vector3.zero;
            var addMat = effectInstance.GetComponent<AddMaterialOnHit>();
            addMat.SetMaterialQueue(currentQueue);
            addMat.UpdateMaterial(e.Hit);
          }
          else {
            var effectInstance = Instantiate(EffectOnHit) as GameObject;
            var tr = effectInstance.transform;
            tr.parent = GetComponent<Renderer>().transform;
            tr.localPosition = Vector3.zero;
            tr.localScale = transform.localScale * ScaleWave;
            tr.LookAt(e.Hit.point);
            tr.Rotate(AngleFix);
            effectInstance.GetComponent<Renderer>().material.renderQueue = currentQueue-1000;
          }
        }
        if (currentQueue > 4000) currentQueue = 3001;
        else ++currentQueue;

        if (ExplosionOnHit!=null) {
          var inst2 = Instantiate(ExplosionOnHit, e.Hit.point, new Quaternion()) as GameObject;
          inst2.transform.parent = transform;
        }
      }
    }

    //Debug.Log(e.Hit.textureCoord);
  }

  // Update is called once per frame
	void Update () {
	
  }


  //  void OnTriggerEnter(Collider collider)
  //  {
  //    pos = transform.position;
  //    Vector3 hitPoint = Vector3.zero;
  //    if (!IsDefaultCollisionPoint)
  //    {
  //      RaycastHit hit;
  //      Physics.Raycast(transform.position, (collider.transform.position - pos).normalized, out hit);
  //      hitPoint = hit.point;
  //    }
  //    if (effect!=null) {
  //      var part = effect.GetComponent<ParticleSystem>();
  //      if (part!=null) {
  //        part.startSize = transform.lossyScale.x;
  //      }
  //      else {
  //        effect.transform.localScale = transform.lossyScale;
  //      }
  //      var inst1 = Instantiate(effect) as GameObject;
  //      inst1.transform.parent = gameObject.transform;
  //      inst1.transform.localPosition = transform.localPosition + FixInctancePosition;
  //      if (IsDefaultCollisionPoint) inst1.transform.localRotation = new Quaternion();
  //      else
  //        inst1.transform.LookAt(hitPoint);
  //      inst1.transform.Rotate(FixInctanceAngle);
  //      inst1.transform.localScale = transform.localScale * FixInctanceScalePercent / 100f;
  //    }
  //    if (explosion != null)
  //      {
  //        var inst2 = Instantiate(explosion, hitPoint, new Quaternion()) as GameObject;
  //          inst2.transform.parent = transform;
  //      }
  //  }
}
