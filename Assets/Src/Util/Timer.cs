public class Timer {

    private float startTime;
    private float timeout;

    private static float TotalElapsedTime = 0;

    public static void Tick(float deltaTime) {
        TotalElapsedTime += deltaTime;
    }

    public Timer() : this(-1) { }

    public Timer(float timeout) {
        this.timeout = timeout;
        startTime = TotalElapsedTime;
    }

    public bool ReadyWithReset(float resetTimeout = -1) {
        if (Ready) {
            Reset((resetTimeout >= 0) ? resetTimeout : timeout);
            return true;
        }
        else {
            return false;
        }
    }

    public void Reset(float timeout = -1) {
        this.timeout = timeout;
        startTime = TotalElapsedTime;
    }

    public bool Ready {
        get { return timeout >= 0f && TotalElapsedTime - startTime > timeout; }
    }

    public float CompletedPercent {
        get {
            if (timeout <= 0) return 0;
            return UnityEngine.Mathf.Clamp((TotalElapsedTime - startTime) / timeout, 0, float.MaxValue);
        }
    }

    public float TimeToReady {
        get {
            if (timeout <= 0) return 0;
            return timeout - (TotalElapsedTime - startTime);
        }
    }

    public float Timeout {
        get { return timeout; }
        set { timeout = value; }
    }

    public float ElapsedTime {
        get { return TotalElapsedTime - startTime; }
    }

    public float Timestamp {
        get { return ElapsedTime; }
    }

    public static float GetTimestamp {
        get {
            return TotalElapsedTime;
        }
    }
}