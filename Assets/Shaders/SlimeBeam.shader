Shader "Pathogen/SlimeBeam"
{
    Properties
    {
        _Color      ("Core Color",     Color)        = (0.35, 0.85, 0.15, 1)
        _EdgeColor  ("Edge Color",     Color)        = (0.08, 0.30, 0.00, 1)
        _Slime      ("Slime Amount",   Range(0, 1))  = 0.85
        _FlowSpeed  ("Flow Speed",     Range(0, 4))  = 1.4
        _PulseSpeed ("Pulse Speed",    Range(0, 4))  = 2.0
        _Thickness  ("Core Thickness", Range(0.1, 1)) = 0.65
        _Alpha      ("Alpha",          Range(0, 1))  = 0.9
    }

    SubShader
    {
        Tags
        {
            "RenderType"      = "Transparent"
            "Queue"           = "Transparent+5"
            "RenderPipeline"  = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        Pass
        {
            Name "SlimeBeam"
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off  ZTest LEqual  Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 pos : POSITION; float2 uv : TEXCOORD0; float4 col : COLOR; };
            struct Varyings   { float4 cs  : SV_POSITION; float2 uv : TEXCOORD0; float4 col : COLOR; };

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _EdgeColor;
                float  _Slime;
                float  _FlowSpeed;
                float  _PulseSpeed;
                float  _Thickness;
                float  _Alpha;
            CBUFFER_END

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
                    lerp(hash21(i),                hash21(i + float2(1, 0)), f.x),
                    lerp(hash21(i + float2(0, 1)), hash21(i + float2(1, 1)), f.x),
                    f.y);
            }

            float fbm(float2 p)
            {
                float v = 0.0, a = 0.5;
                for (int k = 0; k < 3; k++) { v += valueNoise(p) * a; p *= 2.07; a *= 0.5; }
                return v;
            }

            Varyings vert(Attributes i)
            {
                Varyings o;
                o.cs  = TransformObjectToHClip(i.pos.xyz);
                o.uv  = i.uv;
                o.col = i.col;
                return o;
            }

            float4 frag(Varyings i) : SV_Target
            {
                float t = _Time.y;
                float u = i.uv.x;                 // 0 at origin, 1 at target
                float v = i.uv.y;                 // 0..1 across width
                float slime = saturate(_Slime);
                float flow  = t * _FlowSpeed;

                // Organic domain noise — drives thickness wobble + gob alpha
                float n = fbm(float2(u * 5.5 - flow, v * 2.4 + t * 0.15));

                // Thickness wobbles along the beam (slimy segments). Clean side stays steady.
                float thick    = _Thickness + (n - 0.5) * 0.5 * slime;
                float edgeDist = abs(v - 0.5) * 2.0;
                float core     = saturate(thick - edgeDist);

                // Edge jiggle — rough membrane for slime, smooth for immune
                float edgeNoise = (fbm(float2(u * 12.0 + flow, v * 5.0)) - 0.5) * 0.25 * slime;
                float coreSoft  = saturate(core - edgeNoise);

                // Pulse traveling from structure → target
                float pulse = pow(sin((u * 4.0 - t * _PulseSpeed) * 6.28318) * 0.5 + 0.5, 6.0);

                // Alpha: soft falloff + pulse spike + slimy gobs + tip fade
                float alpha  = smoothstep(0.0, 0.38, coreSoft);
                alpha += pulse * 0.35 * smoothstep(0.05, 0.35, coreSoft);
                alpha *= lerp(0.95, 0.4 + n * 0.95, slime);
                alpha *= smoothstep(0.0, 0.05, u) * smoothstep(1.0, 0.94, u);
                alpha *= _Alpha;

                // Color: edge tint → bright core, hot pulse highlights, slight dim on thin skin
                float3 col = lerp(_EdgeColor.rgb, _Color.rgb, smoothstep(0.0, 0.6, coreSoft));
                col += _Color.rgb * pulse * 0.6;
                col  = lerp(col * 0.55, col, smoothstep(0.0, 0.3, coreSoft));

                // Honor vertex color tint/fade (TrailRenderer age fade, LineRenderer color gradient)
                col *= i.col.rgb;
                alpha *= i.col.a;

                return float4(col, saturate(alpha));
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Unlit"
}
