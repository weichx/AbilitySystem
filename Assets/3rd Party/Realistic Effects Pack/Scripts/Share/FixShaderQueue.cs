using UnityEngine;
using System.Collections;

public class FixShaderQueue : MonoBehaviour
{

  public int AddQueue = 1;
	// Use this for initialization
	void Start ()
	{
	  if (GetComponent<Renderer>()!=null)
	    GetComponent<Renderer>().sharedMaterial.renderQueue += AddQueue;
	  else
	    Invoke("SetProjectorQueue", 0.1f);
	}

  void SetProjectorQueue()
  {
    GetComponent<Projector>().material.renderQueue += AddQueue;
  }
	
	// Update is called once per frame
	void Update () {
	
	}
}
