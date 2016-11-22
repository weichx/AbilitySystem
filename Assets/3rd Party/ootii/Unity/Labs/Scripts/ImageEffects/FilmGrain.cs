using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class FilmGrain : SkinImageEffectBase {
    
    // Shader specific properties set via the Inspector
    public Texture2D m_NoiseTex;
    public float noiseIntensity;
    public float exposure;

    // One time property assignments
    private void CreateMaterials()
    {
        if (m_shader != null && !m_material && m_shader.isSupported)
        {
            m_material = CreateMaterial(m_shader);
            m_material.SetTexture("_NoiseTex", m_NoiseTex);                                  
        }
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        CreateMaterials();
        
        // Update properties per frame
        m_material.SetFloat("noiseIntensity", noiseIntensity);
        m_material.SetFloat("exposure", 5.0f * (exposure / (8.0f - 0.0f)));
        m_material.SetVector("pixelSize", new Vector2(Screen.width, Screen.height));

        Graphics.Blit(source, destination, m_material);        
    }
}
