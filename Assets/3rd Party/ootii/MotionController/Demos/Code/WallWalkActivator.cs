using UnityEngine;
using com.ootii.Actors;
using com.ootii.Geometry;

namespace com.ootii.Demos
{
    // Simple script used to activate or deactivate wall walking
    public class WallWalkActivator : MonoBehaviour
    {
        /// <summary>
        /// Deterimines if we enable or disable all walking
        /// </summary>
        public bool EnableWalkWalking = false;

        /// <summary>
        /// Collider we'll use for future checks
        /// </summary>
        private BoxCollider mCollider = null;

        /// <summary>
        /// Actors we'll check
        /// </summary>
        private ActorController[] mActors = null;

        // Use this for initialization
        void Start()
        {
            mCollider = gameObject.GetComponent<BoxCollider>();
            mActors = Component.FindObjectsOfType<ActorController>();
        }

        // Update is called once per frame yes
        void Update()
        {
            if (mCollider == null) { return; }
            if (mActors == null) { return; }

            for (int i = 0; i < mActors.Length; i++)
            {
                if (GeometryExt.ContainsPoint(mActors[i]._Transform.position, mCollider))
                {
                    mActors[i].IsGravityRelative = EnableWalkWalking;
                    mActors[i].OrientToGround = EnableWalkWalking;

                    if (!EnableWalkWalking && mActors[i]._Transform.up != Vector3.up)
                    {
                        mActors[i].SetTargetGroundNormal(Vector3.up);
                    }
                    else
                    {
                        mActors[i].SetTargetGroundNormal(Vector3.zero);
                    }
                }
            }
        }
    }
}

