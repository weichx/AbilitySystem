// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/TransparencyBlit" 
{
	Properties 
	{
		_Angle ("Angle", Float) = 0
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}

		CGINCLUDE

#include "UnityCG.cginc"
#pragma glsl

    float       _Angle;
	sampler2D	_MainTex;
	
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
		float4 lBaseColor = tex2D(_MainTex, rInput.uv);
		
		float lAlpha = lBaseColor.a;

		float lAngle = atan2(rInput.uv.x - 0.5, rInput.uv.y - 0.5);
		if (lAngle > _Angle)
		{
			lAlpha = 0.0;
		}

		lBaseColor.a = lAlpha;
		
		return lBaseColor; 
	}

	ENDCG	
	 
	Subshader 
	{
		Tags { "Queue" = "Transparent" }

		//ZTest Off
		Cull Off
		ZWrite Off
		Lighting Off
	    Blend SrcAlpha OneMinusSrcAlpha
		Fog { Mode off }

		//Pass 0 Mask
		Pass 
		{
			Name "Mask"
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			ENDCG
		} 
	}
}
