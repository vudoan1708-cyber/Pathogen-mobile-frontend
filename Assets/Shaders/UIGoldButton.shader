Shader "Pathogen/UIGoldButton"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _InnerColor  ("Inner Color",  Color) = (0.76, 0.6, 0.38, 0.95)
        _OuterColor  ("Outer Color",  Color) = (0.4, 0.26, 0.13, 0.95)
        _BorderColor ("Border Color", Color) = (0.75, 0.65, 0.05, 1)
        _BorderWidth ("Border Width", Range(0.02, 0.2)) = 0.08
        _BorderGlow  ("Border Glow",  Range(0, 0.5))    = 0.18
        _Roundness   ("Corner Roundness", Range(0, 0.5)) = 0.22

        _PulseSpeed     ("Pulse Speed",     Range(0.2, 4))   = 1.2
        _PulseIntensity ("Pulse Intensity", Range(0, 0.35))  = 0.12

        _NoiseScale  ("Noise Scale",  Range(2, 20))   = 8
        _NoiseAmount ("Noise Amount", Range(0, 0.04)) = 0.012

        _ShineSpeed     ("Shine Speed",     Range(0.1, 2))  = 0.45
        _ShineWidth     ("Shine Width",     Range(0.04, 0.3)) = 0.12
        _ShineIntensity ("Shine Intensity", Range(0, 1))     = 0.55
        _ShinePause     ("Shine Pause Ratio", Range(0, 0.7)) = 0.35

        // ── UI stencil (required for Mask / RectMask2D) ──
        _StencilComp      ("Stencil Comparison", Float) = 8
        _Stencil          ("Stencil ID",         Float) = 0
        _StencilOp        ("Stencil Operation",  Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask  ("Stencil Read Mask",  Float) = 255
        _ColorMask        ("Color Mask",         Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"           = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType"      = "Transparent"
            "PreviewType"     = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Stencil
        {
            Ref  [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "UIGoldButton"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                float4 color  : COLOR;
            };

            struct v2f
            {
                float4 pos           : SV_POSITION;
                float2 uv            : TEXCOORD0;
                float4 color         : COLOR;
                float4 worldPosition : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _ClipRect;

            fixed4 _InnerColor;
            fixed4 _OuterColor;
            fixed4 _BorderColor;
            float  _BorderWidth;
            float  _BorderGlow;

            float _PulseSpeed;
            float _PulseIntensity;

            float _NoiseScale;
            float _NoiseAmount;

            float _ShineSpeed;
            float _ShineWidth;
            float _ShineIntensity;
            float _ShinePause;
            float _Roundness;

            // ─── SDF ────────────────────────────────────────────────────

            // Signed distance to a rounded rectangle centred at origin.
            // halfSize = (0.5, 0.5) for full UV quad, r = corner radius in UV.
            float sdRoundedBox(float2 p, float2 halfSize, float r)
            {
                float2 q = abs(p) - halfSize + r;
                return min(max(q.x, q.y), 0.0) + length(max(q, 0.0)) - r;
            }

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
                float b = hash21(i + float2(1, 0));
                float c = hash21(i + float2(0, 1));
                float d = hash21(i + float2(1, 1));
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            // ─── VERTEX ─────────────────────────────────────────────────

            v2f vert(appdata v)
            {
                v2f o;
                o.worldPosition = v.vertex;
                o.pos   = UnityObjectToClipPos(v.vertex);
                o.uv    = v.uv;
                o.color = v.color;
                return o;
            }

            // ─── FRAGMENT ───────────────────────────────────────────────

            fixed4 frag(v2f i) : SV_Target
            {
                float2 centred = i.uv - 0.5;

                // ── Rounded-rect SDF (replaces circular dist) ──
                float sdf = sdRoundedBox(centred, float2(0.5, 0.5), _Roundness);
                float dist = saturate(sdf / 0.5 + 1.0); // 0 at centre, ~1 at edge

                // Anti-aliased shape mask — sharp outside, smooth 1.5px AA
                float shapeMask = 1.0 - smoothstep(-0.012, 0.0, sdf);

                // ── Organic edge noise ──
                float2 nc = i.uv * _NoiseScale + _Time.y * 0.25;
                float noise = valueNoise(nc) * _NoiseAmount;

                // ── Gradient: inner → outer, driven by SDF distance ──
                float gradT = saturate(dist + noise);
                fixed4 col = lerp(_InnerColor, _OuterColor, gradT * gradT);

                // ── Border with glow bleed ──
                float borderSDF = abs(sdf) - _BorderWidth * 0.12;
                float borderMask = 1.0 - smoothstep(0.0, 0.03, borderSDF);
                col.rgb = lerp(col.rgb, _BorderColor.rgb * 1.4, borderMask * 0.75);
                float glowMask = 1.0 - smoothstep(0.0, 0.06, borderSDF);
                col.rgb += _BorderColor.rgb * glowMask * _BorderGlow;

                // ── Organic breathing pulse ──
                float pulse = sin(_Time.y * _PulseSpeed) * 0.5 + 0.5;
                col.rgb += pulse * _PulseIntensity * _OuterColor.rgb;

                // ── Diagonal glass-shine sweep (bottom-left → top-right) ──
                float rawPhase = frac(_Time.y * _ShineSpeed * 0.25);
                float activeRange = 1.0 - _ShinePause;
                float shinePhase = saturate(rawPhase / activeRange);

                float diagonal = (centred.x + centred.y) * 0.707;
                float shinePos = lerp(-0.85, 0.85, shinePhase);
                float shine = 1.0 - saturate(abs(diagonal - shinePos) / _ShineWidth);
                shine *= shine;
                shine *= step(sdf, -0.02);           // only inside the shape
                shine *= smoothstep(0.0, 0.15, -sdf); // fade near border
                col.rgb += shine * _ShineIntensity;

                // ── Alpha: shape mask × vertex color ──
                col.a *= shapeMask * i.color.a;

                // ── UI clip rect ──
                col.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);

                return col;
            }
            ENDCG
        }
    }

    FallBack "UI/Default"
}
