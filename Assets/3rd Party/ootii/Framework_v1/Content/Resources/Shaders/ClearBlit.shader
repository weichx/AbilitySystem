// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/ClearBlit" 
{
	Properties 
	{
	}

		CGINCLUDE

#include "UnityCG.cginc"
#pragma glsl

	struct VertexStruct 
	{
		float4 pos : POSITION;
		float2 uv : TEXCOORD0;
	};
	
	//Common Vertex Shader
	VertexStruct vert( appdata_img rInput )
	{
		VertexStruct lOutput;
		lOutput.pos = UnityObjectToClipPos (rInput.vertex);
		lOutput.uv = rInput.texcoord.xy;
		return lOutput;
	} 
	
	half4 frag(VertexStruct rInput) : COLOR
	{		
		return float4(0,0,0,0); 
	}

	ENDCG	
	 
	Subshader 
	{
		Tags { "Queue" = "Transparent" }

		Cull Off
		ZWrite Off
		Lighting Off
		Fog { Mode off }

		//Pass 0 Mask
		Pass 
		{
			Name "Clear"
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			ENDCG
		} 
	}
}
