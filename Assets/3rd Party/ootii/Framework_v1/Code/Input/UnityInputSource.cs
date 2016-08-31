using UnityEngine;

namespace com.ootii.Input
{
    /// <summary>
    /// Base class used to abstract how input is retrieved. Inherit from this class 
    /// and then implement the function as needed in order to allow other objects 
    /// to process input using your input component.
    /// </summary>
    [AddComponentMenu("ootii/Input Sources/Unity Input Source")]
    public class UnityInputSource : MonoBehaviour, IInputSource
    {
        /// <summary>
        /// Helps users of the input source to determine if
        /// they are processing user input
        /// </summary>
        public bool _IsEnabled = true;
        public virtual bool IsEnabled
        {
            get { return _IsEnabled; }
            set { _IsEnabled = value; }
        }

        /// <summary>
        /// Determines if the Xbox controller is enable. We default this to off since
        /// the editor needs to enable settings.
        /// </summary>
        public bool _IsXboxControllerEnabled = false;
        public virtual bool IsXboxControllerEnabled
        {
            get { return _IsXboxControllerEnabled; }
        }

        /// <summary>
        /// Set by an external object, it tracks the angle of the
        /// user input compared to the camera's forward direction
        /// Note that this info isn't reliable as objects using it 
        /// before it's set it will get float.NaN.
        /// </summary>
        public virtual float InputFromCameraAngle
        {
            get { return 0f; }
            set { }
        }

        /// <summary>
        /// Set by an external object, it tracks the angle of the
        /// user input compared to the avatars's forward direction
        /// Note that this info isn't reliable as objects using it 
        /// before it's set it will get float.NaN.
        /// </summary>
        public virtual float InputFromAvatarAngle
        {
            get { return 0f; }
            set { }
        }

        /// <summary>
        /// Retrieves horizontal movement from the the input
        /// </summary>
        public virtual float MovementX
        {
            get
            {
                if (!_IsEnabled) { return 0f; }
                return UnityEngine.Input.GetAxis("Horizontal");
            }
        }

        /// <summary>
        /// Retrieves vertical movement from the the input
        /// </summary>
        public virtual float MovementY
        {
            get
            {
                if (!_IsEnabled) { return 0f; }
                return UnityEngine.Input.GetAxis("Vertical");
            }
        }

        /// <summary>
        /// Squared value of MovementX and MovementY (mX * mX + mY * mY)
        /// </summary>
        public virtual float MovementSqr
        {
            get
            {
                if (!_IsEnabled) { return 0f; }

                float lMovementX = MovementX;
                float lMovementY = MovementY;
                return ((lMovementX * lMovementX) + (lMovementY * lMovementY));
            }
        }

        /// <summary>
        /// Retrieves horizontal view movement from the the input
        /// </summary>
        public virtual float ViewX
        {
            get
            {
                if (!_IsEnabled) { return 0f; }

                float lValue = UnityEngine.Input.GetAxis("Mouse X");

                // The mouse value is already frame rate independent (since it's basedon position). However, 
                // we need the stick movement to compensate for the frame rate too. We'll make it's value relative
                // to 60FPS (1/60 = 0.01666)
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
                if (_IsXboxControllerEnabled && lValue == 0f) { lValue = UnityEngine.Input.GetAxis("MXRightStickX") * (Time.deltaTime / 0.01666f); }
#else
                if (_IsXboxControllerEnabled && lValue == 0f) { lValue = UnityEngine.Input.GetAxis("WXRightStickX") * (Time.deltaTime / 0.01666f); }
#endif

                return lValue;
            }
        }

        /// <summary>
        /// Retrieves vertical view movement from the the input
        /// </summary>
        public virtual float ViewY
        {
            get
            {
                if (!_IsEnabled) { return 0f; }

                float lValue = UnityEngine.Input.GetAxis("Mouse Y");

                // The mouse value is already frame rate independent (since it's basedon position). However, 
                // we need the stick movement to compensate for the frame rate too. We'll make it's value relative
                // to 60FPS (1/60 = 0.01666)
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
                if (_IsXboxControllerEnabled && lValue == 0f) { lValue = UnityEngine.Input.GetAxis("MXRightStickY") * (Time.deltaTime / 0.01666f); }
#else
                if (_IsXboxControllerEnabled && lValue == 0f) { lValue = UnityEngine.Input.GetAxis("WXRightStickY") * (Time.deltaTime / 0.01666f); }
#endif

                return lValue;
            }
        }

        /// <summary>
        /// Determines if the player can freely look around
        /// </summary>
        public virtual bool IsViewingActivated
        {
            get
            {
                if (!_IsEnabled) { return false; }

                bool lValue = false;

                if (_IsXboxControllerEnabled)
                {
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
                    lValue = (UnityEngine.Input.GetAxis("MXRightStickX") != 0f);
                    if (!lValue) { lValue = (UnityEngine.Input.GetAxis("MXRightStickY") != 0f); }
#else
                    lValue = (UnityEngine.Input.GetAxis("WXRightStickX") != 0f);
                    if (!lValue) { lValue = (UnityEngine.Input.GetAxis("WXRightStickY") != 0f); }
#endif
                }

                if (!lValue)
                {
                    if (_ViewActivator == 0)
                    {
                        lValue = true;
                    }
                    else if (_ViewActivator == 1)
                    {
                        lValue = UnityEngine.Input.GetMouseButton(0);
                    }
                    else if (_ViewActivator == 2)
                    {
                        lValue = UnityEngine.Input.GetMouseButton(1);
                    }
                    else if (_ViewActivator == 3)
                    {
                        lValue = UnityEngine.Input.GetMouseButton(0);
                        if (!lValue) { lValue = UnityEngine.Input.GetMouseButton(1); }
                    }
                }

                return lValue;
            }
        }

