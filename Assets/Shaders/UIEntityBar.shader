Shader "Pathogen/UIEntityBar"
{
    Properties
    {
        _FillPct       ("Fill %",           Range(0, 1)) = 1
        _TrailPct      ("Trail %",          Range(0, 1)) = 1
        _TrailSign     ("Trail Sign",       Float)       = 0  // -1 = damage, +1 = heal, 0 = none
        _TrailAlpha    ("Trail Alpha",      Range(0, 1)) = 0

        _FillColor     ("Fill Color",       Color) = (0.2, 0.7, 0.3, 1)
        _DamageColor   ("Damage Trail",     Color) = (0.9, 0.15, 0.1, 0.7)
        _HealColor     ("Heal Trail",       Color) = (0.5, 1, 0.55, 0.7)
        _BGColor       ("Background",       Color) = (0.12, 0.12, 0.15, 0.85)
        _BorderColor   ("Border",           Color) = (0.35, 0.35, 0.4, 0.9)
        _TickColor     ("Tick Color",       Color) = (0.0, 0.0, 0.0, 0.6)

        _BorderWidth   ("Border Width",     Range(0.005, 0.06)) = 0.025
        _TickSpacing   ("Tick Spacing UV",  Range(0.01, 0.5))   = 0.1
        _TickWidth     ("Tick Width",       Range(0.001, 0.025)) = 0.009
        _Roundness     ("Corner Roundness", Range(0, 0.5))      = 0.15
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent+20"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest Always
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float _FillPct;
                float _TrailPct;
                float _TrailSign;
                float _TrailAlpha;
                float4 _FillColor;
                float4 _DamageColor;
                float4 _HealColor;
                float4 _BGColor;
                float4 _BorderColor;
                float4 _TickColor;
                float _BorderWidth;
                float _TickSpacing;
                float _TickWidth;
                float _Roundness;
            CBUFFER_END

            // ─── SDF ────────────────────────────────────────────────────

            float sdRoundedBox(float2 p, float2 halfSize, float r)
            {
                float2 q = abs(p) - halfSize + r;
                return min(max(q.x, q.y), 0.0) + length(max(q, 0.0)) - r;
            }

            // ─── VERTEX ─────────────────────────────────────────────────

            Varyings vert(Attributes input)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                o.uv = input.uv;
                return o;
            }

            // ─── FRAGMENT ───────────────────────────────────────────────

            float4 frag(Varyings i) : SV_Target
            {
                float2 centred = i.uv - 0.5;

                // Outer shape — rounded rect with AA
                float sdf = sdRoundedBox(centred, float2(0.5, 0.5), _Roundness);
                float outerMask = 1.0 - smoothstep(-0.008, 0.0, sdf);

                // Border band
                float innerSdf = sdRoundedBox(centred, float2(0.5 - _BorderWidth, 0.5 - _BorderWidth), max(_Roundness - _BorderWidth, 0.01));
                float borderMask = outerMask * (1.0 - step(innerSdf, 0.0));

                // Inner mask (inside border)
                float innerMask = 1.0 - smoothstep(-0.006, 0.0, innerSdf);

                // UV remapped to inner area (0–1 within border)
                float2 innerUV = (centred + 0.5 - _BorderWidth) / (1.0 - 2.0 * _BorderWidth);

                // ── Layers ──

                // Background
                float4 col = float4(_BGColor.rgb, _BGColor.a * innerMask);

                // Fill bar
                float fillMask = step(innerUV.x, _FillPct) * innerMask;
                col.rgb = lerp(col.rgb, _FillColor.rgb, fillMask);
                col.a = max(col.a, fillMask * _FillColor.a);

                // Damage / heal trail
                if (_TrailAlpha > 0.001)
                {
                    float lo = min(_FillPct, _TrailPct);
                    float hi = max(_FillPct, _TrailPct);
                    float trailMask = step(lo, innerUV.x) * step(innerUV.x, hi) * innerMask;

                    float4 trailColor = _TrailSign < 0 ? _DamageColor : _HealColor;
                    trailColor.a *= _TrailAlpha;

                    col.rgb = lerp(col.rgb, trailColor.rgb, trailMask * trailColor.a);
                    col.a = max(col.a, trailMask * trailColor.a);
                }

                // 100 HP tick marks (membrane lines)
                if (_TickSpacing > 0.001)
                {
                    float tickX = frac(innerUV.x / _TickSpacing) * _TickSpacing;
                    float tickDist = min(tickX, _TickSpacing - tickX);
                    float tickMask = (1.0 - smoothstep(0.0, _TickWidth, tickDist)) * innerMask;
                    // Only show ticks within the fill or trail region for clarity
                    float barRegion = step(innerUV.x, max(_FillPct, _TrailPct));
                    tickMask *= barRegion;
                    col.rgb = lerp(col.rgb, _TickColor.rgb, tickMask * _TickColor.a);
                }

                // Border
                col.rgb = lerp(col.rgb, _BorderColor.rgb, borderMask * _BorderColor.a);
                col.a = max(col.a, borderMask * _BorderColor.a);

                // Final outer mask
                col.a *= outerMask;

                return col;
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Unlit"
}
