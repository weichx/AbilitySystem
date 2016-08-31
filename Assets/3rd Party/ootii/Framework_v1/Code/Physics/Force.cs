using System;
using UnityEngine;
using com.ootii.Collections;

namespace com.ootii.Physics
{
	/// <summary>
	/// Encapsulates the information of a force
	/// that is applied to the body.
	/// </summary>
	public class Force
	{
		/// <summary>
		/// Defines the type of force we're dealing with
		/// </summary>
		public ForceMode Type;

		/// <summary>
		/// The total value (magnitude and direction) of the
		/// force
		/// </summary>
		public Vector3 Value;

		/// <summary>
		/// Determines when (in game seconds) the force
		/// will start to be applied. 0 represents the
		/// force should start immediately.
		/// </summary>
		public float StartTime;

		/// <summary>
		/// Determines how long (in seconds) the force
		/// will be applied once it has been started. 0 represents
		/// no duration expiration.
		/// </summary>
		public float Duration;

		/// <summary>
		/// Returns the magnitude of the force
		/// </summary>
		/// <value>The magnitude.</value>
		public float Magnitude
		{
			get { return Value.magnitude; }
		}

		/// <summary>
		/// Returns the normalized version of the force without
		/// actually changing the value of the force
		/// </summary>
		/// <value>The direction.</value>
		public Vector3 Direction
		{
			get { return Value.normalized; }
		}

		/// <summary>
		/// Default constructor
		/// </summary>
		public Force()
		{
		}
		
		// ******************************** OBJECT POOL ********************************
		
		/// <summary>
		/// Allows us to reuse objects without having to reallocate them over and over
		/// </summary>
		private static ObjectPool<Force> sPool = new ObjectPool<Force>(20, 5);
		
		/// <summary>
		/// Returns the number of items allocated
		/// </summary>
		/// <value>The allocated.</value>
		public static int Length
		{
			get { return sPool.Length; }
		}
		
		/// <summary>
		/// Pulls an object from the pool.
		/// </summary>
		/// <returns></returns>
		public static Force Allocate()
		{
			// Grab the next available object
			Force lInstance = sPool.Allocate();
			
			// Set values
			lInstance.Type = ForceMode.Force;
			lInstance.Value = Vector3.zero;
			lInstance.StartTime = 0f;
			lInstance.Duration = 0f;

			return lInstance;
		}
		
		/// <summary>
		/// Pulls an object from the pool.
		/// </summary>
		/// <returns></returns>
		/// <param name="rValue">Vector3 defining the magnitude and direction of the force</param>
		public static Force Allocate(Vector3 rValue)
		{
			// Grab the next available object
			Force lInstance = sPool.Allocate();
			
			// Set values
			lInstance.Type = ForceMode.Force;
            lInstance.Value = rValue;
			lInstance.StartTime = 0f;
			lInstance.Duration = 0f;

			return lInstance;
		}
		
		/// <summary>
		/// Returns an element back to the pool.
		/// </summary>
		/// <param name="rEdge"></param>
		public static void Release(Force rInstance)
		{
			if (rInstance == null) { return; }
			
			// Make it available to others.
			sPool.Release(rInstance);
		}
	}
}

