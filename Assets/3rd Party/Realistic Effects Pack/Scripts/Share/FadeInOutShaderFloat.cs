using UnityEngine;
using System.Collections;

public class FadeInOutShaderFloat : MonoBehaviour {
    public string PropertyName = "_CutOut";
    public float MaxFloat = 1;
    public float StartDelay = 0;
    public float FadeInSpeed = 0;
    public float FadeOutDelay = 0;
    public float FadeOutSpeed = 0;
    public bool FadeOutAfterCollision;
    public bool UseHideStatus;

    private Material OwnMaterial;
    private Material mat;
    private float oldFloat, currentFloat;
    private bool canStart, canStartFadeOut, fadeInComplited, fadeOutComplited, previousFrameVisibleStatus;
    private bool isCollisionEnter;
    private bool isStartDelay, isIn, isOut;
    private EffectSettings effectSettings;
    private bool isInitialized;

    #region Non-public methods

    private void GetEffectSettingsComponent(Transform tr) {
        var parent = tr.parent;
        if (parent != null) {
            effectSettings = parent.GetComponentInChildren<EffectSettings>();
            if (effectSettings == null)
                GetEffectSettingsComponent(parent.transform);
        }
    }

    private void Start() {
        GetEffectSettingsComponent(transform);
        if (effectSettings != null)
            effectSettings.CollisionEnter += prefabSettings_CollisionEnter;

        InitMaterial();
    }

    public void UpdateMaterial(Material instanceMaterial) {
        mat = instanceMaterial;
        InitMaterial();
    }

    private void InitMaterial() {
        if (isInitialized) return;
        if (GetComponent<Renderer>() != null) mat = GetComponent<Renderer>().material;
        else {
            var lineRenderer = GetComponent<LineRenderer>();
            if (lineRenderer != null) mat = lineRenderer.material;
            else {
                var projector = GetComponent<Projector>();
                if (projector != null) {
                    if (!projector.material.name.EndsWith("(Instance)"))
                        projector.material = new Material(projector.material) { name = projector.material.name + " (Instance)" };
                    mat = projector.material;
                }
            }
        }
        if (mat == null) return;

        isStartDelay = StartDelay > 0.001f;
        isIn = FadeInSpeed > 0.001f;
        isOut = FadeOutSpeed > 0.001f;
        InitDefaultVariables();
        isInitialized = true;
    }

    private void InitDefaultVariables() {
        fadeInComplited = false;
        fadeOutComplited = false;
        canStartFadeOut = false;
        canStart = false;
        isCollisionEnter = false;
        oldFloat = 0;
        currentFloat = MaxFloat;

        if (isIn) currentFloat = 0;
        mat.SetFloat(PropertyName, currentFloat);

        if (isStartDelay) Invoke("SetupStartDelay", StartDelay);
        else canStart = true;
        if (!isIn) {
            if (!FadeOutAfterCollision)
                Invoke("SetupFadeOutDelay", FadeOutDelay);
            oldFloat = MaxFloat;
        }
    }

    private void prefabSettings_CollisionEnter(object sender, CollisionInfo e) {
        isCollisionEnter = true;
        if (!isIn && FadeOutAfterCollision) Invoke("SetupFadeOutDelay", FadeOutDelay);
    }


    void OnEnable() {
        if (isInitialized) InitDefaultVariables();
    }

    private void SetupStartDelay() {
        canStart = true;
    }

    private void SetupFadeOutDelay() {
        canStartFadeOut = true;
    }

    private void Update() {
        if (!canStart)
            return;

        if (effectSettings != null && UseHideStatus) {
            if (!effectSettings.IsVisible && fadeInComplited)
                fadeInComplited = false;
            if (effectSettings.IsVisible && fadeOutComplited)
                fadeOutComplited = false;
        }

        if (UseHideStatus) {
            if (isIn) {
                if (effectSettings != null && effectSettings.IsVisible && !fadeInComplited)
                    FadeIn();
            }
            if (isOut) {
                if (effectSettings != null && !effectSettings.IsVisible && !fadeOutComplited)
                    FadeOut();
            }
        }
        else if (!FadeOutAfterCollision) {
            if (isIn) {
                if (!fadeInComplited)
                    FadeIn();
            }
            if (isOut && canStartFadeOut) {
                if (!fadeOutComplited)
                    FadeOut();
            }
        }
        else {
            if (isIn) {
                if (!fadeInComplited)
                    FadeIn();
            }
            if (isOut && isCollisionEnter && canStartFadeOut && !fadeOutComplited)
                FadeOut();
        }
    }


    private void FadeIn() {
        currentFloat = oldFloat + Time.deltaTime / FadeInSpeed * MaxFloat;
        if (currentFloat >= MaxFloat) {
            fadeInComplited = true;
            currentFloat = MaxFloat;
            Invoke("SetupFadeOutDelay", FadeOutDelay);
        }

        mat.SetFloat(PropertyName, currentFloat);
        oldFloat = currentFloat;
    }

    private void FadeOut() {
        currentFloat = oldFloat - Time.deltaTime / FadeOutSpeed * MaxFloat;
        if (currentFloat <= 0) {
            currentFloat = 0;
            fadeOutComplited = true;
        }

        mat.SetFloat(PropertyName, currentFloat);
        oldFloat = currentFloat;
    }

    #endregion
}