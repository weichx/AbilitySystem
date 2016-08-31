#define ENABLE_PROFILING

using System;
using System.Collections.Generic;
using System.Diagnostics;
using com.ootii.Utilities.Debug;

namespace com.ootii.Utilities
{
    /// <summary>
    /// Profiler is used to measure the amount of time (milliseconds) it takes specific
    /// tasks.  The profiler can be started seperately or run through
    /// the application
    /// </summary>
    public class Profiler
    {
        /// <summary>
        /// Tag to help identify the profiler */
        /// </summary> 
        public string Tag = "";

        /// <summary>
        /// Spacing used before the tag */
        /// </summary> 
        private string mSpacing = "";

        /// <summary>
        /// Number of times the profiler was run */
        /// </summary> 
        private int mCount = 0;

        /// <summary>
        /// Tracks the time between start and stop */
        /// </summary> 
        private float mRunTime = 0;

        /// <summary>
        /// Total time the profiler has been running */
        /// </summary> 
        private float mTotalTime = 0;

        /// <summary>
        /// Sortest time on a single run */
        /// </summary>
        private float mMinTime = 0;

        /// <summary>
        /// Max time on a single run */
        /// </summary> 
        private float mMaxTime = 0;

        /// <summary>
        /// Manages the internal time 
        /// </summary>
        private Stopwatch mTimer = new Stopwatch();

        /// <summary>
        /// Allows us to convert ticks to milliseconds
        /// </summary>
        private float mTicksPerMillisecond = 0f;

        /// <summary>
        /// Constructor for the profiler
        /// </summary>
        /// <param name="rTag"></param>
        /// <param name="rSpacing"></param>
        public Profiler(string rTag)
        {
            Tag = rTag;

            mTicksPerMillisecond = (float)TimeSpan.TicksPerMillisecond;

            mMinTime = int.MaxValue;
            mMaxTime = int.MinValue;
        }

        /// <summary>
        /// Constructor for the profiler
        /// </summary>
        /// <param name="rTag"></param>
        /// <param name="rSpacing"></param>
        public Profiler(string rTag, string rSpacing)
        {
            Tag = rTag;
            mSpacing = rSpacing;

            mTicksPerMillisecond = (float)TimeSpan.TicksPerMillisecond;

            mMinTime = int.MaxValue;
            mMaxTime = int.MinValue;
        }

        /// <summary>
        /// Resets the profilder data
        /// </summary>
        public void Reset()
        {
            mCount = 0;
            mRunTime = 0;
            mTotalTime = 0;
            mMinTime = 0;
            mMaxTime = 0;
        }

        /// <summary>
        /// Average time (in milliseconds) the profiler has run for
        /// </summary>
        public float AverageTime
        {
            get
            {
                if (mCount == 0) { return 0; }
                return mTotalTime / mCount;
            }
        }

        /// <summary>
        /// Minimum time (in milliseconds) the profiler has run
        /// </summary>
        public float MinTime
        {
            get { return mMinTime; }
        }

        /// <summary>
        /// Maximum time (in milliseconds) the profiler has run
        /// </summary>
        public float MaxTime
        {
            get { return mMaxTime; }
        }

        /// <summary>
        /// Total time (in milliseconds) of all the profiles
        /// </summary>
        public float TotalTime
        {
            get { return mTotalTime; }
        }

        /// <summary>
        /// Time (in milliseconds) of the last profile
        /// </summary>
        public float Time
        {
            get { return mRunTime; }
        }

        /// <summary>
        /// Elapsed time (in milliseconds) of the current timer
        /// </summary>
        /// <value>The elapsed time.</value>
        public float ElapsedTime
        {
            get
            {
                if (mTimer.IsRunning)
                {
                    return mTimer.ElapsedTicks / mTicksPerMillisecond;
                }
                else
                {
                    return mRunTime;
                }
            }
        }

        /// <summary>
        /// Returns the number of profile counts
        /// </summary>
        public int Count
        {
            get { return mCount; }
        }

        /// <summary>
        /// Starts the profile
        /// </summary>
        public void Start()
        {
            mTimer.Reset();
            mTimer.Start();
        }

        /// <summary>
        /// Stops the profiler and returns the run time (in milliseconds)
        /// </summary>
        /// <returns>Milliseconds the profiler ran for</returns>
        public float Stop()
        {
            mTimer.Stop();
            mRunTime = mTimer.ElapsedTicks / mTicksPerMillisecond;

            mTotalTime = mTotalTime + mRunTime;
            if (mMinTime == 0 || mRunTime < mMinTime) { mMinTime = mRunTime; }
            if (mMaxTime == 0 || mRunTime > mMaxTime) { mMaxTime = mRunTime; }

            mCount++;

            return mRunTime;
        }

