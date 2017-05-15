// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/GraphicsManagerUI"
{
	Properties
	{
	}

	SubShader
	{
		Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" "PerformanceChecks" = "False" }
		LOD 200

		Pass
	{
		ZTest Always
		Cull Off
		ZWrite Off
		Lighting Off
		Blend SrcAlpha OneMinusSrcAlpha

		CGPROGRAM
#include "UnityCG.cginc"
#pragma vertex Vert
#pragma fragment Frag

	struct VertOut
	{
		float4 Position : POSITION;
		float4 Color : COLOR;
	};

	struct VertIn
	{
		float4 Vertex : POSITION;
		float4 Color : COLOR;
	};

	VertOut Vert(VertIn rInput, float3 rNormal : NORMAL)
	{
		VertOut lOutput;
		lOutput.Position = UnityObjectToClipPos(rInput.Vertex);
		lOutput.Color = rInput.Color;

		return lOutput;
	}

	float4 Frag(VertOut rInput) : COLOR
	{
		return rInput.Color;
	}

		ENDCG
	}
	}

		Fallback off
}