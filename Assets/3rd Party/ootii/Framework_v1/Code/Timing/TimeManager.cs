using System.Collections;
using UnityEngine;

namespace com.ootii.Timing
{
    /// <summary>
    /// Provides access to additional timing functions
    /// </summary>
    public class TimeManager
    {
        /// <summary>
        /// We want everything to run at 60FPS. This provides us
        /// a way to scale things like constant rotations should we be running
        /// slower or faster. The value is (Time.deltaTime / 0.01666f).
        /// So, if we are running at 30fps (0.0333/0.01666) = 2
        /// If we are running at 120fps (0.00833 / 0.01666) = 0.5
        /// </summary>
        public static float Relative60FPSDeltaTime = 1f;

        /// <summary>
        /// Number of samples used with the time manager
        /// </summary>
        private static int mSampleCount = 30;
        public static int SampleCount
        {
            get { return mSampleCount; }
        }

        /// <summary>
        /// Stores the average delta time for the game. We use this to
        /// smooth out delta time and avoid spikes.
        /// </summary>
        public static float _AverageDeltaTime = Time.fixedDeltaTime;
        public static float AverageDeltaTime
        {
            get { return _AverageDeltaTime; }
        }

        /// <summary>
        /// Returns the current delta time unless it exceeds the current
        /// average. In which case, we return the average of the sampled values.
        /// </summary>
        public static float SmoothedDeltaTime
        {
            get
            {
                if (Time.deltaTime <= _AverageDeltaTime)
                {
                    return Time.deltaTime;
                }
                else
                {
                    return _AverageDeltaTime;
                }
            }
        }

        /// <summary>
        /// This stub is a game object that will update the input over time. The
        /// stub can be placed by the scene builder or generated automatically.
        /// </summary>
        public static TimeManagerCore Core;

        /// <summary>
        /// Creates the number of samples to use
        /// </summary>
        private static float[] mSamples = new float[mSampleCount];

        /// <summary>
        /// Current index for the sample
        /// </summary>
        private static int mSampleIndex = 0;

        /// <summary>
        /// Static constructor is called at most one time, before any 
        /// instance constructor is invoked or member is accessed. 
        /// </summary>
        static TimeManager()
        {
            // Check to see if an input manager stub exists. If so, associate it.
            // If not, we'll need to create one. The core is what manages our update cycle.
            Core = Component.FindObjectOfType<TimeManagerCore>();
            if (Core == null)
            {
#pragma warning disable 0414

                GameObject lCoreGameObject = new GameObject("TimeManagerCore", typeof(TimeManagerCore));
                lCoreGameObject.hideFlags = HideFlags.HideInHierarchy;

                Core = lCoreGameObject.GetComponent<TimeManagerCore>();

#pragma warning restore 0414
            }
        }

        /// <summary>
        /// Called once to initialize the manager (once the core is created)
        /// </summary>
        public static void Initialize()
        {
            // Initialize the samples
            for (int i = 0; i < mSampleCount; i++)
            {
                mSamples[i] = Time.deltaTime;
            }
        }

        /// <summary>
        /// Called once per frame at the end.
        /// </summary>
        public static void Update()
        {
            // Calculate our frame-time relative to 60FPS
            Relative60FPSDeltaTime = (Time.deltaTime / 0.01666f);

            // Calculate the smoothed time
            mSamples[mSampleIndex++] = Time.deltaTime;
            if (mSampleIndex >= mSampleCount) { mSampleIndex = 0; }

            float lTotal = 0f;
            for (int i = 0; i < mSampleCount; i++)
            {
                lTotal = lTotal + mSamples[i];
            }

            _AverageDeltaTime = lTotal / mSampleCount;
        }
    }

    /// <summary>
    /// Used by the TimeHelperCore to hook into the unity update process. This allows us
    /// to update the input and track old values
    /// </summary>
    public class TimeManagerCore : MonoBehaviour
    {
        /// <summary>
        /// Raised first when the object comes into existance. Called
        /// even if script is not enabled.
        /// </summary>
        void Awake()
        {
            // Don't destroyed automatically when loading a new scene
            DontDestroyOnLoad(gameObject);

            // Initialize the manager
            TimeManager.Initialize();
        }

        /// <summary>
        /// Called after the Awake() and before any update is called.
        /// </summary>
        public IEnumerator Start()
        {
            // Create the coroutine here so we don't re-create over and over
            WaitForEndOfFrame lWaitForEndOfFrame = new WaitForEndOfFrame();

            // Loop endlessly so we can process the timings at the end of each frame in
            // preperation for the next
            while (true)
            {
                yield return lWaitForEndOfFrame;
                TimeManager.Update();
            }
        }
    }
}
