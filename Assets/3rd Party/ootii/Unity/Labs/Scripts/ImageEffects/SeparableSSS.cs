using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class SeparableSSS : SkinImageEffectBase {

    // Shader specific properties set via the Inspector
    public float sssWidth;
    public float ssssStrengthSource;

    // One time property assignments
    private void CreateMaterials()
    {
        if (m_shader != null && !m_material && m_shader.isSupported)
        {
            m_material = CreateMaterial(m_shader);
            m_material.SetVector("kernel0", new Vector4(0.530605f, 0.613514f, 0.739601f, 0.0f));
            m_material.SetVector("kernel1", new Vector4(0.000973794f, 1.11862e-005f, 9.43437e-007f, -3.0f));
            m_material.SetVector("kernel2", new Vector4(0.00333804f, 7.85443e-005f, 1.2945e-005f, -2.52083f));
            m_material.SetVector("kernel3", new Vector4(0.00500364f, 0.00020094f, 5.28848e-005f, -2.08333f));
            m_material.SetVector("kernel4", new Vector4(0.00700976f, 0.00049366f, 0.000151938f, -1.6875f));
            m_material.SetVector("kernel5", new Vector4(0.0094389f, 0.00139119f, 0.000416598f, -1.33333f));
            m_material.SetVector("kernel6", new Vector4(0.0128496f, 0.00356329f, 0.00132016f, -1.02083f));
            m_material.SetVector("kernel7", new Vector4(0.017924f, 0.00711691f, 0.00347194f, -0.75f));
            m_material.SetVector("kernel8", new Vector4(0.0263642f, 0.0119715f, 0.00684598f, -0.520833f));
            m_material.SetVector("kernel9", new Vector4(0.0410172f, 0.0199899f, 0.0118481f, -0.333333f));
            m_material.SetVector("kernel10", new Vector4(0.0493588f, 0.0367726f, 0.0219485f, -0.1875f));
            m_material.SetVector("kernel11", new Vector4(0.0402784f, 0.0657244f, 0.04631f, -0.0833333f));
            m_material.SetVector("kernel12", new Vector4(0.0211412f, 0.0459286f, 0.0378196f, -0.0208333f));
            m_material.SetVector("kernel13", new Vector4(0.0211412f, 0.0459286f, 0.0378196f, 0.0208333f));
            m_material.SetVector("kernel14", new Vector4(0.0402784f, 0.0657244f, 0.04631f, 0.0833333f));
            m_material.SetVector("kernel15", new Vector4(0.0493588f, 0.0367726f, 0.0219485f, 0.1875f));
            m_material.SetVector("kernel16", new Vector4(0.0410172f, 0.0199899f, 0.0118481f, 0.333333f));
            m_material.SetVector("kernel17", new Vector4(0.0263642f, 0.0119715f, 0.00684598f, 0.520833f));
            m_material.SetVector("kernel18", new Vector4(0.017924f, 0.00711691f, 0.00347194f, 0.75f));
            m_material.SetVector("kernel19", new Vector4(0.0128496f, 0.00356329f, 0.00132016f, 1.02083f));
            m_material.SetVector("kernel20", new Vector4(0.0094389f, 0.00139119f, 0.000416598f, 1.33333f));
            m_material.SetVector("kernel21", new Vector4(0.00700976f, 0.00049366f, 0.000151938f, 1.6875f));
            m_material.SetVector("kernel22", new Vector4(0.00500364f, 0.00020094f, 5.28848e-005f, 2.08333f));
            m_material.SetVector("kernel23", new Vector4(0.00333804f, 7.85443e-005f, 1.2945e-005f, 2.52083f));
            m_material.SetVector("kernel24", new Vector4(0.000973794f, 1.11862e-005f, 9.43437e-007f, 3f));
        }
    }    

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        CreateMaterials();

      //  m_material.SetFloat("SSSS_FOVY", GetComponent<Camera>().fieldOfView);
        m_material.SetFloat("ssssStrengthSource", ssssStrengthSource);
        m_material.SetFloat("sssWidth", sssWidth);

        // Update properties per frame
        RenderTexture temp = null;

        temp = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);

        m_material.SetVector("dir", new Vector4(1.0f, 0.0f, 0.0f, 0.0f));
        Graphics.Blit(source, temp, m_material);

        m_material.SetVector("dir", new Vector4(0.0f, 1.0f, 0.0f, 0.0f));
        Graphics.Blit(temp, destination, m_material);

        Graphics.ClearRandomWriteTargets();
        RenderTexture.ReleaseTemporary(temp);
    }
}
