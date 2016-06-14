using UnityEngine;
using System.Collections;

public class SetRandomStartPoint : MonoBehaviour
{
  public Vector3 RandomRange;
  public GameObject StartPointGo;
  public float Height = 10;
	// Use this for initialization

  private EffectSettings effectSettings;
  private bool isInitialized;
  private Transform tRoot;
  
  void GetEffectSettingsComponent(Transform tr)
  {
    var parent = tr.parent;
    if (parent != null)
    {
      effectSettings = parent.GetComponentInChildren<EffectSettings>();
      if (effectSettings == null)
        GetEffectSettingsComponent(parent.transform);
    }
  }

  void Start ()
  {
    GetEffectSettingsComponent(transform);
    if (effectSettings == null)
      Debug.Log("Prefab root or children have not script \"PrefabSettings\"");
    tRoot = effectSettings.transform;
    InitDefaultVariables();
    isInitialized = true;
  }

  void OnEnable()
  {
    if(isInitialized) InitDefaultVariables();
  }

  private void InitDefaultVariables()
  {
    if (GetComponent<ParticleSystem>()!=null)
      GetComponent<ParticleSystem>().Stop();
    var targetPos = effectSettings.Target.transform.position;
    var curentPos = new Vector3(targetPos.x, Height, targetPos.z);
    var randomX = Random.Range(0, (RandomRange.x) * 200) / 100 - RandomRange.x;
    var randomY = Random.Range(0, (RandomRange.y) * 200) / 100 - RandomRange.y;
    var randomZ = Random.Range(0, (RandomRange.z) * 200) / 100 - RandomRange.z;
    var randomPos = new Vector3(curentPos.x + randomX, curentPos.y + randomY, curentPos.z + randomZ);
    if (StartPointGo==null)
      tRoot.position = randomPos;
    else
      StartPointGo.transform.position = randomPos;
    if (GetComponent<ParticleSystem>()!=null)
      GetComponent<ParticleSystem>().Play();
  }

  // Update is called once per frame
	void Update () {
	}
}
