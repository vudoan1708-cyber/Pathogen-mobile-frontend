Shader "Pathogen/BioPulse"
{
    Properties
    {
        _Color          ("Color",            Color)          = (0.3, 0.5, 0.8, 1)
        _FillAlpha      ("Fill Alpha",       Range(0, 0.3))  = 0.06
        _EdgeAlpha      ("Edge Alpha",       Range(0, 1))    = 0.7
        _EdgeWidth      ("Edge Width",       Range(0.01, 0.3)) = 0.07
        _EdgeSoftness   ("Edge Softness",    Range(0.005, 0.1)) = 0.025
        _PulseSpeed     ("Pulse Speed",      Range(0.1, 3))  = 0.7
        _PulseIntensity ("Pulse Intensity",  Range(0, 0.4))  = 0.18
        _NoiseScale     ("Edge Noise Scale", Range(1, 20))   = 5
        _NoiseAmount    ("Edge Noise Amt",   Range(0, 0.04)) = 0.012
        _FillNoiseScale ("Fill Noise Scale", Range(1, 30))   = 10
        _FillNoiseAmount("Fill Noise Amt",   Range(0, 0.6))  = 0.35
        _ShowArrow      ("Show Arrow",       Float)          = 0
        _ArrowStart     ("Arrow Start V",    Range(0.3, 0.95)) = 0.6
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent+10" "RenderPipeline"="UniversalPipeline" "IgnoreProjector"="True" }

        Pass
        {
            Name "BioPulse"
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off  ZTest LEqual  Cull Off  Offset -1, -1

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 pos : POSITION; float2 uv : TEXCOORD0; float2 uv2 : TEXCOORD1; };
            struct Varyings  { float4 cs : SV_POSITION; float2 uv : TEXCOORD0; float3 ws : TEXCOORD1; float2 raw : TEXCOORD2; };

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float _FillAlpha, _EdgeAlpha, _EdgeWidth, _EdgeSoftness;
                float _PulseSpeed, _PulseIntensity;
                float _NoiseScale, _NoiseAmount, _FillNoiseScale, _FillNoiseAmount;
                float _ShowArrow, _ArrowStart;
            CBUFFER_END

            // ─── NOISE ──────────────────────────────────────────────────

            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float valueNoise(float2 p)
            {
                float2 i = floor(p), f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                return lerp(
                    lerp(hash21(i), hash21(i + float2(1, 0)), f.x),
                    lerp(hash21(i + float2(0, 1)), hash21(i + float2(1, 1)), f.x), f.y);
            }

            float fbm(float2 p)
            {
                float v = 0.0, amp = 0.5;
                for (int i = 0; i < 3; i++) { v += valueNoise(p) * amp; p *= 2.1; amp *= 0.45; }
                return v;
            }

            // ─── VERTEX / FRAGMENT ──────────────────────────────────────

            Varyings vert(Attributes i)
            {
                Varyings o;
                o.cs  = TransformObjectToHClip(i.pos.xyz);
                o.ws  = TransformObjectToWorld(i.pos.xyz);
                o.uv  = i.uv;
                o.raw = i.uv2;
                return o;
            }

            float4 frag(Varyings i) : SV_Target
            {
                float edgeProx = i.uv.x;

                // Edge noise + band
                float noise = fbm(i.ws.xz * _NoiseScale + _Time.y * 0.25) * _NoiseAmount;
                float threshold = _EdgeWidth + noise;
                float edgeMask = 1.0 - smoothstep(threshold - _EdgeSoftness, threshold + _EdgeSoftness, edgeProx);
                float outerFade = smoothstep(0.0, 0.012, edgeProx);

                // Pulse wave
                float pulse = pow(sin((edgeProx * 5.0 - _Time.y * _PulseSpeed) * 6.28318) * 0.5 + 0.5, 5.0)
                    * _PulseIntensity
                    * smoothstep(0.0, _EdgeWidth * 2.5, edgeProx)
                    * smoothstep(1.0, 0.6, edgeProx);

                // Fill noise
                float fillPattern = lerp(1.0 - _FillNoiseAmount, 1.0, fbm(i.ws.xz * _FillNoiseScale + _Time.y * 0.12));

                // Compose
                float a = lerp(_FillAlpha * fillPattern, _EdgeAlpha, edgeMask) + pulse * 0.12;
                a *= outerFade;

                float3 col = _Color.rgb;
                col = lerp(col, col * 1.5 + 0.08, edgeMask) + pulse * 0.12;

                if (_ShowArrow > 0.5)
                {
                    float lateral = abs(i.raw.x - 0.5);
                    float rawV = i.raw.y;
                    float bodyFade = smoothstep(0.0, 0.06, rawV) * smoothstep(1.0, 0.92, rawV);

                    // 2 parallel guide lines along the body (semi-transparent)
                    float line1 = 1.0 - smoothstep(0.0, 0.018, abs(lateral - 0.15));
                    float line2 = 1.0 - smoothstep(0.0, 0.018, abs(lateral - 0.35));
                    float lines = max(line1, line2) * bodyFade * step(rawV, _ArrowStart);
                    col += _Color.rgb * 0.6 * lines * 0.4;
                    a = max(a, lines * 0.3);

                    // Bold bright chevron at the tip
                    float tipMask = smoothstep(_ArrowStart - 0.05, _ArrowStart, rawV)
                                  * smoothstep(1.0, 0.92, rawV);
                    float cv = rawV + lateral * 0.6;
                    float chevron = 1.0 - smoothstep(0.0, 0.03, abs(frac(cv * 2.5) - 0.5));
                    chevron *= tipMask;
                    col += (_Color.rgb * 1.6 + 0.15) * chevron * 0.8;
                    a = max(a, chevron * 0.75);
                }

                return float4(col, saturate(a));
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Unlit"
}
