Shader "Projector/AdditiveTint" {
	Properties{
		_Color("Tint Color", Color) = (1,1,1,1)
		_Alpha("Alpha", Range(0, 1)) = 1
		_ShadowTex("Cookie", 2D) = "gray" {}
	}
		Subshader{
		Tags{ "Queue" = "Transparent" }
		Pass{
		ZWrite Off
		ColorMask RGB
		Blend SrcAlpha One // Additive blending
		Offset -1, -1

		CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"

	struct v2f {
		float4 uvShadow : TEXCOORD0;
		float4 pos : SV_POSITION;
	};

	float4x4 _Projector;
	float4x4 _ProjectorClip;

	v2f vert(float4 vertex : POSITION)
	{
		v2f o;
		o.pos = mul(UNITY_MATRIX_MVP, vertex);
		o.uvShadow = mul(_Projector, vertex);
		return o;
	}

	sampler2D _ShadowTex;
	fixed4 _Color;
	float _Attenuation;
	uniform fixed _Alpha;

	fixed4 frag(v2f i) : SV_Target
	{
		// Apply alpha mask
		fixed4 texCookie = tex2Dproj(_ShadowTex, UNITY_PROJ_COORD(i.uvShadow));
		fixed4 outColor = _Color * texCookie.a;
		outColor.a = _Alpha;
		return outColor;
	}
		ENDCG
	}
	}
}