// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

Shader "WebVerse/LobbyFloorGrid"
{
    Properties
    {
        [Header(Grid)]
        _GridColor ("Grid Color", Color) = (0.3, 0.5, 0.7, 1)
        _GridSize ("Grid Size", Range(0.1, 10)) = 1.0
        _GridThickness ("Grid Thickness", Range(0.001, 0.1)) = 0.02
        _GridBrightness ("Grid Brightness", Range(0, 2)) = 0.8

        [Header(Fade)]
        _FadeStart ("Fade Start Distance", Range(0, 20)) = 2.0
        _FadeEnd ("Fade End Distance", Range(1, 50)) = 15.0
        _CenterBrightness ("Center Brightness Boost", Range(0, 2)) = 0.5

        [Header(Underglow)]
        _UnderglowColor ("Underglow Color", Color) = (0.4, 0.3, 0.2, 1)
        _UnderglowIntensity ("Underglow Intensity", Range(0, 2)) = 0.6
        _UnderglowRadius ("Underglow Radius", Range(0.5, 10)) = 3.0
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
            #include "UnityCG.cginc"

            // Grid
            fixed4 _GridColor;
            float _GridSize;
            float _GridThickness;
            float _GridBrightness;

            // Fade
            float _FadeStart;
            float _FadeEnd;
            float _CenterBrightness;

            // Underglow
            fixed4 _UnderglowColor;
            float _UnderglowIntensity;
            float _UnderglowRadius;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Distance from origin (center of grid)
                float2 centerOffset = i.worldPos.xz;
                float distFromCenter = length(centerOffset);

                // Grid calculation
                float2 gridPos = i.worldPos.xz / _GridSize;
                float2 grid = abs(frac(gridPos - 0.5) - 0.5);
                float2 gridLine = smoothstep(0, _GridThickness, grid);
                float gridMask = 1.0 - min(gridLine.x, gridLine.y);

                // Distance fade (fades out toward edges)
                float distanceFade = 1.0 - saturate((distFromCenter - _FadeStart) / (_FadeEnd - _FadeStart));
                distanceFade = pow(distanceFade, 1.5); // Smooth falloff

                // Center brightness boost
                float centerBoost = 1.0 + _CenterBrightness * saturate(1.0 - distFromCenter / _FadeStart);

                // Underglow (warm glow from below, concentrated at center)
                float underglowMask = saturate(1.0 - distFromCenter / _UnderglowRadius);
                underglowMask = pow(underglowMask, 2.0); // Soft falloff
                fixed4 underglow = _UnderglowColor * underglowMask * _UnderglowIntensity;

                // Combine grid with underglow
                fixed4 gridCol = _GridColor * gridMask * _GridBrightness * centerBoost;
                fixed4 finalColor = gridCol + underglow;

                // Apply distance fade to alpha
                finalColor.a = (gridMask * 0.8 + underglowMask * 0.4) * distanceFade;

                return finalColor;
            }
            ENDCG
        }
    }

    FallBack Off
}
