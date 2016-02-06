public class Timer {

    private float startTime;
    private float timeout;

    private static float TotalElapsedTime = 0;

    public static void Tick(float deltaTime) {
        TotalElapsedTime += deltaTime;
    }

    public Timer(float timeout = 1) {
        this.timeout = timeout;
        startTime = TotalElapsedTime;
    }

    public bool ReadyWithReset(float timeout) {
        if (Ready) {
            Reset(timeout);
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
}