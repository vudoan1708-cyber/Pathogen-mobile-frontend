Shader "Pathogen/Crosshair"
{
    Properties
    {
        _Color     ("Color",      Color)         = (0.3, 0.5, 0.9, 0.5)
        _LineWidth ("Line Width", Range(0.01, 0.08)) = 0.035
        _GapRadius ("Gap Radius", Range(0.0, 0.4))  = 0.2
        _RingWidth ("Ring Width", Range(0.01, 0.1))  = 0.04
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
            };

            fixed4 _Color;
            float _LineWidth;
            float _GapRadius;
            float _RingWidth;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv - 0.5;
                float dist = length(uv);

                // Outer circle mask — fade out at edge
                float outerMask = 1.0 - smoothstep(0.45, 0.5, dist);
                if (outerMask < 0.001) discard;

                float mask = 0.0;

                // Center ring
                float ringOuter = _GapRadius + _RingWidth;
                float ringInner = _GapRadius;
                float ringMask = smoothstep(ringInner - 0.01, ringInner, dist)
                               * (1.0 - smoothstep(ringOuter, ringOuter + 0.01, dist));
                mask = max(mask, ringMask);

                // Cross lines (outside the gap)
                float halfW = _LineWidth * 0.5;
                float hLine = step(abs(uv.y), halfW) * step(_GapRadius, abs(uv.x));
                float vLine = step(abs(uv.x), halfW) * step(_GapRadius, abs(uv.y));
                mask = max(mask, hLine);
                mask = max(mask, vLine);

                mask *= outerMask;
                if (mask < 0.001) discard;

                fixed4 col = _Color;
                col.a *= mask;
                return col;
            }
            ENDCG
        }
    }

    FallBack Off
}
