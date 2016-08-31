
// Upgrade NOTE: commented out 'sampler2D unity_Lightmap', a built-in variable
// Upgrade NOTE: replaced tex2D unity_Lightmap with UNITY_SAMPLE_TEX2D

// Skin Shader for Unity 5
// Ported by Simon Evans (info@v16studios.co.uk)

Shader "Custom/Skin" {
	Properties {
		_MainTex ("Diffuse", 2D) = "white" {}
		_NormalTex ("Normal", 2D) = "white" {}
		_NormalDetailTex ("Normal Detail", 2D) = "white" {}
		_SpecularAOTex ("Specular & AO", 2D) = "white" {}
		_BeckmannTex ("Beckmann", 2D) = "white" {}
		_BRDFTex ("BRDF", 2D) = "white" {}

		_bumpiness ("Normal Amount", Range(0.0,3.0)) = 0.9
		[MaterialToggle] _EnableNormalDetail ("Enable Normal Detail", Float) = 0.0
		_normalDetailLevel ("Normal Detail Amount", Range(0.0,3.0)) = 0.9
		[MaterialToggle] _EnableSpecular ("Enable Specular", Float) = 1.0
		_specularIntensity ("Specular Intensity", Range(0.0,16.0)) = 1.88
		_specularRoughness ("Specular Roughness", Range(0.01,2.0)) = 0.2
		_specularFresnel ("Specular Fresnel", Range(0.0, 2.0)) = 0.1
		[MaterialToggle] _EnableSecondSpec ("Enable Second Spec", Float) = 1.0
		_normalLOD ("Normal Map Blur Bias", Float) = 1.5
		_sssCurve ("Curvature Scale", Range(0.002, 2)) = 0.02
		_sssPower ("Subsurface Power (1.0 - 5.0)", Range(1.0, 5.0)) = 2.0
		_sssDistortion ("Subsurface Distortion (0.0 - 0.5)", Range(0.0, 5.0)) = 0.1
		_sssScale ("Subsurface Scale (1.0 - )", Range(0.0, 1.0)) = 1.0
		_sssSubColor ("Subsurface Color", Color) = (1, .4, .25, 1)
		
		_sssAmount ("SSS Amount", Range(0.0,1.0)) = 1.0

		_ambientContribution ("Ambient Contribution", Range(0.0,1.0)) = 1.0


	}
	SubShader {
		Tags { "RenderType"="Opaque" "LightCount" = "4" "Queue" = "Geometry" }		
		Fog {Mode Off}
		LOD 200	
				
		CGPROGRAM
		#pragma target 3.0
		#pragma surface RenderPS SkinStandard vertex:RenderVS nofog fullforwardshadows

////////// Setup
		sampler2D _MainTex;			
		sampler2D _NormalTex;		// Condition as Normal Map, uncompressed if possible.
		sampler2D _NormalDetailTex;	// Condition as Normal Map, uncompressed if possible.
		sampler2D _SpecularAOTex;	// Bypass sRGB
		sampler2D _BeckmannTex;		// Bypass sRGB (Important!) and Uncompressed
		sampler2D _BRDFTex;		// Bypass sRGB (Important!) and Uncompressed

		float _EnableNormalDetail;
		float _bumpiness;
		float _normalDetailLevel;
		float _EnableSpecular;
		float _specularIntensity;
		float _specularRoughness;
		float _specularFresnel;
		float _EnableSSS;
		float _normalLOD;
		float _sssCurve;
		float _sssPower;
		float _sssDistortion;
		float _sssScale;
		half4 _sssSubColor;
		float _sssAmount;
		float _ambientContribution;
		float _EnableSecondSpec;

		float2 jitter;


		// Lighting Params		
		float intensity;
		float4x4 _Light2World;
		float4x4 _World2Light;
		float4x4 _Object2Light;
		
		#include "SkinLightingInc.cginc" 

////////// Struct
		struct Input {			
			float2 uv_MainTex;
			float2 uv_NormalDetailTex;

			float3 worldPos;
			float4 view;
			float4 normal;
			float4 tangent;
			float3 worldNormal;
			INTERNAL_DATA
		};
		
////////// Helper Functions
		float3 BumpMap(sampler2D normalTex, float2 texcoord) {
			float3 bump;
			bump.xy = -1.0 + 2.0 * tex2D(normalTex, texcoord).wy;
			bump.z = sqrt(1.0 - bump.x * bump.x - bump.y * bump.y);
			return normalize(bump);
		}
		
		float3 BumpMapLod(sampler2D normalTex, float2 texcoord, float bias) {
			float3 bump;
			bump.xy = -1.0 + 2.0 * tex2Dlod(normalTex, float4(texcoord, 0.0, bias)).wy;
			bump.z = sqrt(1.0 - bump.x * bump.x - bump.y * bump.y);
			return normalize(bump);
		}
		
////////// Shaders
		void RenderVS( inout appdata_full v, out Input output ) {
			UNITY_INITIALIZE_OUTPUT(Input,output);
			
			output.uv_MainTex = v.texcoord.xy;
			output.uv_NormalDetailTex = v.texcoord2.xy;
			
			float3 worldPosition = mul(v.vertex, _Object2World).xyz;

			//world position is stcoked in w of viewspace, normal & tangent output (save one output)
			output.view = float4(WorldSpaceViewDir(v.vertex).xyz, worldPosition.x);
			output.normal = float4(v.normal.xyz, worldPosition.y); 
			output.tangent = float4(mul((float3x3)_Object2World, v.tangent.xyz), worldPosition.z);
		}
		
		void RenderPS( Input IN, inout SurfaceOutputSkinStandard o ) {
			
			float3 tangentNormal = UnpackScaleNormal(tex2D(_NormalTex, IN.uv_MainTex), _bumpiness);
			//if detail is enable, normal is perturbated with the detail normal
			float3 normal = _EnableNormalDetail == 1.0 ? tangentNormal + UnpackScaleNormal(tex2D(_NormalDetailTex, IN.uv_NormalDetailTex), _normalDetailLevel) : tangentNormal;	

			float4 albedo = tex2D(_MainTex, IN.uv_MainTex);
			float4 specularAO = tex2D(_SpecularAOTex, IN.uv_MainTex);
			
			intensity = specularAO.r * _specularIntensity;
			
			o.Occlusion = specularAO.b;
			o.SSS = specularAO.a * _sssScale;					

			o.Albedo = albedo.rgb;		
			o.Alpha = 0.0;
			o.Smoothness = (specularAO.g / 0.3) * _specularRoughness;
			o.Normal = normal;						
			o.AmbientContribution = _ambientContribution;

			//sample the normal map at a lower LOD level to filter it
			fixed3 blurredWorldNormal = UnpackNormal(tex2Dlod(_NormalTex, float4(IN.uv_MainTex, 0.0, _normalLOD)));	
			blurredWorldNormal = WorldNormalVector(IN, blurredWorldNormal);
			o.NormalBlur = blurredWorldNormal;

			float deltaWorldNormal = length( fwidth( blurredWorldNormal ) );
			float deltaWorldPosition = length( fwidth ( IN.worldPos ) );
			
			o.Curvature = (deltaWorldNormal / deltaWorldPosition) * _sssCurve;					
			
		}

		ENDCG
	} 
	FallBack "Diffuse"
	CustomEditor "SkinShaderGUI"
}