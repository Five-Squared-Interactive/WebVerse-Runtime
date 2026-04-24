// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

Shader "FiveSQD/ComfortVignette"
{
    Properties
    {
        _VignetteIntensity ("Intensity", Range(0, 1)) = 0
        _InnerRadius ("Inner Radius", Range(0, 1)) = 0.5
        _OuterRadius ("Outer Radius", Range(0, 1)) = 1.0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Overlay+100"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
        }

        ZTest Always
        ZWrite Off
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "ComfortVignette"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
                float _VignetteIntensity;
                float _InnerRadius;
                float _OuterRadius;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                // Calculate distance from center of screen (0.5, 0.5)
                float2 center = float2(0.5, 0.5);
                float dist = distance(input.uv, center) * 2.0; // Normalize to 0-1 range for corners

                // Radial gradient: smooth transition from inner to outer radius
                float vignette = smoothstep(_InnerRadius, _OuterRadius, dist);

                // Apply intensity
                float alpha = vignette * _VignetteIntensity;

                return half4(0, 0, 0, alpha);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
