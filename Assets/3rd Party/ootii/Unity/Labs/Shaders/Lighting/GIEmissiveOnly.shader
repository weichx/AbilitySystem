Shader "EmissiveOnly"
{
	Properties
	{
		_EmissionColor("EmissionColor", Color) = (1,1,1,1)
	}
	
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Lambert

		struct Input
		{
			float2 uv_MainTex;
		};

		float4 _EmissionColor;

		void surf (Input IN, inout SurfaceOutput o)
		{
			o.Albedo = float3(1,1,1);
			o.Emission = _EmissionColor;
		}
		ENDCG
	} 

	CustomEditor "GIEmissiveOnlyShaderGUI"

}
