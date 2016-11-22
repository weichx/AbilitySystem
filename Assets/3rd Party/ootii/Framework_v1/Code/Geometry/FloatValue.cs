using System;
using UnityEngine;

namespace com.ootii.Geometry
{
    /// <summary>
    /// Simple class to help find the running average of values
    /// and determine trends. Meant to be fast by using fixed arrays.
    /// </summary>
    public struct FloatValue
    {
        public const int TREND_CONSTANT = 0;
        public const int TREND_DECREASING = 1;
        public const int TREND_INCREASING = 2;

        /// <summary>
        /// Determines the number of samples to keep track of over time
        /// </summary>
        private int mSampleCount;
        public int SampleCount
        {
            get { return mSampleCount; }
            
            set 
            {
                mSampleCount = (value > 0 ? value : 1);
                if (mSamples == null || mSamples.Length != mSampleCount)
                {
                    Resize(mSampleCount, mDefault);
                }
            }
        }

        /// <summary>
        /// Value of the last added sample
        /// </summary>
        private float mValue;
        public float Value
        {
            get { return mValue; }
            set { Add(value); }
        }

        /// <summary>
        /// Returns the previous value that was added (one before the last)
        /// </summary>
        private float mPrevValue;
        public float PrevValue
        {
            get { return mPrevValue; }
        }

        /// <summary>
        /// Current sum of the sample set
        /// </summary>
        private float mSum;
        public float Sum
        {
            get { return mSum; }
        }

        /// <summary>
        /// Current average of the sample set
        /// </summary>
        private float mAverage;
        public float Average
        {
            get { return mAverage; }
        }

        /// <summary>
        /// Return the value once the trend completes
        /// </summary>
        private float mTrendValue;
        public float TrendValue
        {
            get { return mTrendValue; }
        }

        /// <summary>
        /// Tracks the trending direction of the values
        /// </summary>
        private int mTrendDirection;
        public int TrendDirection
        {
            get { return mTrendDirection; }
        }

        /// <summary>
        /// Samples we're storing
        /// </summary>
        private float[] mSamples;

        /// <summary>
        /// Default value for the samples
        /// </summary>
        private float mDefault;

        /// <summary>
        /// Index to place the next sample value
        /// </summary>
        private int mIndex;

        /// <summary>
        /// Default constructor
        /// </summary>
        public FloatValue(float rValue)
        {
            mSampleCount = 10;
            mValue = 0f;
            mPrevValue = 0f;
            mSum = 0f;
            mAverage = 0f;
            mDefault = 0f;
            mTrendDirection = FloatValue.TREND_CONSTANT;
            mTrendValue = 0f;
            mIndex = -1;
            mSamples = null;

            Resize(mSampleCount, mDefault);
            Add(rValue);
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public FloatValue(float rValue, int rSampleCount)
        {
            mSampleCount = rSampleCount;
            mValue = 0f;
            mPrevValue = 0f;
            mSum = 0f;
            mAverage = 0f;
            mDefault = 0f;
            mTrendDirection = FloatValue.TREND_CONSTANT;
            mTrendValue = 0f;
            mIndex = -1;
            mSamples = null;

            Resize(mSampleCount, mDefault);
            Add(rValue);
        }

        ///// <summary>
        ///// Size constructor constructor
        ///// </summary>
        //public FloatValue(int rSampleCount, float rDefault)
        //{
        //    mDefault = rDefault;
        //    mSampleCount = rSampleCount;
        //    Resize(mSampleCount, mDefault);
        //}

        public void Clear(float rValue = 0f)
        {
            for (int i = 0; i < mSampleCount; i++)
            {
                mSamples[i] = rValue;
            }

            mValue = rValue;
            mPrevValue = rValue;
            mAverage = rValue;
            mTrendDirection = FloatValue.TREND_CONSTANT;
            mTrendValue = rValue;

            mIndex = -1;
        }

        /// <summary>
        /// Replaces the value that was last added with this new value
        /// </summary>
        /// <param name="rValue"></param>
        /// <returns></returns>
        public float Replace(float rValue)
        {
            mIndex--;
            if (mIndex < 0) { mIndex = mSampleCount - 1; }

            return Add(rValue);
        }

        /// <summary>
        /// Adds a value to the sample set and returns the
        /// average of the sample
        /// </summary>
        /// <param name="rValue"></param>
        /// <returns></returns>
        public float Add(float rValue)
        {
            // Ensure our array is valid
            if (mSampleCount == 0) { Resize(10, mDefault); }

            // Set the value
            mPrevValue = mValue;
            mValue = rValue;

            // Determine the trend
            if (mValue == mPrevValue)
            {
                if (mTrendDirection != FloatValue.TREND_CONSTANT) { mTrendValue = mValue; }
                mTrendDirection = FloatValue.TREND_CONSTANT;
            }
            else if (mValue < mPrevValue)
            {
                if (mTrendDirection != FloatValue.TREND_DECREASING) { mTrendValue = mValue; }
                mTrendDirection = FloatValue.TREND_DECREASING;
            }
            else if (mTrendDirection > mPrevValue)
            {
                if (mTrendDirection != FloatValue.TREND_INCREASING) { mTrendValue = mValue; }
                mTrendDirection = FloatValue.TREND_INCREASING;
            }

            // Add the value
            mIndex++;
            if (mIndex >= mSampleCount) { mIndex = 0; }

            mSamples[mIndex] = mValue;

            // Find the average
            mSum = 0f;
            for (int i = 0; i < mSampleCount; i++)
            {
                mSum += mSamples[i];
            }

            mAverage = mSum / mSampleCount;            
            return mAverage;
        }

        /// <summary>
        /// Resize the array
        /// </summary>
        /// <param name="rSize">New size of the array</param>
        private void Resize(int rSize, float rDefault)
        {
            int lCount = 0;

            // Build the new array and copy the contents
            float[] lNewSamples = new float[rSize];

            if (mSamples != null)
            {
                lCount = mSamples.Length;
                Array.Copy(mSamples, lNewSamples, Math.Min(lCount, rSize));

                // Allocate items in the new array
                for (int i = lCount; i < rSize; i++)
                {
                    lNewSamples[i] = rDefault;
                }
            }

            // Replace the old array
            mSamples = lNewSamples;
            mSampleCount = mSamples.Length;

            // Start filling from the beginning
            mIndex = -1;
        }

        /// <summary>
        /// The implicit operator lets us declare a specific conversion 
        /// from the parameter to our type. We'll use this to initialize
        /// the struct
        /// </summary>
        /// <param name="rValue">Value to initialize to</param>
        /// <returns>New object holding the parameter value</returns>
        public static implicit operator FloatValue(float rValue)
        {
            return new FloatValue(rValue);
        }
    }
}
