using UnityEngine;
using AbilitySystem;
using System;

public class PointListTrail : MonoBehaviour, IAbilityInitializer {

    public Vector3[] pointList;
    public bool startAtFirstPoint;
    public float speed;
    public float arrivalThreshold = 0.1f;
    protected int currentIndex = 0;

    public void Initialize(Ability ability, PropertySet properties) {
        pointList = properties.Get<Vector3[]>("PointList");
        if (startAtFirstPoint) {
            transform.position = pointList[0];
            currentIndex = 1;
        }
        RaycastHit hit;
        if(Physics.Raycast(transform.position, Vector3.down, out hit, 100, (9 << 1))) {
            transform.position = hit.point + (Vector3.up * 0.1f);
        }
        //enable all children here so trail starts in the right place, otherwise it will teleport
        int children = transform.childCount;
        for(int i = 0; i < children; i++) {
            transform.GetChild(i).gameObject.SetActive(true);
        }
    }

    public void Update() { 
        if(currentIndex == pointList.Length) return;
        Vector3 currentPoint = pointList[currentIndex];
        transform.position = Vector3.MoveTowards(transform.position, pointList[currentIndex], speed * Time.deltaTime);
        if(transform.position.DistanceToSquared(pointList[currentIndex]) <= arrivalThreshold * arrivalThreshold) {
            currentIndex++;
        }
    }
}