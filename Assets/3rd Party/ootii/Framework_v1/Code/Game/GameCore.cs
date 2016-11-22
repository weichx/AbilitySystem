using System;
using UnityEngine;
using com.ootii.Helpers;
using com.ootii.Input;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace com.ootii.Game
{
    /// <summary>
    /// Component responsible for app-level logic such as hiding the cursor, showing the menu, etc.
    /// </summary>
    public class GameCore : MonoBehaviour
    {
        /// <summary>
        /// Provides global access to the core
        /// </summary>
        public static GameCore Core = null;

        /// <summary>
        /// GameObject that owns the IInputSource we really want
        /// </summary>
        public GameObject _InputSourceOwner = null;
        public GameObject InputSourceOwner
        {
            get { return _InputSourceOwner; }
            set { _InputSourceOwner = value; }
        }

        /// <summary>
        /// Defines the source of the input that we'll use to control
        /// the character movement, rotations, and animations.
        /// </summary>
        [NonSerialized]
        public IInputSource _InputSource = null;
        public IInputSource InputSource
        {
            get { return _InputSource; }
            set { _InputSource = value; }
        }

        /// <summary>
        /// Determines if we'll auto find the input source if one doesn't exist
        /// </summary>
        public bool _AutoFindInputSource = true;
        public bool AutoFindInputSource
        {
            get { return _AutoFindInputSource; }
            set { _AutoFindInputSource = value; }
        }

        /// <summary>
        /// Used to control cursor visiblity
        /// </summary>
        public bool _IsCursorVisible = false;
        public bool IsCursorVisible
        {
            get { return _IsCursorVisible; }

            set
            {
                _IsCursorVisible = value;

                Cursor.lockState = (_IsCursorVisible ? CursorLockMode.None : CursorLockMode.Locked);
                Cursor.visible = _IsCursorVisible;
            }
        }

        /// <summary>
        /// Action alias used to determine if the cursor should be visible or not
        /// </summary>
        public string _ShowCursorAlias = "Cursor";
        public string ShowCursorAlias
        {
            get { return _ShowCursorAlias; }
            set { _ShowCursorAlias = value; }
        }

        /// <summary>
        /// Used to pause the game while playing through the editor
        /// </summary>
        public string _EditorPauseAlias = "";
        public string EditorPauseAlias
        {
            get { return _EditorPauseAlias; }
            set { _EditorPauseAlias = value; }
        }

        /// <summary>
        /// Awake is called after all objects are initialized so you can safely speak to 
        /// other objects. This is where reference can be associated.
        /// </summary>
        protected void Awake()
        {
            // Simply stop if one of the GameCores is already initialized
            if (GameCore.Core != null)
            {
                Destroy(gameObject);
                return;
            }

            // The GameCore will be active for all scenes as it sits above them
            DontDestroyOnLoad(gameObject);

            // Only store the first one
            if (GameCore.Core == null)
            {
                GameCore.Core = this;
            }

            // Object that will provide access to the keyboard, mouse, etc
            if (_InputSourceOwner != null) { _InputSource = InterfaceHelper.GetComponent<IInputSource>(_InputSourceOwner); }

            // If the input source is still null, see if we can grab a local input source
            if (_InputSource == null) { _InputSource = InterfaceHelper.GetComponent<IInputSource>(gameObject); }

            // If that's still null, see if we can grab one from the scene. This may happen
            // if the MC was instanciated from a prefab which doesn't hold a reference to the input source
            if (_AutoFindInputSource && _InputSource == null)
            {
                IInputSource[] lInputSources = InterfaceHelper.GetComponents<IInputSource>();
                for (int i = 0; i < lInputSources.Length; i++)
                {
                    GameObject lInputSourceOwner = ((MonoBehaviour)lInputSources[i]).gameObject;
                    if (lInputSourceOwner.activeSelf && lInputSources[i].IsEnabled)
                    {
                        _InputSource = lInputSources[i];
                        _InputSourceOwner = lInputSourceOwner;
                    }
                }
            }

            // Initialize any states
            IsCursorVisible = _IsCursorVisible;
        }

        /// <summary>
        /// Called every frame.
        /// </summary>
        protected void Update()
        {
            // Process input to do game level input
            if (_InputSource != null)
            {
#if UNITY_EDITOR
                if (_EditorPauseAlias.Length > 0)
                {
                    if (_InputSource.IsJustPressed(_EditorPauseAlias))
                    {
                        EditorApplication.isPaused = true;
                    }
                }
#endif

                if (_ShowCursorAlias.Length > 0)
                {
                    if (_InputSource.IsJustPressed(_ShowCursorAlias))
                    {
                        IsCursorVisible = !IsCursorVisible;
                    }
                }
            }
        }       
    }
}
