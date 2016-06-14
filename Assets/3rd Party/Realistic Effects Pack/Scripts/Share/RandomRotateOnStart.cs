using UnityEngine;
using System.Collections;

public class RandomRotateOnStart : MonoBehaviour
{
  public Vector3 NormalizedRotateVector = new Vector3(0, 1, 0);

  private Transform t;
  private bool isInitialized;
	// Use this for initialization
	void Start ()
	{
	  t = transform;
    t.Rotate(NormalizedRotateVector * Random.Range(0,360));
	  isInitialized = true;
	}

  void OnEnable()
  {
    if (isInitialized) t.Rotate(NormalizedRotateVector * Random.Range(0, 360));
  }
}
