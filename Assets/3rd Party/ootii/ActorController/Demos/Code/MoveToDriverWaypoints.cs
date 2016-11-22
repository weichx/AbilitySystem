using System.Collections.Generic;
using UnityEngine;
using com.ootii.Actors;

namespace com.ootii.Demos
{
    public class MoveToDriverWaypoints : MonoBehaviour
    {
        /// <summary>
        /// Determines if we randomize the next waypoint
        /// </summary>
        public bool Randomize = false;

        /// <summary>
        /// Distance at which we move to the next waypoint
        /// </summary>
        public float ArrivalDistance = 0.5f;

        /// <summary>
        /// Determines if we ignore the height for arrival
        /// </summary>
        public bool IgnoreY = false;

        /// <summary>
        /// Waypoints that we're moving to
        /// </summary>
        public List<Transform> Waypoints = new List<Transform>();

        /// <summary>
        /// Driver that we're using to move the character
        /// </summary>
        protected MoveToDriver mDriver = null;

        /// <summary>
        /// Index of the current waypoint we're moving to
        /// </summary>
        protected int mCurrentIndex = -1;

        /// <summary>
        /// Called to initialize the object
        /// </summary>
        public void Awake()
        {
            mDriver = gameObject.GetComponent<MoveToDriver>();
            if (mDriver != null && Waypoints.Count > 0)
            {
                if (Randomize)
                {
                    mCurrentIndex = UnityEngine.Random.Range(0, Waypoints.Count - 1);
                }
                else
                {
                    mCurrentIndex = 0;
                }

                mDriver.Target = Waypoints[mCurrentIndex];
            }
        }

        /// <summary>
        /// Called once per frame
        /// </summary>
        public void Update()
        {
            if (mDriver == null) { return; }
            if (Waypoints.Count == 0) { return; }

            Vector3 lPosition = gameObject.transform.position;
            Vector3 lWaypoint = Waypoints[mCurrentIndex].position;

            if (IgnoreY)
            {
                lPosition.y = 0f;
                lWaypoint.y = 0f;
            }

            float lDistance = Vector3.Distance(lPosition, lWaypoint);
            if (lDistance < ArrivalDistance)
            {
                if (Randomize)
                {
                    mCurrentIndex = UnityEngine.Random.Range(0, Waypoints.Count - 1);
                }
                else
                {
                    mCurrentIndex++;
                }

                if (mCurrentIndex >= Waypoints.Count) { mCurrentIndex = 0; }

                mDriver.Target = Waypoints[mCurrentIndex];
            }
        }
    }
}
