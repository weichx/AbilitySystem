using UnityEngine;
using System.Collections;

public class FaceDownAlways : MonoBehaviour {
	
	void Update () {
        transform.LookAt(transform.position + Vector3.down);
	}
}
