using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;

public class EventManager : MonoBehaviour {
    public bool limitQueueProcessTime = false;
    public float queueProcessTime = 0.0f;
    private Queue eventQueue = new Queue();

    public delegate void EventDelegate<T>(T e) where T : GameEvent;
    private delegate void NonGenericEventDelegate(GameEvent e);

    private static EventManager instance = null;

    private Dictionary<Type, NonGenericEventDelegate> delegates = new Dictionary<Type, NonGenericEventDelegate>();
    private Dictionary<Delegate, NonGenericEventDelegate> delegateLookup = new Dictionary<Delegate, NonGenericEventDelegate>();
    private Dictionary<Delegate, bool> onceLookups = new Dictionary<Delegate, bool>();

    public static EventManager Instance {
        get {
            if (instance == null) {
                var root = GameObject.FindGameObjectWithTag("MainEventManager");
                if (root == null) {
                    root = new GameObject("EventManager");
                    root.tag = "MainEventManager";
                    root.isStatic = true;
                }
                instance = root.GetComponent<EventManager>();
                if(instance == null) {
                    instance = root.AddComponent<EventManager>();
                }
            }
            return instance;
        }
    }

    private NonGenericEventDelegate AddDelegate<T>(EventDelegate<T> del) where T : GameEvent {
        // Early-out if we've already registered this delegate
        if (delegateLookup.ContainsKey(del)) {
            return null;
        }

        // Create a new non-generic delegate which calls our generic one.
        // This is the delegate we actually invoke.
        NonGenericEventDelegate internalDelegate = (GameEvent evt) => {
            del.Invoke(evt as T);
        };

        delegateLookup[del] = internalDelegate;
        Type type = typeof(T);

        NonGenericEventDelegate tempDel;
        if (delegates.TryGetValue(type, out tempDel)) {
            delegates[type] = tempDel += internalDelegate;
        }
        else {
            delegates[type] = internalDelegate;
        }

        return internalDelegate;
    }

    public void AddListener<T>(EventDelegate<T> del) where T : GameEvent {
        AddDelegate<T>(del);
    }

    public void AddListenerOnce<T>(EventDelegate<T> del) where T : GameEvent {
        NonGenericEventDelegate result = AddDelegate<T>(del);

        if (result != null) {
            // remember this is only called once
            onceLookups[result] = true;
        }
    }

    public void RemoveListener<T>(EventDelegate<T> del) where T : GameEvent {
        NonGenericEventDelegate internalDelegate;
        if (delegateLookup.TryGetValue(del, out internalDelegate)) {
            NonGenericEventDelegate tempDel;
            Type type = typeof(T);
            if (delegates.TryGetValue(type, out tempDel)) {
                tempDel -= internalDelegate;
                if (tempDel == null) {
                    delegates.Remove(type);
                }
                else {
                    delegates[type] = tempDel;
                }
            }

            delegateLookup.Remove(del);
        }
    }

    public void RemoveAll() {
        delegates.Clear();
        delegateLookup.Clear();
        onceLookups.Clear();
    }

    public bool HasListener<T>(EventDelegate<T> del) where T : GameEvent {
        return delegateLookup.ContainsKey(del);
    }

    public void TriggerEvent(GameEvent e) {
        NonGenericEventDelegate del;
        if (delegates.TryGetValue(e.GetType(), out del)) {
            del.Invoke(e);

            // remove listeners which should only be called once
            foreach (NonGenericEventDelegate k in delegates[e.GetType()].GetInvocationList()) {
                if (onceLookups.ContainsKey(k)) {
                    onceLookups.Remove(k);
                }
            }
        }
    }

    //Inserts the event into the current queue.
    public void QueueEvent(GameEvent evt) {
        eventQueue.Enqueue(evt);
    }

    public void QueueEvent<T>(T evt) {
        eventQueue.Enqueue(evt);
    }

    //Every update cycle the queue is processed, if the queue processing is limited,
    //a maximum processing time per update can be set after which the events will have
    //to be processed next update loop.
    void Update() {
        float timer = 0.0f;
        while (eventQueue.Count > 0) {

            if (limitQueueProcessTime && timer > queueProcessTime) { 
                    return;
            }

            TriggerEvent(eventQueue.Dequeue() as GameEvent);
            timer += Time.deltaTime;
        }
    }

    public void OnApplicationQuit() {
        RemoveAll();
        eventQueue.Clear();
        instance = null;
    }
}