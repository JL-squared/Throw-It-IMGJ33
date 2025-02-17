Shader "Custom/ScreenSpaceShadows"
{
    // Yeeted from
    // https://github.com/Unity-Technologies/Graphics/blob/a36d8dd13ab32c6f3436004677a8a83c5096659a/Packages/com.unity.render-pipelines.universal/Shaders/Utils/ScreenSpaceShadows.shader

    SubShader
    {
        Tags{ "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True"}
        



        HLSLINCLUDE

        //Keep compiler quiet about Shadows.hlsl.
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/EntityLighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/ImageBasedLighting.hlsl"
        // Core.hlsl for XR dependencies
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RealtimeLights.hlsl"
        #include "Assets/Miscellaneous/Shaders/CloudValue.cginc"

        float4x4 _sunMatrix;
        float4x4 _invSunMatrix;
        
        float checkScreenSpaceShadows(float3 position) {
            float3 sun_direction = mul(_sunMatrix, float4(0, 0, 1, 0)).xyz;
            float alpha;
            sampleAllClouds_float(position, sun_direction, true, alpha);
            return 1-alpha;
            
            /*
        	float product = 0.0;
            float factor = 0.0003;
            const int total = 2;
        	for	(int x = -total; x <= total; x++) {
        		for	(int y = -total; y <= total; y++) {
                    float alpha;
        			float3 shadowCoord = mul(_sunMatrix, float4(y * factor, x * factor, 1, 0)).xyz;
                    sampleAllClouds_float(position, shadowCoord, alpha);
        			product += alpha;
        		}
        	}
        	return 1 - product / ((total+1) * (total+1));
            */
        }

        float _generalShadowStrength;
        half4 Fragment(Varyings input) : SV_Target
        {
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

#if UNITY_REVERSED_Z
            float deviceDepth = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_PointClamp, input.texcoord.xy).r;
#else
            float deviceDepth = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_PointClamp, input.texcoord.xy).r;
            deviceDepth = deviceDepth * 2.0 - 1.0;
#endif

            //Fetch shadow coordinates for cascade.
            float3 wpos = ComputeWorldSpacePosition(input.texcoord.xy, deviceDepth, unity_MatrixInvVP);
            float4 coords = TransformWorldToShadowCoord(wpos);

            return SAMPLE_TEXTURE2D_X(_ScreenSpaceShadowmapTexture, sampler_PointClamp, input.texcoord.xy).r * checkScreenSpaceShadows(wpos) * _generalShadowStrength;
        }

        ENDHLSL

        Pass
        {
            Name "ScreenSpaceShadows"
            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma multi_compile _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH

            #pragma vertex   Vert
            #pragma fragment Fragment
            ENDHLSL
        }
    }
}