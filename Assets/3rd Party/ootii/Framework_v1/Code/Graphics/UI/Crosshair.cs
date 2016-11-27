using UnityEngine;

namespace com.ootii.Graphics.UI
{
    /// <summary>
    /// Provides a method for managing the cross hairs we display
    /// </summary>
    public class Crosshair : MonoBehaviour
    {
        /// <summary>
        /// Provides global access to the crosshair
        /// </summary>
        private static Crosshair mInstance = null;
        public static Crosshair Instance
        {
            get { return mInstance; }
        }

        /// <summary>
        /// Determines if the crosshair is enabled
        /// </summary>
        public bool _IsEnabled = false;
        public bool IsEnabled
        {
            get { return _IsEnabled; }
            set { _IsEnabled = value; }
        }

        /// <summary>
        /// Determine if the cursor is enabled and visible
        /// </summary>
        public bool _IsCursorEnabled = true;
        public bool IsCursorEnabled
        {
            get { return _IsCursorEnabled; }

            set
            {
                _IsCursorEnabled = value;
                Cursor.lockState = (_IsCursorEnabled ? CursorLockMode.None : CursorLockMode.Locked);
                Cursor.visible = _IsCursorEnabled;
            }
        }

        /// <summary>
        /// Texture that will be used as the crosshair
        /// </summary>
        public Texture2D _Texture;
        public Texture2D Texture
        {
            get { return _Texture; }

            set
            {
                _Texture = value;

                if (_Texture != null)
                {
                    if (_Width == 0f) { _Width = _Texture.width; }
                    if (_Height == 0f) { _Height = _Texture.height; }
                    mPosition = new Rect((Screen.width - _Width) / 2, (Screen.height - _Height) / 2, _Width, _Height);
                }
            }
        }

        /// <summary>
        /// Width of the crosshair
        /// </summary>
        public float _Width = 32f;
        public float Width
        {
            get { return _Width; }

            set
            {
                _Width = value;
                Texture = _Texture;
            }
        }

        /// <summary>
        /// Height of the crosshair
        /// </summary>
        public float _Height = 32f;
        public float Height
        {
            get { return _Height; }

            set
            {
                _Height = value;
                Texture = _Texture;
            }
        }

        /// <summary>
        /// Rectangel representing the position and size of the texture
        /// </summary>
        private Rect mPosition;

        /// <summary>
        /// Runs before any Update is called
        /// </summary>
        void Start()
        {
            // Create our global instance
            if (Crosshair.Instance == null) { Crosshair.mInstance = this; }

            // Ensure the position is set
            if (_Texture != null) { Texture = _Texture; }

            // Determine what we do with the cursor
            IsCursorEnabled = _IsCursorEnabled;
        }

        /// <summary>
        /// OnGUI is called for rendering and handling GUI events.
        /// </summary>
        private void OnGUI()
        {
            //Cursor.lockState = CursorLockMode.Locked;
            //Cursor.visible = false;

            if (_IsEnabled && _Texture != null)
            {
                UnityEngine.GUI.DrawTexture(mPosition, _Texture);
            }
        }
    }
}
