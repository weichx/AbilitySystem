Shader "Projector/Grid" {
	Properties {
		_ShadowTex ("Cookie", 2D) = "gray" {}
	}
	Subshader {
		Tags {"Queue"="Transparent"}
		Pass {
			ZWrite Off
			ColorMask RGB
			Blend SrcAlpha OneMinusSrcAlpha // Additive blending
			Offset -1, -1

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			
			struct v2f {
				float4 pos : SV_POSITION;
				float4 uvShadow : TEXCOORD0;
				float2 uvCoord : TEXCOORD1;
			};
			
			float4x4 _Projector;
		    sampler2D _ShadowTex;

			v2f vert (float4 vertex : POSITION, float2 uvCoord : TEXCOORD0) {
				v2f o;
				o.pos = mul (UNITY_MATRIX_MVP, vertex);
				o.uvShadow = mul (_Projector, vertex);
				o.uvCoord = uvCoord;
				return o;
			}
						
			fixed4 frag (v2f i) : SV_Target {
				//clip(i.uvShadow.xyw);
				//clip(1.0 - i.uvShadow.xy);
				//int row = 8 - int(i.uvCoord.x * 8);
				fixed4 texCookie = tex2Dproj (_ShadowTex, UNITY_PROJ_COORD(i.uvShadow));		
				fixed4 color = tex2D(_ShadowTex, i.uvCoord);
				return color * texCookie.a;
			}
			ENDCG
		}
	}
}