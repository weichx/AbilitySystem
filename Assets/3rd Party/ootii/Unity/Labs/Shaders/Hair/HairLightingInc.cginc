// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

#ifndef UNITY_PBS_LIGHTING_INCLUDED
#define UNITY_PBS_LIGHTING_INCLUDED

#include "UnityShaderVariables.cginc"
#include "UnityStandardConfig.cginc"
#include "UnityLightingCommon.cginc"
#include "UnityGlobalIllumination.cginc"



// Hair specific functions

float3 ShiftTangent(float3 T, float3 N, float shift)
{
	float3 shiftedT = N + shift * T ;
	return normalize(shiftedT);
}

float StrandSpecular(float3 T, float3 V, float3 L, float exponent)
{
	half3 h = normalize(L + V);
	half diff = max(0, dot(T, L));
	
	float nh = max(0, dot(T, h));
	float spec = pow(nh, exponent);
	
	return diff * spec;
}

//====

struct SurfaceOutputHairStandard
{
	fixed3 Albedo;		// diffuse color
	fixed3 Normal;		// tangent space normal, if written
	half3 Emission;
	half Specular;		// metallic
	half Smoothness;	// 0=rough, 1=smooth
	half Occlusion;
	fixed Alpha;

	float4 Tangent;
	half3 SpecShift;
	half3 SpecMask;
	fixed Brightness;
};

inline half4 LightingHairStandard (SurfaceOutputHairStandard s, half3 viewDir, UnityGI gi)
{
	half3 normalizedLightDir = normalize(gi.light.dir);

	half oneMinusReflectivity;
	half3 specColor;

	s.Albedo = DiffuseAndSpecularFromMetallic (s.Albedo, s.Specular, /*out*/ specColor, /*out*/ oneMinusReflectivity);

	// shader relies on pre-multiply alpha-blend (_SrcBlend = One, _DstBlend = OneMinusSrcAlpha)
	// this is necessary to handle transparency in physically correct way - only diffuse component gets affected by alpha
	half outputAlpha = s.Alpha;
	s.Albedo = PreMultiplyAlpha (s.Albedo, s.Alpha, oneMinusReflectivity, /*out*/ outputAlpha);
	
	float3 binormal = cross(s.Normal, s.Tangent.xyz) * s.Tangent.w;
	float3 ansio = normalize(mul(unity_ObjectToWorld, float4(binormal,0)).xyz); 
	
	float shiftTex = s.SpecShift.r;
	float3 t1 = ShiftTangent(ansio, s.Normal, _SpecShift1 + shiftTex);
	float3 t2 = ShiftTangent(ansio, s.Normal, _SpecShift2 + shiftTex);

	float tweakNL = max(0, 0.75 * dot(s.Normal, normalizedLightDir) + 0.25);
	
	float maskFromAlbedo = lerp(1.0, s.Albedo.r, _MaskingPower);

	//two specular, shifted along different tangent.
	half3 specular = _SpecAmount1 * _SpecColour1.rgb * StrandSpecular(t1, viewDir, normalizedLightDir, _SpecPower1) * tweakNL * outputAlpha * maskFromAlbedo;
	specular += _SpecAmount2 * _SpecColour2.rgb * s.SpecMask.r * StrandSpecular(t2, viewDir, normalizedLightDir, _SpecPower2) * tweakNL * outputAlpha * maskFromAlbedo;
	
	half4 color;

	half nv = DotClamped (s.Normal, viewDir);
	half grazingTerm = 0.05;

	color.a = outputAlpha;
	float3 f1 = gi.light.color.rgb * s.Occlusion;

	color.rgb =  0
		+ saturate(s.Albedo) * _HairColour * (gi.indirect.diffuse * _ambientDiffuseMultiplier + f1 * tweakNL)
		+ specular * gi.light.color
		//multiplied by specmask, to hide highlight the same way we hide secondary highlight (visual consistency)
		+ gi.indirect.specular * _ambientSpecularMultiplier * FresnelLerp (specular, grazingTerm, nv) * s.SpecMask.r;
		;

	return color;
}

inline void LightingHairStandard_GI (
	SurfaceOutputHairStandard s,
	UnityGIInput data, 
	inout UnityGI gi)
{
	gi = UnityGlobalIllumination (data, s.Occlusion, s.Smoothness, s.Normal);
}

#endif // UNITY_PBS_LIGHTING_INCLUDED
