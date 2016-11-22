Shader "Skin/Post Effects/Film Grain" {
Properties {
	_MainTex ("", 2D) = "white" {}
	_NoiseTex ("Noise", 2D) = "white" {}
	
	pixelSize ("Pixel Size", Vector) = (0,0,0,0)
	noiseIntensity ("Noise Intensity", Float) = 1.0
	exposure ("Exposure", Float) = 1.0
}
 
SubShader {
 
ZTest Always Cull Off ZWrite Off Fog { Mode Off } //Rendering settings
 
	Pass{
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#include "UnityCG.cginc" 
		//we include "UnityCG.cginc" to use the appdata_img struct
    
		 
		float2 pixelSize;			
		float noiseIntensity;
		float exposure;
		float t;
		

		struct v2f {
			float4 pos : POSITION;
			half2 uv : TEXCOORD0;
		};

		sampler2D _MainTex; //Reference in Pass is necessary to let us use this variable in shaders
		sampler2D _NoiseTex; //Reference in Pass is necessary to let us use this variable in shaders

		float3 Overlay(float3 a, float3 b){
			return pow(abs(b), 2.2) < 0.5? 2 * a * b : 1.0 - 2 * (1.0 - a) * (1.0 - b);
		}

		float3 AddNoise(float3 color, float2 texcoord) {
			float2 coord = texcoord * 2.0;
			coord.x *= pixelSize.x / pixelSize.y;
			float noise = tex2D(_NoiseTex, float2(coord)).r;
			float exposureFactor = exposure / 2.0;
			exposureFactor = sqrt(exposureFactor);
			float t = lerp(3.5 * noiseIntensity, 1.13 * noiseIntensity, exposureFactor);
			return Overlay(color, lerp(0.5, noise, t));
		}
   
		//Our Vertex Shader 
		v2f vert (appdata_img v){
			v2f o;
			o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
			o.uv = MultiplyUV (UNITY_MATRIX_TEXTURE0, v.texcoord.xy);
			return o; 
		}   		
    
		//Our Fragment Shader
		fixed4 frag (v2f i) : COLOR{
			fixed3 finalColor = tex2D(_MainTex, i.uv).rgb;
			finalColor = AddNoise(finalColor, i.uv);
			return float4(finalColor, 1.0);
		}
		ENDCG
	}
} 
 FallBack "Diffuse"
}
