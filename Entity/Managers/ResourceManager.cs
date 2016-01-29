using UnityEngine;
using System.Collections.Generic;
using System;

public class Resource {
    
    //add optional regeneration rate / formula

    protected float value;
    protected float min;
    protected float max;
    public ModifiableAttribute minAttr;
    public ModifiableAttribute maxAttr;
    public readonly string name;
    
    //resource name + "Cost"

    public Resource() {
        minAttr = new ModifiableAttribute("Min", 0f);
        maxAttr = new ModifiableAttribute("Max", float.MaxValue);
    }

    public void Set(float value) {

    }

    public void Increase(float amount) {
        value = Mathf.Clamp(value + amount, minAttr.Update(), maxAttr.Update());
    }

    public void Decrease(float amount) {

    }

    public float Value {
        get { return value; }
    }

    public float TrueMaximum {
        get { return max; }
    }

    public float TrueMinimum {
        get { return min; }
    }

    public float Maximum {
        get { return 0f; }
    }

    public float Minimum {
        get { return 0f; }
    }

    private struct ThresholdWatcher {
        public float threshold;
        public bool wasBelowLastFrame;
        public bool wasAboveLastFrame;
    }
}

//need an automatic linkage between attribute resource costs and resources
//I'd like to keep resources strongly checked (no strings)
//one resource of each type is applicable, unlike attributes
//i might have many attributes that implement some resource interface
//throw error? ignore? subtract twice?

//entity.GetResource<Mana>();
//entity.AddResource<Mana>();
//entity.AddResource<Energy>();


public class ResourceManager : MonoBehaviour {

    protected Entity entity;
    protected Dictionary<string, Resource> resources;

    public void Awake() {
        resources = new Dictionary<string, Resource>();
    }

    public void Start() {
        entity = GetComponent<Entity>();
    }

    private void ConsumeResources(AbilityUsedEvent e) {
        var ability = e.ability;
        foreach (var resourceName in resources.Keys) {
            var resource = resources[resourceName];
            resource.Decrease(ability.GetAttributeValue(resourceName + "Cost"));
        }
    }

    public Dictionary<string, Resource> Resources {
        get { return resources; }
    }

    public void Increase<T>(float amount) {

    }

    public void Decrease<T>(float amount) {

    }

    public void Increase(string resource, float amount) {

    }

    public void Decrease(string resource, float amount) {

    }

    //these checks are done at the top of a frame
    public void OnResourceBelowThreshold<T>(float threshold) where T : Resource {

    }

    public void OnResourceAboveThreshold() {

    }

    public void OnResourceReachedMinimum() {

    }

    public void OnResourceReachedMaximum() {

    }

    public void SetPercentage<T>() {

    }

    public float GetPercentage<T>() {
        return 0f;
    }

    //set min
    //set max
    //get value
    //has resource type
    //set value
    //ConsumeResource
    //resourceManager.Change<Health>(value);
    //resourceManager.Set<Health>(value);
    //entity.resourceManager.SetValue<Health>(100f);
    //entity.resourceManager.ChangeValue<Health>(3f);

    //rm.Adust<Health>(5f);
    public void Add<T>(float value) {

    }

    public void Change<T>(string resourceName, float value) where T : ResourceType {

    }

    public void AddResource<T>() where T : Resource {

    }

    public void RemoveResource<T>() where T : Resource {

    }

    public bool HasResource<T>() where T : Resource {
        return resources.ContainsKey(typeof(T).Name);
    }

    public Resource GetResource<T>() where T : Resource {
        return null;
    }

    public void GetResourceSnapshot() {
        //
    }
}