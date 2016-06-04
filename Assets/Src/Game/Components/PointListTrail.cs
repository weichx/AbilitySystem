using UnityEngine;
using AbilitySystem;
using System;

public class PointListTrail : MonoBehaviour {

    protected Vector3[] pointList;
    public float speed;
    public float arrivalThreshold = 0.1f;
    public float despawnTimeout = 3f;
    protected int currentIndex = 0;

    public void Initialize(Vector3[] pointList) {
        this.pointList = pointList;
        RaycastHit hit;
        if(Physics.Raycast(transform.position, Vector3.down, out hit, 100, (9 << 1))) {
            transform.position = hit.point + (Vector3.up * 0.1f);
        }
    }

    public void Update() {
        if (currentIndex == pointList.Length) {
            Destroy(gameObject, despawnTimeout);
            return;
        }
        transform.position = Vector3.MoveTowards(transform.position, pointList[currentIndex], speed * Time.deltaTime);
        if(transform.position.DistanceToSquared(pointList[currentIndex]) <= arrivalThreshold * arrivalThreshold) {
            currentIndex++;
        }
    }
}