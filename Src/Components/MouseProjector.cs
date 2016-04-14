using UnityEngine;
using System.Collections;

public class MouseProjector : MonoBehaviour {

    protected Projector projector;

	void Start () {
        projector = GetComponent<Projector>();
	}
	
	void Update () {
       Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(mouseRay, out hit, 1000, (1 << 9))) {
            transform.position = hit.point + Vector3.up * 3f;
        }
    }
}
