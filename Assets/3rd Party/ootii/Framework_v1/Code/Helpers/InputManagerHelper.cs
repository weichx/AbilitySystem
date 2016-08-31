using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace com.ootii.Helpers
{
    /// <summary>
    /// Enumeration for Unity's Entry Type
    /// </summary>
    public enum InputManagerEntryType
    {
        KEY_MOUSE_BUTTON = 0,
        MOUSE_MOVEMENT = 1,
        JOYSTICK_AXIS = 2
    };

    /// <summary>
    /// Support class for creating and modifying entries
    /// </summary>
    public struct InputManagerEntry
    {
        public string Name;
        public string DescriptiveName;
        public string DescriptiveNegativeName;
        public string NegativeButton;
        public string PositiveButton;
        public string AltNegativeButton;
        public string AltPositiveButton;

        public float Gravity;
        public float Dead;
        public float Sensitivity;

        public bool Snap;
        public bool Invert;

        public InputManagerEntryType Type;
        public int Axis;
        public int JoyNum;
    }

    /// <summary>
    /// Simplifies the adding and removing of entries into Unity's Input Manager.
    /// 
    /// Major props to PL Young
    /// http://www.plyoung.com/blog/manipulating-input-manager-in-script.html
    /// </summary>
    public class InputManagerHelper
    {
#if UNITY_EDITOR

        /// <summary>
        /// Determines if an axis entry already exists
        /// </summary>
        /// <param name="axisName"></param>
        /// <returns></returns>
        public static bool IsDefined(string rName)
        {
            SerializedObject lSerializedObject = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0]);
            SerializedProperty lProperty = lSerializedObject.FindProperty("m_Axes");

            lProperty.Next(true);
            lProperty.Next(true);

            while (lProperty.Next(false))
            {
                SerializedProperty lAxis = lProperty.Copy();
                lAxis.Next(true);
                if (lAxis.stringValue == rName) return true;
            }

            return false;
        }

        /// <summary>
        /// Adds an axis entry if it doesn't already exist
        /// </summary>
        /// <param name="axis"></param>
        public static void AddEntry(InputManagerEntry rAxis)
        {
            AddEntry(rAxis, false);
        }

        /// <summary>
        /// Adds an axis entry if it doesn't already exist
        /// </summary>
        /// <param name="axis"></param>
        public static void AddEntry(InputManagerEntry rAxis, bool rIgnoreDuplicates)
        {
            if (!rIgnoreDuplicates && IsDefined(rAxis.Name)) return;

            SerializedObject lSerializedObject = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0]);
            SerializedProperty lProperty = lSerializedObject.FindProperty("m_Axes");

            lProperty.arraySize++;
            lSerializedObject.ApplyModifiedProperties();

            SerializedProperty axisProperty = lProperty.GetArrayElementAtIndex(lProperty.arraySize - 1);

            GetChildProperty(axisProperty, "m_Name").stringValue = rAxis.Name;
            GetChildProperty(axisProperty, "descriptiveName").stringValue = rAxis.DescriptiveName;
            GetChildProperty(axisProperty, "descriptiveNegativeName").stringValue = rAxis.DescriptiveNegativeName;
            GetChildProperty(axisProperty, "negativeButton").stringValue = rAxis.NegativeButton;
            GetChildProperty(axisProperty, "positiveButton").stringValue = rAxis.PositiveButton;
            GetChildProperty(axisProperty, "altNegativeButton").stringValue = rAxis.AltNegativeButton;
            GetChildProperty(axisProperty, "altPositiveButton").stringValue = rAxis.AltPositiveButton;
            GetChildProperty(axisProperty, "gravity").floatValue = rAxis.Gravity;
            GetChildProperty(axisProperty, "dead").floatValue = rAxis.Dead;
            GetChildProperty(axisProperty, "sensitivity").floatValue = rAxis.Sensitivity;
            GetChildProperty(axisProperty, "snap").boolValue = rAxis.Snap;
            GetChildProperty(axisProperty, "invert").boolValue = rAxis.Invert;
            GetChildProperty(axisProperty, "type").intValue = (int)rAxis.Type;
            GetChildProperty(axisProperty, "axis").intValue = rAxis.Axis - 1;
            GetChildProperty(axisProperty, "joyNum").intValue = rAxis.JoyNum;

            lSerializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Adds an axis entry if it doesn't already exist
        /// </summary>
        /// <param name="axis"></param>
        public static void RemoveEntry(string rName)
        {
            if (!IsDefined(rName)) return;

            SerializedObject lSerializedObject = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0]);
            SerializedProperty lProperty = lSerializedObject.FindProperty("m_Axes");

            int lCount = lProperty.arraySize;
            for (int i = lCount - 1; i >= 0; i--)
            {
                SerializedProperty lElement = lProperty.GetArrayElementAtIndex(i);

                if (GetChildProperty(lElement, "m_Name").stringValue == rName)
                {
                    lProperty.DeleteArrayElementAtIndex(i);
                }
            }

            lSerializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Retrieves a child property from the existing property
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private static SerializedProperty GetChildProperty(SerializedProperty parent, string name)
        {
            SerializedProperty child = parent.Copy();
            child.Next(true);

            do
            {
                if (child.name == name) return child;
            }
            while (child.Next(false));

            return null;
        }

#endif
    }
}
