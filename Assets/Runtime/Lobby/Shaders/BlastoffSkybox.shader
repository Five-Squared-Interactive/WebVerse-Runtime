// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

Shader "WebVerse/BlastoffSkybox"
{
    Properties
    {
        [Header(Gradient Colors)]
        _BottomColor ("Bottom Color (Atmosphere)", Color) = (0.102, 0.227, 0.361, 1)
        _MidColor ("Mid Color (Transition)", Color) = (0.051, 0.122, 0.200, 1)
        _TopColor ("Top Color (Space)", Color) = (0.020, 0.039, 0.071, 1)

        [Header(Gradient Settings)]
        _HorizonHeight ("Horizon Height", Range(-1, 1)) = -0.2
        _GradientSharpness ("Gradient Sharpness", Range(0.1, 5)) = 1.5

        [Header(Stars)]
        _StarDensity ("Star Density", Range(0, 500)) = 200
        _StarBrightness ("Star Brightness", Range(0, 2)) = 1.0
        _StarSize ("Star Size", Range(0.001, 0.02)) = 0.005
        _TwinkleSpeed ("Twinkle Speed", Range(0, 5)) = 1.5
        _TwinkleAmount ("Twinkle Amount", Range(0, 1)) = 0.3

        [Header(Motion)]
        _DriftSpeed ("Star Drift Speed", Range(0, 0.1)) = 0.01
        [Toggle] _EnableDrift ("Enable Drift", Float) = 1
    }

    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" "RenderPipeline"="UniversalPipeline" }
        Cull Off ZWrite Off

        Pass
        {
            Name "BlastoffSkybox"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _BottomColor;
                half4 _MidColor;
                half4 _TopColor;
                float _HorizonHeight;
                float _GradientSharpness;
                float _StarDensity;
                float _StarBrightness;
                float _StarSize;
                float _TwinkleSpeed;
                float _TwinkleAmount;
                float _DriftSpeed;
                float _EnableDrift;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.worldPos = input.positionOS.xyz;
                return output;
            }

            // Hash function for procedural stars
            float hash(float3 p)
            {
                p = frac(p * 0.3183099 + 0.1);
                p *= 17.0;
                return frac(p.x * p.y * p.z * (p.x + p.y + p.z));
            }

            // Star field generation
            float stars(float3 dir, float time)
            {
                // Apply drift
                if (_EnableDrift > 0.5)
                {
                    dir.y += time * _DriftSpeed;
                }

                float3 p = dir * _StarDensity;
                float3 i = floor(p);
                float3 f = frac(p);

                float star = 0.0;

                // Check neighboring cells for stars
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        for (int z = -1; z <= 1; z++)
                        {
                            float3 cell = i + float3(x, y, z);
                            float3 cellHash = float3(
                                hash(cell),
                                hash(cell + 127.1),
                                hash(cell + 269.5)
                            );

                            // Random position within cell
                            float3 starPos = cell + cellHash;
                            float3 diff = p - starPos;
                            float dist = length(diff);

                            // Star visibility (only some cells have stars)
                            float hasStar = step(0.92, hash(cell + 431.2));

                            // Star intensity with size falloff
                            float intensity = hasStar * smoothstep(_StarSize, 0.0, dist);

                            // Twinkle
                            float twinkle = 1.0 - _TwinkleAmount * 0.5 *
                                (1.0 + sin(time * _TwinkleSpeed + hash(cell) * 6.28));

                            star += intensity * twinkle;
                        }
                    }
                }

                return star * _StarBrightness;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 dir = normalize(input.worldPos);

                // Vertical gradient based on Y direction
                float height = dir.y;

                // Adjust for horizon height
                float adjustedHeight = (height - _HorizonHeight) * _GradientSharpness;

                // Three-way gradient: bottom -> mid -> top
                float bottomToMid = saturate(adjustedHeight + 1.0);
                float midToTop = saturate(adjustedHeight);

                half4 gradientColor = lerp(
                    lerp(_BottomColor, _MidColor, bottomToMid),
                    _TopColor,
                    midToTop
                );

                // Stars (fade in above horizon, concentrated at top)
                float starVisibility = saturate(height + 0.3);
                starVisibility = pow(starVisibility, 0.5);

                float starField = stars(dir, _Time.y) * starVisibility;

                // Combine gradient with stars
                half4 finalColor = gradientColor + half4(starField, starField, starField, 0);

                return finalColor;
            }
            ENDHLSL
        }
    }

    FallBack Off
}
