using System;
using com.ootii.Collections;

namespace com.ootii.Utilities.Debug
{
	/// <summary>
	/// Helps us to render text to the screen
	/// </summary>
	public class LogText
	{
		public string Text;

		public int X;

		public int Y;

		// ******************************** OBJECT POOL ********************************
		
		/// <summary>
		/// Allows us to reuse objects without having to reallocate them over and over
		/// </summary>
		private static ObjectPool<LogText> sPool = new ObjectPool<LogText>(20, 5);

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
		public static LogText Allocate()
		{
			// Grab the next available object
			LogText lInstance = sPool.Allocate();
			
			// Set values
			lInstance.Text = "";
			lInstance.X = 0;
			lInstance.Y = 0;

			return lInstance;
		}
		
		/// <summary>
		/// Pulls an object from the pool.
		/// </summary>
		/// <returns></returns>
		public static LogText Allocate(string rText, int rX, int rY)
		{
			// Grab the next available object
			LogText lInstance = sPool.Allocate();

			// Set values
			lInstance.Text = rText;
			lInstance.X = rX;
			lInstance.Y = rY;

			return lInstance;
		}
		
		/// <summary>
		/// Returns an element back to the pool.
		/// </summary>
		/// <param name="rEdge"></param>
		public static void Release(LogText rInstance)
		{
			if (rInstance == null) { return; }

			// Make it available to others.
			sPool.Release(rInstance);
		}
	}
}

