using UnityEngine;
using System.Collections.Generic;

namespace EntitySystem.Timing {

    public class TimeManager : MonoBehaviour {
        public static Timer GameTimer = new Timer();

        public static float Timestamp { get { return GameTimer.Timestamp; } }

        void Update() {
            Timer.Tick(Time.deltaTime);
        }
    }

}