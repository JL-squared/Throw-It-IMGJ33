#ifndef CUSTOM_LIT_PASS_INCLUDED
#define CUSTOM_LIT_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

void MyFunctionA_float(float3 diffuse, float3 normal, float smoothness, float metallic, out float3 colour)
{
	colour = normal;
}