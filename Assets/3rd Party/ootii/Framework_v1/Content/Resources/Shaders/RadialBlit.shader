Shader "Hidden/RadialBlit" 
{
	Properties 
	{
		_Angle ("Angle", Float) = 0
		_MainTex ("Base (RGB)", 2D) = "white" {}
    	_FillTex ("Base (RGB)", 2D) = "white" {}
	}

		CGINCLUDE

#include "UnityCG.cginc"
#pragma glsl

    float       _Angle;
	sampler2D	_MainTex;
	sampler2D   _FillTex;
	
	struct VertexStruct 
	{
		float4 pos : POSITION;
		float2 uv : TEXCOORD0;
	};
	
	//Common Vertex Shader
	VertexStruct vert( appdata_img rInput )
	{
		VertexStruct lOutput;
		lOutput.pos = mul (UNITY_MATRIX_MVP, rInput.vertex);
		lOutput.uv = rInput.texcoord.xy;
		return lOutput;
	} 
	
	half4 frag(VertexStruct rInput) : COLOR
	{		
		float4 lColor;

		float lAngle = atan2(rInput.uv.x - 0.5, rInput.uv.y - 0.5);
		if (lAngle > _Angle)
		{
			lColor = tex2D(_MainTex, rInput.uv);
		}
		else
		{
			lColor = tex2D(_FillTex, rInput.uv);
		}

		// Comment out if NOT using linear colors
		lColor.r = lColor.r / lColor.a;
		lColor.g = lColor.g / lColor.a;
		lColor.b = lColor.b / lColor.a;

		return lColor; 
	}

	ENDCG	
	 
	Subshader 
	{
		Tags { "Queue" = "Transparent" }

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