        /// <summary>
        /// Key or button used to allow view to be activated
        /// 0 = none
        /// 1 = left mouse button
        /// 2 = right mouse button
        /// 3 = left and right mouse button
        /// </summary>
        public int _ViewActivator = 2;
        public int ViewActivator
        {
            get { return _ViewActivator; }
            set { _ViewActivator = value; }
        }

        /// <summary>
        /// Test if a specific key is pressed this frame.
        /// </summary>
        /// <param name="rKey"></param>
        /// <returns></returns>
        public virtual bool IsJustPressed(KeyCode rKey)
        {
            if (!_IsEnabled) { return false; }
            return UnityEngine.Input.GetKeyDown(rKey);
        }

        /// <summary>
        /// Test if a specific key is pressed this frame.
        /// </summary>
        /// <param name="rEnumInput">Input Manager enumerated key to test</param>
        /// <returns>Boolean that determines if the action just took place</returns>
        public virtual bool IsJustPressed(int rKey)
        {
            if (!_IsEnabled) { return false; }
            return UnityEngine.Input.GetKeyDown((KeyCode)rKey);
        }

        /// <summary>
        /// Test if a specific action is pressed this frame.
        /// </summary>
        /// <param name="rAction">Action to test for</param>
        /// <returns>Boolean that determines if the action just took place</returns>
        public virtual bool IsJustPressed(string rAction)
        {
            if (!_IsEnabled) { return false; }

            try
            { 
                return UnityEngine.Input.GetButtonDown(rAction);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Test if a specific key is pressed. This is used for continuous checking.
        /// </summary>
        /// <param name="rKey"></param>
        /// <returns></returns>
        public virtual bool IsPressed(KeyCode rKey)
        {
            if (!_IsEnabled) { return false; }
            return UnityEngine.Input.GetKey(rKey);
        }

        /// <summary>
        /// Test if a specific key is pressed. This is used for continuous checking.
        /// </summary>
        /// <param name="rEnumInput">Input Manager enumerated key to test</param>
        /// <returns>Boolean that determines if the action is taking place</returns>
        public virtual bool IsPressed(int rKey)
        {
            if (!_IsEnabled) { return false; }
            return UnityEngine.Input.GetKey((KeyCode)rKey);
        }

        /// <summary>
        /// Test if a specific action is pressed. This is used for continuous checking.
        /// </summary>
        /// <param name="rAction">Action to test for</param>
        /// <returns>Boolean that determines if the action is taking place</returns>
        public virtual bool IsPressed(string rAction)
        {
            if (!_IsEnabled) { return false; }

            try
            { 
                bool lValue = UnityEngine.Input.GetButton(rAction);
                if (!lValue) { lValue = (UnityEngine.Input.GetAxis(rAction) != 0f); }

                return lValue;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Test if a specific key is released this frame.
        /// </summary>
        /// <param name="rKey"></param>
        /// <returns></returns>
        public virtual bool IsJustReleased(KeyCode rKey)
        {
            if (!_IsEnabled) { return false; }
            return UnityEngine.Input.GetKeyUp(rKey);
        }

        /// <summary>
        /// Test if a specific key is released this frame.
        /// </summary>
        /// <param name="rKey">Input Manager enumerated key to test</param>
        /// <returns>Boolean that determines if the action just took place</returns>
        public virtual bool IsJustReleased(int rKey)
        {
            if (!_IsEnabled) { return false; }
            return UnityEngine.Input.GetKeyUp((KeyCode)rKey);
        }

        /// <summary>
        /// Test if a specific action is released this frame.
        /// </summary>
        /// <param name="rAction">Action to test for</param>
        /// <returns>Boolean that determines if the action just took place</returns>
        public virtual bool IsJustReleased(string rAction)
        {
            if (!_IsEnabled) { return false; }

            try
            { 
                return UnityEngine.Input.GetButtonUp(rAction);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Test if a specific key is not pressed. This is used for continuous checking.
        /// </summary>
        /// <param name="rKey"></param>
        /// <returns></returns>
        public virtual bool IsReleased(KeyCode rKey)
        {
            if (!_IsEnabled) { return false; }
            return !UnityEngine.Input.GetKey(rKey);
        }

        /// <summary>
        /// Test if a specific key is not pressed. This is used for continuous checking.
        /// </summary>
        /// <param name="rEnumInput">Input Manager enumerated key to test</param>
        /// <returns>Boolean that determines if the action is taking place</returns>
        public virtual bool IsReleased(int rKey)
        {
            if (!_IsEnabled) { return false; }
            return !UnityEngine.Input.GetKey((KeyCode)rKey);
        }

        /// <summary>
        /// Test if a specific action is not pressed. This is used for continuous checking.
        /// </summary>
        /// <param name="rAction">Action to test for</param>
        /// <returns>Boolean that determines if the action is taking place</returns>
        public virtual bool IsReleased(string rAction)
        {
            if (!_IsEnabled) { return false; }

            try
            {
                bool lValue = UnityEngine.Input.GetButton(rAction);
                if (!lValue) { lValue = (UnityEngine.Input.GetAxis(rAction) != 0f); }

                return !lValue;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Test for a specific action value.
        /// </summary>
        /// <param name="rKey">Input Manager enumerated key to test</param>
        /// <returns>Float value as determined by the key</returns>
        public virtual float GetValue(int rKey)
        {
            return 0f;
        }

        /// <summary>
        /// Test for a specific action value.
        /// </summary>
        /// <param name="rAction">Action to test for</param>
        /// <returns>Float value as determined by the action</returns>
        public virtual float GetValue(string rAction)
        {
            try
            {
                return UnityEngine.Input.GetAxis(rAction);
            }
            catch
            {
                return 0f;
            }
        }
    }
}
