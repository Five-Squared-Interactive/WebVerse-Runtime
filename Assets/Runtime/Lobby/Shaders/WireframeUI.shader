// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

Shader "WebVerse/WireframeUI"
{
    Properties
    {
        [Header(Colors)]
        _Color ("Wire Color", Color) = (0.7, 0.9, 1.0, 1)
        _GlowColor ("Glow Color", Color) = (0.5, 0.8, 1.0, 0.5)
        _BackgroundColor ("Background Color", Color) = (0.1, 0.15, 0.2, 0.3)

        [Header(Wire Settings)]
        _WireThickness ("Wire Thickness", Range(0.001, 0.05)) = 0.01
        _WireBrightness ("Wire Brightness", Range(0, 3)) = 1.5

        [Header(Glow)]
        _GlowIntensity ("Glow Intensity", Range(0, 2)) = 0.8
        _GlowSize ("Glow Size", Range(1, 5)) = 2.0

        [Header(Animation)]
        _PulseSpeed ("Pulse Speed", Range(0, 5)) = 1.0
        _PulseAmount ("Pulse Amount", Range(0, 0.5)) = 0.2
        [Toggle] _EnablePulse ("Enable Pulse", Float) = 1

        [Header(Assembly Animation)]
        _AssemblyProgress ("Assembly Progress", Range(0, 1)) = 1.0
        [Toggle] _UseAssembly ("Use Assembly Animation", Float) = 0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma geometry geom
            #include "UnityCG.cginc"

            // Colors
            fixed4 _Color;
            fixed4 _GlowColor;
            fixed4 _BackgroundColor;

            // Wire
            float _WireThickness;
            float _WireBrightness;

            // Glow
            float _GlowIntensity;
            float _GlowSize;

            // Animation
            float _PulseSpeed;
            float _PulseAmount;
            float _EnablePulse;

            // Assembly
            float _AssemblyProgress;
            float _UseAssembly;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2g
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 worldPos : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            struct g2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 bary : TEXCOORD1;
                float4 worldPos : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2g vert(appdata v)
            {
                v2g o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            [maxvertexcount(3)]
            void geom(triangle v2g input[3], inout TriangleStream<g2f> stream)
            {
                g2f o;

                // Assembly animation - vertices move toward center
                float3 center = (input[0].worldPos.xyz + input[1].worldPos.xyz + input[2].worldPos.xyz) / 3.0;

                for (int i = 0; i < 3; i++)
                {
                    UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(input[i], o);

                    float4 worldPos = input[i].worldPos;

                    // Assembly animation
                    if (_UseAssembly > 0.5)
                    {
                        float3 toCenter = center - worldPos.xyz;
                        float assemblyOffset = 1.0 - _AssemblyProgress;
                        worldPos.xyz += toCenter * assemblyOffset * 0.5;
                    }

                    o.pos = mul(UNITY_MATRIX_VP, worldPos);
                    o.uv = input[i].uv;
                    o.worldPos = worldPos;

                    // Barycentric coordinates for wireframe
                    o.bary = float3(i == 0, i == 1, i == 2);

                    stream.Append(o);
                }
            }

            fixed4 frag(g2f i) : SV_Target
            {
                // Wireframe calculation using barycentric coordinates
                float3 bary = i.bary;
                float minBary = min(min(bary.x, bary.y), bary.z);

                // Wire edge detection
                float wireEdge = smoothstep(0, _WireThickness, minBary);
                float wireMask = 1.0 - wireEdge;

                // Glow around wire
                float glowEdge = smoothstep(0, _WireThickness * _GlowSize, minBary);
                float glowMask = (1.0 - glowEdge) * _GlowIntensity;

                // Pulse animation
                float pulse = 1.0;
                if (_EnablePulse > 0.5)
                {
                    pulse = 1.0 + _PulseAmount * sin(_Time.y * _PulseSpeed);
                }

                // Assembly fade
                float assemblyAlpha = 1.0;
                if (_UseAssembly > 0.5)
                {
                    assemblyAlpha = _AssemblyProgress;
                }

                // Combine wire and glow
                fixed4 wireColor = _Color * wireMask * _WireBrightness * pulse;
                fixed4 glowColor = _GlowColor * glowMask;
                fixed4 bgColor = _BackgroundColor * (1.0 - wireMask);

                fixed4 finalColor = wireColor + glowColor + bgColor;
                finalColor.a = saturate(wireMask + glowMask * 0.5 + _BackgroundColor.a) * assemblyAlpha;

                return finalColor;
            }
            ENDCG
        }
    }

    // Fallback for devices that don't support geometry shaders
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _Color;
            float _WireBrightness;
            float _PulseSpeed;
            float _PulseAmount;
            float _EnablePulse;
            float _AssemblyProgress;
            float _UseAssembly;

            struct appdata
            {
                float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float pulse = 1.0;
                if (_EnablePulse > 0.5)
                {
                    pulse = 1.0 + _PulseAmount * sin(_Time.y * _PulseSpeed);
                }

                float assemblyAlpha = _UseAssembly > 0.5 ? _AssemblyProgress : 1.0;

                fixed4 col = _Color * _WireBrightness * pulse;
                col.a *= assemblyAlpha;
                return col;
            }
            ENDCG
        }
    }

    FallBack "Unlit/Transparent"
}
