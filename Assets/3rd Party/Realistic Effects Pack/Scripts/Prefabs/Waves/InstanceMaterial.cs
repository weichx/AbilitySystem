using System.Linq;
using UnityEngine;
using System.Collections;

public class InstanceMaterial : MonoBehaviour
{

  public Material Material;
	// Use this for initialization
	void Start ()
	{
    GetComponent<Renderer>().material = Material;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
