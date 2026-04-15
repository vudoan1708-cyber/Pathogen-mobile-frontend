Shader "Pathogen/UIHealthGradient"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _PulseSpeed     ("Pulse Speed",     Range(0.1, 3))   = 0.8
        _PulseIntensity ("Pulse Intensity", Range(0, 0.2))   = 0.06
        _NoiseScale     ("Noise Scale",     Range(2, 30))    = 12
        _NoiseAmount    ("Noise Amount",    Range(0, 0.08))  = 0.02
        _EdgeGlow       ("Edge Glow",       Range(0, 0.5))   = 0.15

        // ── UI stencil ──
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
            Name "UIHealthGradient"

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

            float _PulseSpeed;
            float _PulseIntensity;
            float _NoiseScale;
            float _NoiseAmount;
            float _EdgeGlow;

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
                float t = i.uv.x;   // 0 (left, death) → 1 (right, healthy)

                // ── Organic noise distortion on the gradient ──
                float2 nc = i.uv * _NoiseScale + _Time.y * 0.2;
                float noise = valueNoise(nc) * _NoiseAmount;
                t = saturate(t + noise);

                // ── 3-stop gradient: red → yellow → green ──
                fixed3 col;
                if (t < 0.33)
                {
                    float s = t / 0.33;
                    col = lerp(fixed3(0.9, 0.1, 0.1), fixed3(1.0, 0.85, 0.1), s);
                }
                else if (t < 0.66)
                {
                    float s = (t - 0.33) / 0.33;
                    col = lerp(fixed3(1.0, 0.85, 0.1), fixed3(0.5, 0.9, 0.2), s);
                }
                else
                {
                    float s = (t - 0.66) / 0.34;
                    col = lerp(fixed3(0.5, 0.9, 0.2), fixed3(0.2, 0.85, 0.2), s);
                }

                // ── Subtle organic breathing pulse ──
                float pulse = sin(_Time.y * _PulseSpeed) * 0.5 + 0.5;
                col += pulse * _PulseIntensity;

                // ── Bright edge glow at the fill boundary (right edge of the bar) ──
                float rightEdge = 1.0 - i.uv.x;
                float edgeMask = smoothstep(0.08, 0.0, rightEdge);
                col += edgeMask * _EdgeGlow;

                // ── Top/bottom edge highlight for depth ──
                float yEdge = min(i.uv.y, 1.0 - i.uv.y);
                float yGlow = smoothstep(0.15, 0.0, yEdge) * 0.12;
                col += yGlow;

                // ── Sprite alpha + vertex color ──
                fixed4 texCol = tex2D(_MainTex, i.uv);
                float alpha = texCol.a * i.color.a;

                // ── UI clip rect ──
                alpha *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);

                return fixed4(col, alpha);
            }
            ENDCG
        }
    }

    FallBack "UI/Default"
}
