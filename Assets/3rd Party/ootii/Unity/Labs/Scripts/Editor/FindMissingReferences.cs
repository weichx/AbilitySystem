using UnityEngine;
using System.Collections;
using UnityEditor;

public class FindMissingReferences {

	[MenuItem("GameObject/Find missing references")]
	public static void ScanObjects()
	{
		var allGameObjects = Object.FindObjectsOfType<GameObject>();
		try
		{
			for(int i = 0; i < allGameObjects.Length; ++i)
			{
				EditorUtility.DisplayProgressBar("Scanning objects", allGameObjects[i].name, i / (float)allGameObjects.Length);
				
				foreach(var component in allGameObjects[i].GetComponents<Component>())
				{				
					var so = new SerializedObject(component);
					
					var sp = so.GetIterator();
					while(sp.Next(true))
					{
						if(sp.propertyPath == "m_PrefabParentObject") continue;
						
						if(sp.propertyType == SerializedPropertyType.ObjectReference)
						{
							if(sp.objectReferenceInstanceIDValue == 0) continue;
							if(sp.objectReferenceValue) continue;
							
							var rp = component as ReflectionProbe;
							if(rp && rp.mode != UnityEngine.Rendering.ReflectionProbeMode.Custom && sp.propertyPath == "m_CustomBakedTexture")
								continue;
							
							Debug.LogWarningFormat (component, "Component of type {0} on gameobject {1} has property \"{2}\" with a missing object reference.",
								component.GetType(), component.gameObject, sp.propertyPath);
						}
					}
					
					so.Dispose ();
				}
			}
		}
		finally
		{
			EditorUtility.ClearProgressBar();
		}
	}
	
}
