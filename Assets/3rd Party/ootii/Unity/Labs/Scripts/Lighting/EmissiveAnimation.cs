using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class EmissiveAnimation : MonoBehaviour
{
	[SerializeField]
	Color m_EmissiveColor = Color.white;
	[SerializeField]
	[Range (0.0F, 8.0F)]
	float m_EmissiveIntensity = 1.0F;

	[SerializeField]
	[Range (0.0F, 8.0F)]
	float m_RendererEmissiveScale = 1.0F;

	[SerializeField]
	[Range (0.0F, 16.0F)]
	float m_LightIntensity = 1.0F;

	[SerializeField]
	Renderer[] m_GIEmitters = {};

	[SerializeField]
	Renderer[] m_Renderers = {};

	[SerializeField]
	Light[] m_Lights = {};

	MaterialPropertyBlock m_MaterialProperties;
	static int m_EmissivePropertyName;

	public Color emissiveColor
	{
		get { return m_EmissiveColor; }
		set { m_EmissiveColor = value; Apply(); }
	}

	public float emissiveIntensity
	{
		get { return m_EmissiveIntensity; }
		set { m_EmissiveIntensity = value;  Apply(); }
	}

	void Awake()
	{
		m_MaterialProperties = new MaterialPropertyBlock();
		m_EmissivePropertyName = Shader.PropertyToID("_EmissionColor");

		Apply ();
	}

	public void OnDidApplyAnimationProperties ()
	{
		Apply ();
	}

	void Apply()
	{
		Color color = m_EmissiveColor * m_EmissiveIntensity;

		// Apply to GI emitter
		foreach (Renderer renderer in m_GIEmitters)
		{
			if (renderer == null)
				continue;

			DynamicGI.SetEmissive (renderer, color);
		}

		// Apply material properties
		foreach (Renderer renderer in m_Renderers)
		{
			if (renderer == null)
				continue;

			m_MaterialProperties.SetColor (m_EmissivePropertyName, color * m_RendererEmissiveScale);
			renderer.SetPropertyBlock (m_MaterialProperties);
		}

		// Apply to lights
		foreach (Light light in m_Lights)
		{
            if(light == null)
                continue;
		    
			light.intensity = m_LightIntensity;
		}
	}

	//@TODO: Validate that materials have material flags set to none...
	//@TODO: Validate that emitter has lightmapmaterial setup

	void OnValidate ()
	{
		if (gameObject.activeInHierarchy)
		{
			Awake ();
			Apply ();
		}
	}
}
