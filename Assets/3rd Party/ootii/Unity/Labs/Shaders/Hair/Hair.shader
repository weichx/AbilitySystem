//NOTE : For the sahder to work properly, hair mesh need to be properly authored.
//V of UV need to go from hair root (0) to tip, for the shift to work properly
//Also if your mesh is pretty "flat" & ordered inside to outside, you can try
//to switch the shader to Alpha Blended instead of alpha cutout


Shader "Custom/Hair" {
	Properties {
		_Color ("Main Color", Color) = (1,1,1,1)
		_HairColour ("Hair Colour", Color) = (1,1,1,1)
		_SpecColour1 ("Specular Colour 1", Color) = (0.5, 0.5, 0.5, 0)
		_SpecColour2 ("Specular Colour 2", Color) = (0.5, 0.5, 0.5, 0)
		_SpecPower1 ("Spec Power 1", Range (0.01, 128)) = 0.078125
		_SpecPower2 ("Spec Power 2", Range (0.01, 128)) = 0.078125
		_SpecShift1 ("Spec Shift 1", Range (-1, 1)) = 0.5
		_SpecShift2 ("Spec Shift 2", Range (-1, 1)) = 0.5
		_SpecAmount1 ("Spec Amount 1", Range (0.0, 4)) = 1
		_SpecAmount2 ("Spec Amount 2", Range (0.0, 4)) = 1
		_Brightness ("Brightness Shift", Range(0.0, 1.0)) = 0.0
		_MaskingPower("Masking Power", Range(0.0,1.0)) = 1.0
		_MainTex ("Base (RGB) TransGloss (A)", 2D) = "white" {}
		_SpecShiftTex ("Specular Shift Texture", 2D) = "white" {}
		_SpecStrandMaskTex ("Specular Mask Texture", 2D) = "white" {}
		_HorizontalTiling ("Horizontal Hair Tiling", Range(0, 10.0)) = 0.4
		_NormalMap ("Normalmap", 2D) = "bump" {}
		_LOD ("Normal Blur", Range(0.0,5.0)) = 2.4
		_NormalScale ("Normal Amount", Range(0, 1.0)) = 0.4
		_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5

		_ambientDiffuseMultiplier("Ambient Diffuse Multiplier", Range(0.0, 32.0)) = 1.0
		_ambientSpecularMultiplier("Ambient Specular Multiplier", Range(0.0, 32.0)) = 1.0
	}

	SubShader {
		Tags {"Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="TransparentCutout"}
		LOD 400
		Cull Back
		ZWrite On
	

		CGPROGRAM
		#pragma surface surf HairStandard vertex:vert alphatest:_Cutoff nofog 
		#pragma target 3.0		

		sampler2D _MainTex;
		sampler2D _NormalMap;
		sampler2D _SpecShiftTex;
		sampler2D _SpecStrandMaskTex;
		fixed4 _Color;
		fixed4 _SpecColour1;
		fixed4 _SpecColour2;
		fixed4 _HairColour;
		float _SpecPower1;
		float _SpecPower2;
		float _SpecShift1;
		float _SpecShift2; 
		float _SpecAmount1;
		float _SpecAmount2;
		float _Brightness;
		float _HorizontalTiling;
		float _NormalScale;
		float _LOD;
		float _MaskingPower;

		float _ambientDiffuseMultiplier;
		float _ambientSpecularMultiplier;
		
		#include "HairLightingInc.cginc"

		struct Input {			
			float2 uv_MainTex;	

			float3 normal;
			float4 tangent;
			INTERNAL_DATA
		};
		
		void vert (inout appdata_full v, out Input output ) {
			UNITY_INITIALIZE_OUTPUT(Input,output);

			output.tangent = v.tangent;
			output.normal = v.normal;
		}

		void surf (Input IN, inout SurfaceOutputHairStandard o) 
		{
			float2 shiftedUV = frac(IN.uv_MainTex * float2(_HorizontalTiling,1));

			o.Alpha =  tex2D(_MainTex, IN.uv_MainTex).a;

			//Brightness allow to "smooth" hair color, as authored albedo map can have way too much dark
			//giving a "plastic hair helmet" impression
			o.Albedo = saturate(tex2D(_MainTex, shiftedUV).rgb + _Brightness);

			o.Specular = 0.0;

			o.SpecShift = tex2D(_SpecShiftTex, shiftedUV)-0.5;
			o.SpecMask = tex2D(_SpecStrandMaskTex, shiftedUV);
			o.Tangent = IN.tangent;

			//this could be hooked to an occlusion map
			o.Occlusion = 1.0f;

			//Smoothness isn't really used, apart for indirect lighting
			//pluging the mask into it allow for specular highlit to roughly follow hair strands shape
			o.Smoothness = o.SpecMask.r;
			o.Brightness = _Brightness;

			o.Normal = UnpackScaleNormal(tex2Dlod(_NormalMap, float4(IN.uv_MainTex, 0, _LOD)), _NormalScale);
		}
		ENDCG

		
	}

	Fallback "Transparent/Cutout/VertexLit"
}
