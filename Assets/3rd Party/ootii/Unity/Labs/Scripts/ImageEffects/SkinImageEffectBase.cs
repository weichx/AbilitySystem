using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class SkinImageEffectBase : MonoBehaviour {
    
    // Shader property set via the Inspector
    // Material property created by the script
    public Shader m_shader;
    protected Material m_material;

	void Start () {
        if (!SystemInfo.supportsImageEffects)
        {
            enabled = false;
            return;
        }
        GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;
    
	}
	
    // Helper functions
    protected static void DestroyMaterial(Material mat)
    {
        if (mat)
        {
            DestroyImmediate(mat);
            mat = null;
        }
    }

    protected virtual void OnDisable()
    {
        DestroyMaterial(m_material);
    }

    protected static Material CreateMaterial(Shader shader)
    {
        if (!shader)
            return null;
        Material m = new Material(shader);
        m.hideFlags = HideFlags.HideAndDontSave;
        return m;
    }
}
