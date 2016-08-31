using System;
using UnityEngine;

namespace com.ootii.Demos
{
    /// <summary>
    /// Simple logic for a platform that moves back and forth
    /// </summary>
    public class PlatformMover : MonoBehaviour
    {
        private Vector3 mStartPosition = new Vector3();
        private Vector3 mEndPosition = new Vector3();
        private Vector3 mVelocity = new Vector3(1, 0, 0);

        public bool UseFixedUpdate = true;

        public bool Rotate = false;

        public float RotationSpeed = 45f;

        public bool Move = false;

        public float MoveSpeed = 0f;

        public Vector3 EndPosition = new Vector3(10, 0, 0);

        /// <summary>
        /// Called right before the first frame update
        /// </summary>
        void Start()
        {
            mStartPosition = transform.position;
            mEndPosition = mStartPosition + EndPosition;

            if (MoveSpeed == 0f)
            {
                mVelocity = (mEndPosition - mStartPosition) / 4f;
            }
            else
            {
                mVelocity = (mEndPosition - mStartPosition).normalized * MoveSpeed;
            }
        }

        /// <summary>
        /// Called once per frame to update physics objects.
        /// </summary>
        void FixedUpdate()
        {
            if (!UseFixedUpdate) { return; }
            InternalUpdate();
        }

        /// <summary>
        /// Called once per frame as the heart-beat
        /// </summary>
        void Update()
        {
            if (UseFixedUpdate) { return; }
            InternalUpdate();
        }

        /// <summary>
        /// Moves and rotates the objects
        /// </summary>
        void InternalUpdate()
        {
            if (Move)
            {
                // Determine the destination and the distance
                float lDistance = Vector3.Distance(transform.position, mEndPosition);
                if (lDistance <= mVelocity.magnitude * Time.deltaTime)
                {
                    Vector3 lTemp = mEndPosition;
                    mEndPosition = mStartPosition;
                    mStartPosition = lTemp;

                    if (MoveSpeed == 0f)
                    {
                        mVelocity = (mEndPosition - mStartPosition) / 4f;
                    }
                    else
                    {
                        mVelocity = (mEndPosition - mStartPosition).normalized * MoveSpeed;
                    }
                }

                // Move the object
                //Rigidbody lRigidBody = gameObject.GetComponent<Rigidbody>();
                //if (lRigidBody != null)
                //{
                //    if (lRigidBody.isKinematic)
                //    {
                //        lRigidBody.MovePosition(transform.position + (mVelocity * Time.deltaTime));
                //    }
                //    else
                //    {
                //        lRigidBody.MovePosition(transform.position + (mVelocity * Time.deltaTime));
                //    }
                //}
                //else
                //{
                    transform.position += mVelocity * Time.deltaTime;
                //}
            }

            // Rotate the object
            if (Rotate)
            {
                //Rigidbody lRigidBody = gameObject.GetComponent<Rigidbody>();
                //if (lRigidBody != null)
                //{
                //    if (lRigidBody.isKinematic)
                //    {
                //        lRigidBody.MoveRotation(transform.rotation * Quaternion.AngleAxis(RotationSpeed * Time.deltaTime, Vector3.up));
                //    }
                //    else
                //    {
                //        lRigidBody.MoveRotation(transform.rotation * Quaternion.AngleAxis(RotationSpeed * Time.deltaTime, Vector3.up));
                //    }
                //}
                //else
                //{
                    transform.Rotate(0f, RotationSpeed * Time.deltaTime, 0f);
                //}
            }
        }
    }
}