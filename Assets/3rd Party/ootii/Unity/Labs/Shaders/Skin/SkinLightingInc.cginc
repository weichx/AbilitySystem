#ifndef UNITY_PBS_LIGHTING_INCLUDED
#define UNITY_PBS_LIGHTING_INCLUDED

#include "UnityShaderVariables.cginc"
#include "UnityStandardConfig.cginc"
#include "UnityLightingCommon.cginc"
#include "UnityGlobalIllumination.cginc"

//-------------------------------------------------------------------------------------
// Default BRDF to use:
#if !defined (UNITY_BRDF_PBS) // allow to explicitly override BRDF in custom shader
	#if (SHADER_TARGET < 30) || defined(SHADER_API_PSP2)
		// Fallback to low fidelity one for pre-SM3.0
		#define UNITY_BRDF_PBS BRDF3_Unity_PBS
	#elif defined(SHADER_API_MOBILE)
		// Somewhat simplified for mobile
		#define UNITY_BRDF_PBS BRDF2_Unity_PBS
	#else
		// Full quality for SM3+ PC / consoles
		#define UNITY_BRDF_PBS BRDF1_Unity_PBS
	#endif
#endif

//-------------------------------------------------------------------------------------
// BRDF for lights extracted from *indirect* directional lightmaps (baked and realtime).
// Baked directional lightmap with *direct* light uses UNITY_BRDF_PBS.
// For better quality change to BRDF1_Unity_PBS.
// No directional lightmaps in SM2.0.

//#if !defined(UNITY_BRDF_PBS_LIGHTMAP_INDIRECT)
//	#define UNITY_BRDF_PBS_LIGHTMAP_INDIRECT BRDF2_Unity_PBS
//#endif
//#if !defined (UNITY_BRDF_GI)
//	#define UNITY_BRDF_GI BRDF_Unity_Indirect
//#endif
//
////-------------------------------------------------------------------------------------
//
//
//inline half3 BRDF_Unity_Indirect (half3 baseColor, half3 specColor, half oneMinusReflectivity, half oneMinusRoughness, half3 normal, half3 viewDir, half occlusion, UnityGI gi)
//{
//	half3 c = 0;
//	#if defined(DIRLIGHTMAP_SEPARATE)
//		gi.indirect.diffuse = 0;
//		gi.indirect.specular = 0;
//
//		#ifdef LIGHTMAP_ON
//			c += UNITY_BRDF_PBS_LIGHTMAP_INDIRECT (baseColor, specColor, oneMinusReflectivity, oneMinusRoughness, normal, viewDir, gi.light2, gi.indirect).rgb * occlusion;
//		#endif
//		#ifdef DYNAMICLIGHTMAP_ON
//			c += UNITY_BRDF_PBS_LIGHTMAP_INDIRECT (baseColor, specColor, oneMinusReflectivity, oneMinusRoughness, normal, viewDir, gi.light3, gi.indirect).rgb * occlusion;
//		#endif
//	#endif
//	return c;
//}

//-------------------------------------------------------------------------------------
// Custom Specular 

float Fresnel(float3 _half, float3 view, float f0) {
	float base = 1.0 - dot(view, _half);
	float exponential = pow(base, 5.0);
	return exponential + f0 * (1.0 - exponential);
}

half SpecularKSK(sampler2D beckmannTex, float3 normal, float3 light, float3 view, float roughness) {
	half3 _half = view + light;
	half3 halfn = normalize(_half);

	half ndotl = max(dot(normal, light), 0.0);
	half ndoth = max(dot(normal, halfn), 0.0);

	half ph = pow(2.0 * tex2D(beckmannTex, float2(ndoth, roughness)).r, 10.0);
	half f = lerp(0.25, Fresnel(halfn, view, 0.028), _specularFresnel);
	half ksk = max(ph * f / dot(_half, _half), 0.0);

	return ndotl * ksk;   
}

//-------------------------------------------------------------------------------------
// Surface shader output structure to be used with physically
// based shading model.

//-------------------------------------------------------------------------------------
// Metallic workflow

struct SurfaceOutputSkinStandard
{
	fixed3 Albedo;		// diffuse color
	fixed3 Normal;		// tangent space normal, if written
	half3 Emission;
	half Specular;		// metallic
	half Smoothness;	// 0=rough, 1=smooth
	half Occlusion;
	fixed Alpha;
	
	fixed SSS;
	fixed NormalLod;
	fixed3 NormalBlur;
	float Curvature;
	float AmbientContribution;
};


