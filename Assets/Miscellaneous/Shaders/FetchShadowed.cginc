
#ifndef AMOGUS
#define AMOGUS

//#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

void MyFunctionA_float(float3 world, out float2 shadowed)
{
	shadowed = 0.0;
	//shadowed = TransformWorldToShadowCoord(world);
	//shadowed = SAMPLE_TEXTURE2D(_ScreenSpaceShadowmapTexture, sampler_ScreenSpaceShadowmapTexture, uv).rgba;
}

#endif