Shader "Pathogen/BioPulse"
{
    Properties
    {
        _Color ("Color", Color) = (0.3, 0.5, 0.8, 1)
        _FillAlpha ("Fill Alpha", Range(0, 0.3)) = 0.06
        _EdgeAlpha ("Edge Alpha", Range(0, 1)) = 0.7
        _EdgeWidth ("Edge Width", Range(0.01, 0.3)) = 0.07
        _EdgeSoftness ("Edge Softness", Range(0.005, 0.1)) = 0.025
        _PulseSpeed ("Pulse Speed", Range(0.1, 3)) = 0.7
        _PulseIntensity ("Pulse Intensity", Range(0, 0.4)) = 0.18
        _NoiseScale ("Edge Noise Scale", Range(1, 20)) = 5
        _NoiseAmount ("Edge Noise Amount", Range(0, 0.04)) = 0.012
        _FillNoiseScale ("Fill Noise Scale", Range(1, 30)) = 10
        _FillNoiseAmount ("Fill Noise Amount", Range(0, 0.6)) = 0.35
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent+10"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        Pass
        {
            Name "BioPulse"
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull Off
            Offset -1, -1

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

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
                float3 positionWS : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float _FillAlpha;
                float _EdgeAlpha;
                float _EdgeWidth;
                float _EdgeSoftness;
                float _PulseSpeed;
                float _PulseIntensity;
                float _NoiseScale;
                float _NoiseAmount;
                float _FillNoiseScale;
                float _FillNoiseAmount;
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
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);

                float a = hash21(i);
                float b = hash21(i + float2(1.0, 0.0));
                float c = hash21(i + float2(0.0, 1.0));
                float d = hash21(i + float2(1.0, 1.0));

                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            float fbm(float2 p)
            {
                float value = 0.0;
                float amp = 0.5;
                for (int i = 0; i < 3; i++)
                {
                    value += valueNoise(p) * amp;
                    p *= 2.1;
                    amp *= 0.45;
                }
                return value;
            }

            // ─── VERTEX ─────────────────────────────────────────────────

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            // ─── FRAGMENT ───────────────────────────────────────────────

            float4 frag(Varyings input) : SV_Target
            {
                // UV.x = edge proximity: 0 at any edge, 1 deep inside
                float edgeProx = input.uv.x;

                // Organic micro-variation on edge boundary
                float2 noiseCoord = input.positionWS.xz * _NoiseScale;
                float edgeNoise = fbm(noiseCoord + _Time.y * 0.25) * _NoiseAmount;

                // Edge band with soft falloff
                float edgeThreshold = _EdgeWidth + edgeNoise;
                float edgeMask = 1.0 - smoothstep(
                    edgeThreshold - _EdgeSoftness,
                    edgeThreshold + _EdgeSoftness,
                    edgeProx);

                // Soft outer dissolve at the very boundary
                float outerFade = smoothstep(0.0, 0.012, edgeProx);

                // Bio-pulse: radial wave expanding from center to edge
                float pulsePhase = edgeProx * 5.0 - _Time.y * _PulseSpeed;
                float pulse = sin(pulsePhase * 6.28318);
                pulse = pulse * 0.5 + 0.5;
                pulse = pow(pulse, 5.0); // sharpen into a narrow traveling ring
                pulse *= _PulseIntensity;
                // Fade pulse near edge (edge glow takes over there)
                pulse *= smoothstep(0.0, _EdgeWidth * 2.5, edgeProx);
                // Fade pulse near center
                pulse *= smoothstep(1.0, 0.6, edgeProx);

                // Organic fill — subtle cellular pattern
                float2 fillCoord = input.positionWS.xz * _FillNoiseScale;
                float fillNoise = fbm(fillCoord + _Time.y * 0.12);
                float fillPattern = lerp(1.0 - _FillNoiseAmount, 1.0, fillNoise);

                // ─── COMPOSE ────────────────────────────────────────────

                // Alpha
                float fillA = _FillAlpha * fillPattern;
                float a = lerp(fillA, _EdgeAlpha, edgeMask);
                a += pulse * 0.12;
                a *= outerFade;

                // Color
                float3 col = _Color.rgb;
                // Edge gets brighter
                col = lerp(col, col * 1.5 + 0.08, edgeMask);
                // Pulse adds subtle brightness
                col += pulse * 0.12;

                return float4(col, saturate(a));
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Unlit"
}
