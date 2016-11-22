using UnityEngine;
using EntitySystem;
using System;

public class PointToPointBeam : MonoBehaviour {

    public Transform target;
    public Transform origin;
    public LineRenderer lineRenderer;
    public Transform muzzle;
    public Transform contact;
    public Transform particleScale;

    public void Initialize(Entity caster, Entity target) {
        this.target = target.transform;
        origin = caster.transform;
        lineRenderer = GetComponent<LineRenderer>();
    }

    public void Update() {
        Vector3 toTarget = (target.position - origin.position).normalized;
        muzzle.transform.position = origin.position;
        contact.transform.position = target.position - (toTarget * 0.5f);
        lineRenderer.SetPosition(0, origin.position + (toTarget * 0.5f));
        lineRenderer.SetPosition(1, contact.transform.position - (toTarget * 0.25f));
        particleScale.localScale = new Vector3(1, 1, target.DistanceTo(origin));
        particleScale.position = origin.position;

        particleScale.position = (target.position - origin.position) * 0.5f;
        particleScale.position += Vector3.up;
    }
}