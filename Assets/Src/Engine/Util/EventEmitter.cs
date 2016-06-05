using System;
using System.Collections.Generic;

public abstract class GameEvent { }

///<summary>A basic event emitter with an async queue</summary>
public class EventEmitter {

    private Dictionary<Type, Delegate> dict;
    private Queue<EventDesc> queue;

    private class EventDesc {
        public Type type;
        public GameEvent evt;
    }

    public EventEmitter() {
        queue = new Queue<EventDesc>();
        dict = new Dictionary<Type, Delegate>();
    }

    public void AddEventListener<T>(Action<T> del) where T : GameEvent {
        if (del == null) {
            return;
        }
        Type type = typeof(T);
        Delegate evtDel;
        if (dict.TryGetValue(type, out evtDel)) {
            Action<T> evtDelReal = evtDel as Action<T>;
            evtDelReal += del;
        }
        else {
            dict[type] = del;
        }
    }

    public void RemoveEventListener<T>(Action<T> del) where T : GameEvent {
        if (del == null) {
            return;
        }
        Delegate evtDel;
        Type type = typeof(T);
        if (dict.TryGetValue(type, out evtDel)) {
            Action<T> evtDelReal = evtDel as Action<T>;
            evtDelReal -= del;
        }
    }


    public void TriggerEvent<T>(T evt = null) where T : GameEvent {
        EventDesc desc = new EventDesc();
        desc.type = typeof(T);
        desc.evt = evt;
        queue.Enqueue(desc);
    }

    public void FlushQueue() {
        //store count now because flushing the queue may add new events
        int total = queue.Count;
        for (int i = 0; i < total; i++) {
            EventDesc desc = queue.Dequeue();
            if (desc == null) {
                continue;
            }
            Action<GameEvent> del = dict.Get(desc.type) as Action<GameEvent>;
            if (del != null) {
                del.Invoke(desc.evt);
            }
        }
    }

}