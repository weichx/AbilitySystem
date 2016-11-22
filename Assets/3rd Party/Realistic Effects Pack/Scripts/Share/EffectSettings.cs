using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EffectSettings : MonoBehaviour
{
  public enum EffectTypeEnum
  {
    Projectile,
    AOE,
    Other
  };

  public enum DeactivationEnum
  {
    Deactivate,
    DestroyAfterCollision,
    DestroyAfterTime,
    Nothing
  };

	#if !UNITY_4_3 
	[Tooltip("Type of the effect")]	
	#endif 
    public EffectTypeEnum EffectType;

	#if !UNITY_4_3 
	[Tooltip("The radius of the collider is required to correctly calculate the collision point. For example, if the radius 0.5m, then the position of the collision is shifted on 0.5m relative motion vector.")]
	#endif 
	public float ColliderRadius = 0.2f;
  
	#if !UNITY_4_3 
	[Tooltip("The radius of the \"Area Of Damage (AOE)\"")]
#endif 
  public float EffectRadius = 0;

	#if !UNITY_4_3 
	[Tooltip("Get the position of the movement of the motion vector, and not to follow to the target.")]
	#endif 
  public bool UseMoveVector;
  
	#if !UNITY_4_3 
	[Tooltip("A projectile will be moved to the target (any object)")]
	#endif 
  public GameObject Target;

	#if !UNITY_4_3 
	[Tooltip("Motion vector for the projectile (eg Vector3.Forward)")]
	#endif 
  public Vector3 MoveVector = Vector3.forward;

	#if !UNITY_4_3 
	[Tooltip("The speed of the projectile")]
	#endif 
  public float MoveSpeed = 1;
  
	#if !UNITY_4_3 
	[Tooltip("Should the projectile have move to the target, until the target not reaches?")]
	#endif 
  public bool IsHomingMove;

	#if !UNITY_4_3 
	[Tooltip("Distance flight of the projectile, after which the projectile is deactivated and call a collision event with a null value \"RaycastHit\"")]
	#endif 
  public float MoveDistance = 20;

	#if !UNITY_4_3 
	[Tooltip("Allows you to smoothly activate / deactivate effects which have an indefinite lifetime")]
	#endif 
  public bool IsVisible = true;

	#if !UNITY_4_3 
	[Tooltip("Whether to deactivate or destroy the effect after a collision. Deactivation allows you to reuse the effect without instantiating, using \"effect.SetActive (true)\"")]
	#endif 
  public DeactivationEnum InstanceBehaviour = DeactivationEnum.Nothing;

	#if !UNITY_4_3 
	[Tooltip("Delay before deactivating effect. (For example, after effect, some particles must have time to disappear).")]
	#endif 
  public float DeactivateTimeDelay = 4;

	#if !UNITY_4_3 
	[Tooltip("Delay before deleting effect. (For example, after effect, some particles must have time to disappear).")]
	#endif 
  public float DestroyTimeDelay = 10;

	#if !UNITY_4_3 
	[Tooltip("Allows you to adjust the layers, which can interact with the projectile.")]
	#endif 
  public LayerMask LayerMask = -1;
  

  public event EventHandler<CollisionInfo> CollisionEnter;
  public event EventHandler EffectDeactivated;

  private GameObject[] active_key = new GameObject[100];
  private float[] active_value = new float[100];
  private GameObject[] inactive_Key = new GameObject[100];
  private float[] inactive_value = new float[100];
  private int lastActiveIndex;
  private int lastInactiveIndex;
  private int currentActiveGo;
  private int currentInactiveGo;
  private bool deactivatedIsWait;

  void Start()
  {
    if (InstanceBehaviour == DeactivationEnum.DestroyAfterTime) Destroy(gameObject, DestroyTimeDelay);
  }

  public void OnCollisionHandler(CollisionInfo e)
  {
    for (int i = 0; i < lastActiveIndex; i++)
    {
      Invoke("SetGoActive", active_value[i]);
    }
    for (int i = 0; i < lastInactiveIndex; i++)
    {
      Invoke("SetGoInactive", inactive_value[i]);
    }
    var handler = CollisionEnter;
    if (handler != null)
      handler(this, e);
    if (InstanceBehaviour == DeactivationEnum.Deactivate && !deactivatedIsWait)
    {
      deactivatedIsWait = true;
      Invoke("Deactivate", DeactivateTimeDelay);
    }
    if (InstanceBehaviour == DeactivationEnum.DestroyAfterCollision) Destroy(gameObject, DestroyTimeDelay);
  }
  public void OnEffectDeactivatedHandler()
  {
    var handler = EffectDeactivated;
    if (handler != null)
      handler(this, EventArgs.Empty);
  }

  public void Deactivate()
  {
    OnEffectDeactivatedHandler();
    gameObject.SetActive(false);
  }

  private void SetGoActive()
  {
    active_key[currentActiveGo].SetActive(false);
    ++currentActiveGo;
    if (currentActiveGo >= lastActiveIndex) currentActiveGo = 0;
  }

  private void SetGoInactive()
  {
    inactive_Key[currentInactiveGo].SetActive(true);
    ++currentInactiveGo;
    if (currentInactiveGo >= lastInactiveIndex) {
      currentInactiveGo = 0;
    }
  }

  public void OnEnable()
  {
    for (int i = 0; i < lastActiveIndex; i++)
    {
      active_key[i].SetActive(true);
    }
    for (int i = 0; i < lastInactiveIndex; i++)
    {
      inactive_Key[i].SetActive(false);
    }
    deactivatedIsWait = false;
  }

  public void OnDisable()
  {
    CancelInvoke("SetGoActive");
    CancelInvoke("SetGoInactive");
    CancelInvoke("Deactivate");
    currentActiveGo = 0;
    currentInactiveGo = 0;
  }

  public void RegistreActiveElement(GameObject go, float time)
  {
    active_key[lastActiveIndex] = go;
    active_value[lastActiveIndex] = time;
    ++lastActiveIndex;
  }

  public void RegistreInactiveElement(GameObject go, float time)
  {
    inactive_Key[lastInactiveIndex] = go;
    inactive_value[lastInactiveIndex] = time;
    ++lastInactiveIndex;
  }
}

public class CollisionInfo : EventArgs
{
  public RaycastHit Hit;
}