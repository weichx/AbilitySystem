using System;
using System.Linq;
using System.Reflection;

namespace com.ootii.Helpers
{
    public class ReflectionHelper
    {
        /// <summary>
        /// Grabs an attribute from the class type and returns it
        /// </summary>
        /// <param name="rObjectType">Object type who has the attribute value</param>
        public static T GetAttribute<T>(Type rObjectType)
        {
#if !UNITY_EDITOR && (NETFX_CORE || WINDOWS_UWP || UNITY_WP8 || UNITY_WP_8_1 || UNITY_WSA || UNITY_WSA_8_0 || UNITY_WSA_8_1 || UNITY_WSA_10_0)
            System.Collections.Generic.IEnumerable<System.Attribute> lInitialAttributes = rObjectType.GetTypeInfo().GetCustomAttributes(typeof(T), true);
            object[] lAttributes = lInitialAttributes.ToArray();
#else
            object[] lAttributes = rObjectType.GetCustomAttributes(typeof(T), true);
#endif

            if (lAttributes == null || lAttributes.Length == 0) { return default(T); }

            return (T)lAttributes[0];           
        }

        /// <summary>
        /// Sets the property value if the property exists
        /// </summary>
        public static void SetProperty(object rObject, string rName, object rValue)
        {
            Type lType = rObject.GetType();
            PropertyInfo[] lProperties = lType.GetProperties();
            if (lProperties != null && lProperties.Length > 0)
            {
                for (int i = 0; i < lProperties.Length; i++)
                {
                    if (lProperties[i].Name == rName && lProperties[i].CanWrite)
                    {
                        lProperties[i].SetValue(rObject, rValue, null);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Determines if the specified type exists
        /// </summary>
        /// <param name="rType"></param>
        /// <returns></returns>
        public static bool IsTypeValid(string rType)
        {
            try
            {
                Type lType = Type.GetType(rType);
                return (lType != null);
            }
            catch
            {
                return false;
            }
        }
    }
}
