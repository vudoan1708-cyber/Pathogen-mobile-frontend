Shader "Pathogen/UIEntityBarCanvas"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _FillPct       ("Fill %",           Range(0, 1)) = 1
        _TrailPct      ("Trail %",          Range(0, 1)) = 1
        _TrailSign     ("Trail Sign",       Float)       = 0
        _TrailAlpha    ("Trail Alpha",      Range(0, 1)) = 0

        _FillColor     ("Fill Color",       Color) = (0.2, 0.7, 0.3, 1)
        _DamageColor   ("Damage Trail",     Color) = (0.9, 0.15, 0.1, 0.7)
        _HealColor     ("Heal Trail",       Color) = (0.5, 1, 0.55, 0.7)
        _BGColor       ("Background",       Color) = (0.12, 0.12, 0.15, 0.85)
        _TickColor     ("Tick Color",       Color) = (0.0, 0.0, 0.0, 0.6)

        _TickSpacing   ("Tick Spacing UV",  Range(0, 0.5))   = 0.1
        _TickWidth     ("Tick Width",       Range(0.001, 0.025)) = 0.009

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

            float _FillPct;
            float _TrailPct;
            float _TrailSign;
            float _TrailAlpha;
            fixed4 _FillColor;
            fixed4 _DamageColor;
            fixed4 _HealColor;
            fixed4 _BGColor;
            fixed4 _TickColor;
            float _TickSpacing;
            float _TickWidth;

            v2f vert(appdata v)
            {
                v2f o;
                o.worldPosition = v.vertex;
                o.pos   = UnityObjectToClipPos(v.vertex);
                o.uv    = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float x = i.uv.x;

                // Background
                fixed4 col = _BGColor;

                // Fill
                float fillMask = step(x, _FillPct);
                col.rgb = lerp(col.rgb, _FillColor.rgb, fillMask);
                col.a = lerp(col.a, _FillColor.a, fillMask);

                // Damage / heal trail
                if (_TrailAlpha > 0.001)
                {
                    float lo = min(_FillPct, _TrailPct);
                    float hi = max(_FillPct, _TrailPct);
                    float trailMask = step(lo, x) * step(x, hi);

                    fixed4 trailCol = _TrailSign < 0 ? _DamageColor : _HealColor;
                    float ta = trailCol.a * _TrailAlpha;
                    col.rgb = lerp(col.rgb, trailCol.rgb, trailMask * ta);
                    col.a = max(col.a, trailMask * ta);
                }

                // 100 HP tick marks
                if (_TickSpacing > 0.001)
                {
                    float tickX = frac(x / _TickSpacing) * _TickSpacing;
                    float tickDist = min(tickX, _TickSpacing - tickX);
                    float tickMask = (1.0 - smoothstep(0.0, _TickWidth, tickDist));
                    tickMask *= step(x, max(_FillPct, _TrailPct));
                    col.rgb = lerp(col.rgb, _TickColor.rgb, tickMask * _TickColor.a);
                }

                // Sprite alpha + vertex color
                fixed4 texCol = tex2D(_MainTex, i.uv);
                col.a *= texCol.a * i.color.a;

                // UI clip rect
                col.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);

                return col;
            }
            ENDCG
        }
    }

    FallBack "UI/Default"
}
