Shader "Pathogen/UIUpgradeButton"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _FillColorTop    ("Fill Top",    Color) = (0.2, 0.4, 0.9, 0.25)
        _FillColorBottom ("Fill Bottom", Color) = (1, 1, 1, 0)
        _BorderColorTop  ("Border Top",  Color) = (1, 1, 1, 0.9)
        _BorderColorBot  ("Border Bot",  Color) = (0.3, 0.5, 1, 0.4)
        _BorderWidth     ("Border Width",    Range(0.01, 0.15)) = 0.06
        _WhiteTipSize    ("White Tip Size",  Range(0.01, 0.3))  = 0.12
        _WaveSpeed       ("Wave Speed",      Range(0.5, 5))     = 1.0
        _WaveWidth       ("Wave Width",      Range(0.05, 0.4))  = 0.15
        _WaveBrightness  ("Wave Brightness", Range(0, 1))       = 0.5

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

            fixed4 _FillColorTop;
            fixed4 _FillColorBottom;
            fixed4 _BorderColorTop;
            fixed4 _BorderColorBot;
            float _BorderWidth;
            float _WhiteTipSize;
            float _WaveSpeed;
            float _WaveWidth;
            float _WaveBrightness;

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
                float2 center = float2(0.5, 0.5);
                float dist = length(i.uv - center) * 2.0;

                float circleMask = 1.0 - smoothstep(0.92, 1.0, dist);
                if (circleMask < 0.001) discard;

                float t = i.uv.y;

                fixed4 fillColor = lerp(_FillColorBottom, _FillColorTop, t);

                float innerEdge = 1.0 - _BorderWidth * 2.0;
                float borderMask = smoothstep(innerEdge - 0.04, innerEdge, dist);
                borderMask *= circleMask;

                float topTip = smoothstep(1.0 - _WhiteTipSize, 1.0, t);
                float botTip = smoothstep(_WhiteTipSize, 0.0, t);
                float tipBlend = max(topTip, botTip);
                fixed4 borderColor = lerp(_BorderColorBot, _BorderColorTop, tipBlend);

                // Wave: bright band sweeping bottom to top along the border
                float wavePos = frac(_Time.y * _WaveSpeed);
                float waveDist = abs(t - wavePos);
                waveDist = min(waveDist, 1.0 - waveDist);
                float waveMask = (1.0 - smoothstep(0.0, _WaveWidth, waveDist)) * borderMask;

                // Chevron arrow — open, no fill, no baseline
                float2 uv = i.uv - 0.5;
                float chevronY = uv.y - 0.13;
                float leftDist = abs(chevronY - uv.x * 1.35);
                float rightDist = abs(chevronY + uv.x * 1.35);
                float leftArm = step(uv.x, 0.0) * (1.0 - smoothstep(0.0, 0.04, leftDist));
                float rightArm = step(0.0, uv.x) * (1.0 - smoothstep(0.0, 0.04, rightDist));
                float chevronMask = (leftArm + rightArm)
                    * step(-0.28, uv.y) * step(uv.y, 0.28)
                    * step(-0.2, uv.x) * step(uv.x, 0.2);

                fixed4 col = fillColor;
                col = lerp(col, borderColor, borderMask);
                col.rgb += waveMask * _WaveBrightness;
                col.a += waveMask * _WaveBrightness * 0.5;
                col.rgb = lerp(col.rgb, float3(1, 1, 1), chevronMask * 0.95);
                col.a = max(col.a, chevronMask * 0.95);
                col.a *= circleMask;

                col *= i.color;
                col.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);

                return col;
            }
            ENDCG
        }
    }

    FallBack "UI/Default"
}