        /// <summary>
        /// Rturns a string that spits out the profiler information.
        /// </summary>
        /// <returns>String that is the profiler results</returns>
        public override string ToString()
        {
            return String.Format("{0} {1} - time:{2:f4}ms cnt:{3} avg:{4:f4}ms min:{5:f4}ms max:{6:f4}ms", mSpacing, Tag, mRunTime, mCount, AverageTime, mMinTime, mMaxTime);
        }

        // ******************************** STATIC FUNCTIONS ********************************

        /// <summary>
        /// Holds the various profilers
        /// </summary>
        private static Dictionary<string, Profiler> sProfilers = new Dictionary<string, Profiler>();

        /// <summary>
        /// Starts the specified profiler
        /// </summary>
        /// <param name="rProfiler">Profiler to start</param>
        public static Profiler Start(string rProfiler)
        {
#if ENABLE_PROFILING
            if (!sProfilers.ContainsKey(rProfiler)) { sProfilers.Add(rProfiler, new Profiler(rProfiler, "")); }
            sProfilers[rProfiler].Start();

            return sProfilers[rProfiler];
#else
            return null;
#endif
        }

        /// <summary>
        /// Starts the specified profiler
        /// </summary>
        /// <param name="rProfiler">Profiler to start</param>
        public static Profiler Start(string rProfiler, string rSpacing)
        {
#if ENABLE_PROFILING
            if (!sProfilers.ContainsKey(rProfiler)) { sProfilers.Add(rProfiler, new Profiler(rProfiler, rSpacing)); }
            sProfilers[rProfiler].Start();

            return sProfilers[rProfiler];
#else
            return null;
#endif
        }

        /// <summary>
        /// Stops the specified profiler
        /// </summary>
        /// <param name="rProfiler">Profiler to stop</param>
        /// <returns>Time the profiler ran for (in milliseconds)</returns>
        public static float Stop(string rProfiler)
        {
#if ENABLE_PROFILING
            if (!sProfilers.ContainsKey(rProfiler)) { return 0; }
            return sProfilers[rProfiler].Stop();
#else
            return 0f;
#endif
        }

        /// <summary>
        /// Current time (or last time if it's not running) of the specified profiler
        /// </summary>
        /// <param name="rProfiler"></param>
        /// <returns>Time in seconds</returns>
        public static float ProfilerTime(string rProfiler)
        {
#if ENABLE_PROFILING
            if (!sProfilers.ContainsKey(rProfiler)) { return 0; }
            return sProfilers[rProfiler].ElapsedTime;
#else
            return 0f;
#endif
        }

        /// <summary>
        /// Grabs the string representing the profiler data
        /// </summary>
        /// <param name="rProfiler">Profiler to print</param>
        /// <returns>String representing the profiler's data</returns>
        public static string ToString(string rProfiler)
        {
#if ENABLE_PROFILING

            if (rProfiler.Length == 0)
            {
                float lTotal = 0f;
                float lAvgTotal = 0f;
                foreach (Profiler lProfiler in sProfilers.Values)
                {
                    lTotal += lProfiler.Time;
                    lAvgTotal += lProfiler.AverageTime;
                }

                string lResult = String.Format("Profiles - Time:{0:f4}ms Avg:{1:f4}ms\r\n", lTotal, lAvgTotal);

                foreach (Profiler lProfiler in sProfilers.Values)
                {
                    lResult += String.Format("{0} Prc:{1:f3} AvgPrc:{2:f3}\r\n", lProfiler.ToString(), (lProfiler.Time / lTotal), (lProfiler.AverageTime / lAvgTotal));
                }

                return lResult;
            }
            else
            {
                if (!sProfilers.ContainsKey(rProfiler)) { return ""; }
                return sProfilers[rProfiler].ToString();
            }

#else
            return "";
#endif
        }

        /// <summary>
        /// Grabs the string representing the profiler data
        /// </summary>
        /// <param name="rProfiler">Profiler to print</param>
        /// <returns>String representing the profiler's data</returns>
        public static void ScreenWrite(string rProfiler, int rLine)
        {
            if (rProfiler.Length == 0)
            {
                float lTotal = 0f;
                float lAvgTotal = 0f;
                foreach (Profiler lProfiler in sProfilers.Values)
                {
                    lTotal += lProfiler.Time;
                    lAvgTotal += lProfiler.AverageTime;
                }

                //string lResult = String.Format("Profiles - Time:{0:f4}ms Avg:{1:f4}ms\r\n", lTotal, lAvgTotal);

                int lLine = 0;
                foreach (Profiler lProfiler in sProfilers.Values)
                {
                    Log.ScreenWrite(String.Format("{0} Prc:{1:f3} AvgPrc:{2:f3}\r\n", lProfiler.ToString(), (lProfiler.Time / lTotal), (lProfiler.AverageTime / lAvgTotal)), rLine + lLine);

                    lLine++;
                }
            }
            else
            {
                if (!sProfilers.ContainsKey(rProfiler)) { return; }
                Log.ScreenWrite(sProfilers[rProfiler].ToString(), rLine);
            }
        }
    }
}
