Shader "Pathogen/UISkillCooldown"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Progress    ("Cooldown Progress", Range(0, 1)) = 0.5
        _TintColor   ("Tint Color", Color) = (0, 0, 0, 0.55)
        _EdgeColor   ("Sweep Edge Color", Color) = (0.4, 0.7, 1, 0.6)
        _EdgeWidth   ("Sweep Edge Width", Range(0.005, 0.08)) = 0.025

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
            float _Progress;
            fixed4 _TintColor;
            fixed4 _EdgeColor;
            float _EdgeWidth;

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
                float2 centred = i.uv - 0.5;

                // Angle from top (12 o'clock), clockwise, 0→1
                float angle = atan2(centred.x, centred.y); // top = 0
                float normalized = angle / (3.14159265 * 2.0) + 0.5; // 0→1 clockwise from top

                // Sprite shape mask
                fixed4 texCol = tex2D(_MainTex, i.uv);
                float shapeMask = texCol.a * i.color.a;

                // Cooldown region: dark tint where angle > progress (uncooled portion)
                float sweepAngle = 1.0 - _Progress; // 1 = full cooldown, 0 = ready
                float inCooldown = step(normalized, sweepAngle);

                // Bright edge along the sweep boundary
                float edgeDist = abs(normalized - sweepAngle);
                edgeDist = min(edgeDist, 1.0 - edgeDist); // wrap-around
                float edgeMask = (1.0 - smoothstep(0.0, _EdgeWidth, edgeDist)) * step(0.01, _Progress) * step(_Progress, 0.99);

                fixed4 col = _TintColor;
                col.a *= inCooldown * shapeMask;

                // Add sweep edge glow
                col.rgb = lerp(col.rgb, _EdgeColor.rgb, edgeMask);
                col.a = max(col.a, edgeMask * _EdgeColor.a * shapeMask);

                col.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);

                return col;
            }
            ENDCG
        }
    }

    FallBack "UI/Default"
}