//Compute lighting for skin
half3 Skin_BRDF_PBS (SurfaceOutputSkinStandard s, float oneMinusReflectivity, half3 viewDir,
	UnityLight light, UnityIndirect gi)
{
	half3 normalizedLightDir = normalize(light.dir);
	viewDir = normalize(viewDir);

	float3 f1 = light.color.rgb * s.Occlusion;
	float3 f2 = s.Albedo * f1;

	//we compute 2 specular lobes and linearly blend them. Second one is harsher, to simulate small detail specularity. It is derived from 1st one
	//to decrease the number of maps needed. Also pass 1.0 - Smoothness, as the function want roughness
	half specular = _EnableSpecular == 0.0 ? 0.0 : (intensity * SpecularKSK(_BeckmannTex, s.NormalBlur, normalizedLightDir, viewDir , s.Smoothness) );
	half specular2 = _EnableSecondSpec == 0.0 ? 0.0 : (intensity * SpecularKSK(_BeckmannTex, s.Normal, normalizedLightDir, viewDir , (s.Smoothness + 0.2)));

	//main specular is stronger, detail specular get less influence
	const float blendAmount = 0.6f;
	specular = blendAmount * specular + (1.0f - blendAmount) * specular2;

	float dotNL = dot(s.Normal, normalizedLightDir);;
	half3 brdf = float3(1,1,1);
	
	float dotNLBlur = dot(s.NormalBlur, normalizedLightDir);
	float2 brdfUV;
	brdfUV.x = dotNLBlur * 0.5 + 0.5;
	//curvature is weighted by light luminosity. So for a same curvature, a brighter light will scatter more.
	brdfUV.y = s.Curvature * dot(light.color, fixed3(0.22, 0.707, 0.071));

	//BRDF texture contains cached values of shading parametrized by dotNL in x and curvature in y
	//high curvature zone have a smooth white -> red -> black ramp for dotNL
	//low curvature zone have a simpler white -> black ramp
	brdf = tex2D( _BRDFTex, brdfUV ).rgb;
	//dotNL = max(0.0, dotNLBlur);

	half3 color = half3(0,0,0);

	half3 transLightDir = light.dir + s.NormalBlur * _sssDistortion;
	float transDot = pow ( saturate(dot ( viewDir, -transLightDir ) ) * s.SSS * s.SSS, _sssPower ) * _sssScale;
	half3 lightScattering = transDot * _sssSubColor * light.color;

	half nv = DotClamped (s.NormalBlur, viewDir);
	half grazingTerm = saturate(1-s.Smoothness + (1-oneMinusReflectivity));

	color.rgb += 0
				+ s.Albedo * (_ambientContribution * gi.diffuse + f1 * lerp(saturate(dotNLBlur.xxx), brdf, s.SSS)) * 1.570796
				+ specular * light.color
				+ lightScattering
				+ gi.specular * FresnelLerp (specular, grazingTerm, nv) * _ambientContribution
				;

	return color;
}

//compute GI contribution to the skin shading, but character don't use lightmap
inline half3 Skin_BRDF_Indirect (SurfaceOutputSkinStandard s, half3 viewDir, UnityGI gi)
{
	return half3(0,0,0);
}

inline half4 LightingSkinStandard (SurfaceOutputSkinStandard s, half3 viewDir, UnityGI gi)
{
	s.Normal = normalize(s.Normal);

	half oneMinusReflectivity;
	half3 specColor;
	s.Albedo = EnergyConservationBetweenDiffuseAndSpecular (s.Albedo, s.Specular, /*out*/ oneMinusReflectivity);

	// shader relies on pre-multiply alpha-blend (_SrcBlend = One, _DstBlend = OneMinusSrcAlpha)
	// this is necessary to handle transparency in physically correct way - only diffuse component gets affected by alpha
	half outputAlpha;
	s.Albedo = PreMultiplyAlpha (s.Albedo, s.Alpha, oneMinusReflectivity, /*out*/ outputAlpha);
	
	half4 color = half4(0.0, 0.0, 0.0, 1.0);
		
	color.a = s.Alpha;			

	color.rgb = Skin_BRDF_PBS(s, oneMinusReflectivity, viewDir, gi.light, gi.indirect);

	return color;
}

inline void LightingSkinStandard_GI (
	SurfaceOutputSkinStandard s,
	UnityGIInput data,
	inout UnityGI gi)
{
	gi = UnityGlobalIllumination (data, s.Occlusion, s.Smoothness, s.NormalBlur);
}

#endif // UNITY_PBS_LIGHTING_INCLUDED
