using UnityEngine;

namespace com.ootii.Graphics.UI
{
    /// <summary>
    /// Provides a method for managing the cross hairs we display
    /// </summary>
    public class CrosshairFillCircle : MonoBehaviour
    {
        /// <summary>
        /// Provides global access to the crosshair
        /// </summary>
        private static CrosshairFillCircle mInstance = null;
        public static CrosshairFillCircle Instance
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

        ///// <summary>
        ///// Determine if the cursor is enabled and visible
        ///// </summary>
        //public bool _IsCursorEnabled = true;
        //public bool IsCursorEnabled
        //{
        //    get { return _IsCursorEnabled; }

        //    set
        //    {
        //        _IsCursorEnabled = value;
        //        Cursor.lockState = (_IsCursorEnabled ? CursorLockMode.None : CursorLockMode.Locked);
        //        Cursor.visible = _IsCursorEnabled;
        //    }
        //}

        /// <summary>
        /// Determines how much to fill the circle
        /// </summary>
        protected float mFillPercent = 0f;
        public float FillPercent
        {
            get { return mFillPercent; }

            set
            {
                if (value != mFillPercent)
                {
                    CreateTexture(value);
                    mFillPercent = value;
                }
            }
        }

        /// <summary>
        /// Texture that will be used as the crosshair
        /// </summary>
        public Texture2D _BGTexture;
        public Texture2D BGTexture
        {
            get { return _BGTexture; }
            set { _BGTexture = value; }
        }

        /// <summary>
        /// Texture that will be used as the crosshair
        /// </summary>
        public Texture2D _FillTexture;
        public Texture2D FillTexture
        {
            get { return _FillTexture; }
            set { _FillTexture = value; }
        }

        /// <summary>
        /// Width of the crosshair
        /// </summary>
        public float _Width = 32f;
        public float Width
        {
            get { return _Width; }
            set { _Width = value; }
        }

        /// <summary>
        /// Height of the crosshair
        /// </summary>
        public float _Height = 32f;
        public float Height
        {
            get { return _Height; }
            set { _Height = value; }
        }

        /// <summary>
        /// Rectangel representing the position and size of the texture
        /// </summary>
        private Rect mScreenRect;

        /// <summary>
        /// Support blitting
        /// </summary>
        private Material mClearMaterial = null;
        private Material mBlitMaterial = null;
        private RenderTexture mRenderTexture = null;

        /// <summary>
        /// Runs before any Update is called
        /// </summary>
        void Start()
        {
            // Create our global instance
            if (CrosshairFillCircle.Instance == null) { CrosshairFillCircle.mInstance = this; }

            // Ensure the position is set
            if (_FillTexture != null) { FillTexture = _FillTexture; }
            if (_BGTexture != null) { BGTexture = _BGTexture; }

            // Initialize the material
            CreateTexture(0f);

            // Determine what we do with the cursor
            //IsCursorEnabled = _IsCursorEnabled;
        }

        /// <summary>
        /// OnGUI is called for rendering and handling GUI events.
        /// </summary>
        private void OnGUI()
        {
            if (!_IsEnabled) { return; }
            if (mRenderTexture == null) { return; }

            mScreenRect.x = (Screen.width - _Width) / 2f;
            mScreenRect.y = (Screen.height - _Height) / 2f;
            mScreenRect.width = _Width;
            mScreenRect.height = _Height;
            UnityEngine.GUI.DrawTexture(mScreenRect, mRenderTexture);
        }

        /// <summary>
        /// Generates the texture we'll display as the crosshair. Note that we CANNOT
        /// call this function in OnGUI.
        /// </summary>
        private void CreateTexture(float rPercent)
        {
            if (_BGTexture == null) { return; }

            if (mClearMaterial == null)
            {
                mClearMaterial = new Material(Shader.Find("Hidden/ClearBlit"));
            }

            if (mBlitMaterial == null)
            {
                mBlitMaterial = new Material(Shader.Find("Hidden/RadialBlit"));
            }

            if (mRenderTexture == null)
            {
                mRenderTexture = new RenderTexture(256, 256, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                mRenderTexture.wrapMode = TextureWrapMode.Clamp;
            }

            // Clear the image
            UnityEngine.Graphics.Blit(_BGTexture, mRenderTexture, mClearMaterial, 0);

            // Fill with the background or fill based on the angle
            mBlitMaterial.SetFloat("_Angle", Mathf.Lerp(-3.1416f, 3.1416f, rPercent));
            mBlitMaterial.SetTexture("_FillTex", _FillTexture);
            UnityEngine.Graphics.Blit(_BGTexture, mRenderTexture, mBlitMaterial, 0);
        }
    }
}
