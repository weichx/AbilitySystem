using UnityEngine;
using System.Collections.Generic;
using Intelligence;

public class PointListTrail : MonoBehaviour, IContextAware {

    protected List<Vector3> pointList;
    public float speed;
    public float arrivalThreshold = 0.1f;
    public float despawnTimeout = 3f;
    protected int currentIndex = 0;


    public void SetContext(Context ctx) {
        MultiPointContext context = ctx as MultiPointContext;
        pointList = context.points;
        RaycastHit hit;
        transform.position = pointList[0];
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 100)) {
            transform.position = hit.point + (Vector3.up * 0.1f);
        }
    }

    public void Update() {
        if (currentIndex == pointList.Count) {
            Destroy(gameObject, despawnTimeout);
            return;
        }
        transform.position = Vector3.MoveTowards(transform.position, pointList[currentIndex], speed * Time.deltaTime);
        if(transform.position.DistanceToSquared(pointList[currentIndex]) <= arrivalThreshold * arrivalThreshold) {
            currentIndex++;
        }
    }

}