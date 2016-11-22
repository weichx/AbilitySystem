/*
http://www.cgsoso.com/forum-257-1.html

CG搜搜 Unity3d 插件团购

CGSOSO 主打游戏开发，影视设计等CG资源素材。

每日Unity3d插件免费更新，仅限下载试用，如若商用，请务必官网购买！
*/

using System.Runtime.InteropServices;
using UnityEngine;
using Random = UnityEngine.Random;

public class LineProjectileCollisionBehaviour : MonoBehaviour {
    public GameObject EffectOnHit;
    public GameObject EffectOnHitObject;
    public GameObject ParticlesScale;
    public GameObject GoLight;
    public bool IsCenterLightPosition;
    public LineRenderer[] LineRenderers;

    private EffectSettings effectSettings;
    private Transform t, tLight, tEffectOnHit, tParticleScale;
    private RaycastHit hit;
    private RaycastHit oldRaycastHit;
    private bool isInitializedOnStart;
    private bool frameDroped;
    private ParticleSystem[] effectOnHitParticles;
    private EffectSettings effectSettingsInstance;

    void GetEffectSettingsComponent(Transform tr) {
        var parent = tr.parent;
        if (parent != null) {
            effectSettings = parent.GetComponentInChildren<EffectSettings>();
            if (effectSettings == null)
                GetEffectSettingsComponent(parent.transform);
        }
    }

    // Use this for initialization
    private void Start() {
        t = transform;
        if (EffectOnHit != null) {
            tEffectOnHit = EffectOnHit.transform;
            effectOnHitParticles = EffectOnHit.GetComponentsInChildren<ParticleSystem>();
        }
        if (ParticlesScale != null) tParticleScale = ParticlesScale.transform;
        GetEffectSettingsComponent(t);
        if (effectSettings == null)
            Debug.Log("Prefab root or children have not script \"PrefabSettings\"");
        if (GoLight != null) tLight = GoLight.transform;
        InitializeDefault();
        isInitializedOnStart = true;
    }

    void OnEnable() {
        if (isInitializedOnStart) InitializeDefault();
    }

    void OnDisable() {
        CollisionLeave();
    }

    private void InitializeDefault() {
        hit = new RaycastHit();
        frameDroped = false;
    }

    private void Update() {
        if (!frameDroped) {
            frameDroped = true;
            return;
        }

        var endPoint = t.position + t.forward * effectSettings.MoveDistance;

        RaycastHit raycastHit;
        if (Physics.Raycast(t.position, t.forward, out raycastHit, effectSettings.MoveDistance + 1, effectSettings.LayerMask)) {
            hit = raycastHit;
            endPoint = raycastHit.point;
            if (oldRaycastHit.collider != hit.collider) {
                CollisionLeave();
                oldRaycastHit = hit;
                CollisionEnter();

                if (EffectOnHit != null) {
                    foreach (var effectOnHitParticle in effectOnHitParticles) {
                        effectOnHitParticle.Play();
                    }
                }
            }
            if (EffectOnHit != null) tEffectOnHit.position = hit.point - t.forward * effectSettings.ColliderRadius;
        }
        else {
            if (EffectOnHit != null)
                foreach (var effectOnHitParticle in effectOnHitParticles) {
                    effectOnHitParticle.Stop();
                }
        }

        if (EffectOnHit != null) tEffectOnHit.LookAt(hit.point + hit.normal);

        if (IsCenterLightPosition && GoLight != null)
            tLight.position = (t.position + endPoint) / 2;

        foreach (var additionalLineRenderer in LineRenderers) {
            additionalLineRenderer.SetPosition(0, endPoint);
            additionalLineRenderer.SetPosition(1, t.position);
        }
        if (ParticlesScale != null) {
            var distance = Vector3.Distance(t.position, endPoint) / 2;
            tParticleScale.localScale = new Vector3(distance, 1, 1);
        }
    }

    private void CollisionEnter() {
        if (EffectOnHitObject != null && hit.transform != null) {
            var addMat = hit.transform.GetComponentInChildren<AddMaterialOnHit>();
            effectSettingsInstance = null;
            if (addMat != null)
                effectSettingsInstance = addMat.gameObject.GetComponent<EffectSettings>();
            if (effectSettingsInstance != null)
                effectSettingsInstance.IsVisible = true;
            else {
                var hitGO = hit.transform;
                var renderer = hitGO.GetComponentInChildren<Renderer>();
                var effect = Instantiate(EffectOnHitObject) as GameObject;
                effect.transform.parent = renderer.transform;
                effect.transform.localPosition = Vector3.zero;
                effect.GetComponent<AddMaterialOnHit>().UpdateMaterial(hit);
                effectSettingsInstance = effect.GetComponent<EffectSettings>();
            }
        }
        effectSettings.OnCollisionHandler(new CollisionInfo { Hit = hit });
    }

    void CollisionLeave() {
        if (effectSettingsInstance != null) {
            effectSettingsInstance.IsVisible = false;
        }
    }
}


