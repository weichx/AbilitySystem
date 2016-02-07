using UnityEngine;
using System.Collections;

public class ProjectorOrder : MonoBehaviour {

    public Projector[] projectors;

	void OnEnable () {
        if (projectors == null) return;
        for (int i = 0; i < projectors.Length; i++) {
            projectors[i].gameObject.SetActive(true);
        }
	}

    void OnDisable() {
        if (projectors == null) return;
        for (int i = 0; i < projectors.Length; i++) {
            projectors[i].gameObject.SetActive(false);
        }
    }
	
}
