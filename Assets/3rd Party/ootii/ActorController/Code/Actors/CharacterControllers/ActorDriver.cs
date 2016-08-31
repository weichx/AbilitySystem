using UnityEngine;
using com.ootii.Helpers;
using com.ootii.Input;
using com.ootii.Timing;
using com.ootii.Utilities.Debug;

namespace com.ootii.Actors
{
    /// <summary>
    /// The Actor Controller itself doesn't know how to handle input
    /// in order to create motion. For that, we use something that
    /// understands how to read input for the specific device
    /// we're running. It could be the Motion Controller or it could
    /// be this simplified driver.
    /// </summary>
    [AddComponentMenu("ootii/Actor Drivers/Actor Driver")]
    public class ActorDriver : MonoBehaviour
    {
        /// <summary>
        /// Determines if the driver is enabled
        /// </summary>
        public bool _IsEnabled = true;
        public virtual bool IsEnabled
        {
            get { return _IsEnabled; }
            set { _IsEnabled = value; }
        }

        /// <summary>
        /// GameObject that owns the IInputSource we really want
        /// </summary>
        public GameObject _InputSourceOwner = null;
        public virtual GameObject InputSourceOwner
        {
            get { return _InputSourceOwner; }

            set
            {
                _InputSourceOwner = value;

                // Ensure we update the actual input source
                if (_InputSourceOwner != null)
                {
                    mInputSource = InterfaceHelper.GetComponent<IInputSource>(_InputSourceOwner);
                }
            }
        }

        /// <summary>
        /// Unity units per second the actor moves
        /// </summary>
        public float _MovementSpeed = 5f;
        public virtual float MovementSpeed
        {
            get { return _MovementSpeed; }
            set { _MovementSpeed = value; }
        }

        /// <summary>
        /// Degrees per second the actor rotates
        /// </summary>
        public float _RotationSpeed = 120f;
        public virtual float RotationSpeed
        {
            get { return _RotationSpeed; }

            set
            {
                _RotationSpeed = value;
                mDegreesPer60FPSTick = _RotationSpeed / 60f;
            }
        }

        /// <summary>
        /// Amount of force to apply during a jump
        /// </summary>
        public float _JumpForce = 10f;
        public virtual float JumpForce
        {
            get { return _JumpForce; }
            set { _JumpForce = value; }
        }

        /// <summary>
        /// Provides access to the keyboard, mouse, etc.
        /// </summary>
        protected IInputSource mInputSource = null;

        /// <summary>
        /// Actor Controller being controlled
        /// </summary>
        protected ActorController mActorController = null;

        /// <summary>
        /// Speed we'll actually apply to the rotation. This is essencially the
        /// number of degrees per tick assuming we're running at 60 FPS
        /// </summary>
        protected float mDegreesPer60FPSTick = 1f;

        /// <summary>
        /// Once the objects are instanciated, awake is called before start. Use it
        /// to setup references to other objects
        /// </summary>
        protected virtual void Awake()
        {
            mActorController = gameObject.GetComponent<ActorController>();

            // Object that will provide access to the keyboard, mouse, etc
            if (_InputSourceOwner != null) { mInputSource = InterfaceHelper.GetComponent<IInputSource>(_InputSourceOwner); }

            // If the input source is still null, see if we can grab a local input source
            if (mInputSource == null) { mInputSource = InterfaceHelper.GetComponent<IInputSource>(gameObject); }

            // If that's still null, see if we can grab one from the scene. This may happen
            // if the MC was instanciated from a prefab which doesn't hold a reference to the input source
            if (mInputSource == null)
            {
                IInputSource[] lInputSources = InterfaceHelper.GetComponents<IInputSource>();
                for (int i = 0; i < lInputSources.Length; i++)
                {
                    GameObject lInputSourceOwner = ((MonoBehaviour)lInputSources[i]).gameObject;
                    if (lInputSourceOwner.activeSelf && lInputSources[i].IsEnabled)
                    {
                        mInputSource = lInputSources[i];
                        _InputSourceOwner = lInputSourceOwner;
                    }
                }
            }

            // Default the speed we'll use to rotate
            mDegreesPer60FPSTick = _RotationSpeed / 60f;
        }

        /// <summary>
        /// Called every frame so the driver can process input and
        /// update the actor controller.
        /// </summary>
        protected virtual void Update()
        {
            if (!_IsEnabled) { return; }
            if (mActorController == null) { return; }
            if (mInputSource == null || !mInputSource.IsEnabled) { return; }

            float lDeltaTime = TimeManager.SmoothedDeltaTime;

            // Rotate based on the mouse
            if (mInputSource.IsViewingActivated) {
                float lYaw = mInputSource.ViewX;
                Quaternion lRotation = Quaternion.Euler(0f, lYaw * mDegreesPer60FPSTick, 0f);

                mActorController.Rotate(lRotation);
            }

            //// Move based on WASD
            Vector3 lInput = new Vector3(mInputSource.MovementX, 0f, mInputSource.MovementY);
            Vector3 lMovement = lInput * _MovementSpeed * lDeltaTime;

            if (lMovement.sqrMagnitude > 0f) {
                mActorController.RelativeMove(lMovement);
            }

            // Jump based on space
            if (mInputSource.IsJustPressed("Jump")) {
                if (mActorController.State.IsGrounded) {
                    mActorController.AddImpulse(transform.up * _JumpForce);
                }
            }
        }
    }
}
