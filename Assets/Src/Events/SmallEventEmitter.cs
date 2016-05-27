//using System;
//using System.Collections.Generic;
//using UnityEngine;

//public delegate void EventDelegate<T>(T e) where T : GameEvent;

//public class EventDesc<T> where T : GameEvent {
//    public Action<T> del;
//    public bool once;
//}

//public class SmallEventEmitter : MonoBehaviour {

//    private bool running;
//    private List<object> listeners;
//    private Queue<object> eventQueue;

//    void Awake() {
//        listeners = new List<object>();
//        eventQueue = new Queue<object>();
//    }

//    public void AddEventListener<T>(Action<T> listener) where T : GameEvent {
//        EventDesc<T> desc = new EventDesc<T>();
//        desc.once = false;
//        desc.del = listener;
//        listeners.Add(desc);
//    }

//    public void TriggerEvent<T>(T evt = null) where T : GameEvent {
//        for (int i = 0; i < listeners.Count; i++) {
//            EventDesc<T> desc = listeners[i] as EventDesc<T>;
//            if (desc == null) continue;
//            if(desc.once) {
//                listeners.RemoveAt(i);
//                i--;
//            }
//            desc.del.Invoke(evt);
//        }
//    }

//    public void QueueEvent<T>(T evt = null) where T : GameEvent) {
//        if(!running) {
//            running = true;
//            TriggerEvent(evt);
//            while(eventQueue.Count > 0) {
//                object e = eventQueue.Dequeue();

//            }
//            running = false;
//        }
//        else {
//            eventQueue.Enqueue(evt);
//        }
//    }

//}