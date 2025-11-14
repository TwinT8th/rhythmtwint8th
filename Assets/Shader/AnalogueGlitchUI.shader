Shader "UI/AnalogGlitchUI"
{
    Properties
    {
        [PerRendererData]_MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        // Kino와 호환되는 이름/타입
        _ScanLineJitter ("ScanLineJitter (disp, thresh)", Vector) = (0, 1, 0, 0)
        _VerticalJump   ("VerticalJump (amp, time)",     Vector) = (0, 0, 0, 0)
        _HorizontalShake("HorizontalShake",              Float)  = 0
        _ColorDrift     ("ColorDrift (amount, time)",    Vector) = (0, 0, 0, 0)

        // 마스크/클리핑(UGUI 전용)
        [HideInInspector]_ClipRect      ("Clip Rect", Vector) = (0,0,0,0)
        [HideInInspector]_UIMaskSoftnessX ("SoftnessX", Float) = 0
        [HideInInspector]_UIMaskSoftnessY ("SoftnessY", Float) = 0
        [HideInInspector]_StencilComp ("", Float) = 8
        [HideInInspector]_Stencil ("", Float) = 0
        [HideInInspector]_StencilOp ("", Float) = 0
        [HideInInspector]_StencilWriteMask ("", Float) = 255
        [HideInInspector]_StencilReadMask ("", Float) = 255
        [HideInInspector]_ColorMask ("", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "UIAnalogGlitch"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;

            float4 _ClipRect;
            float _UIMaskSoftnessX;
            float _UIMaskSoftnessY;

            // Kino와 동일한 명칭
            float2 _ScanLineJitter;   // x: disp, y: thresh
            float2 _VerticalJump;     // x: amp,  y: time
            float  _HorizontalShake;  // 0..1
            float2 _ColorDrift;       // x: amount, y: time

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex        : SV_POSITION;
                fixed4 color         : COLOR;
                float2 texcoord      : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };

            v2f vert (appdata_t v)
            {
                v2f o;
                o.worldPosition = v.vertex;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.color = v.color * _Color;
                return o;
            }

            // 간단 난수
            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 34.345);
                return frac(p.x * p.y);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 기본 UV
                float2 uv = i.texcoord;

                // --- Vertical Jump (세로 점프) ---
                // _VerticalJump.x : amplitude(0..1)
                // _VerticalJump.y : time
                float vj = sin(_VerticalJump.y) * _VerticalJump.x;
                uv.y = frac(uv.y + vj * 0.05);   // 감도 0.05

                // --- Scanline Jitter (스캔라인 끊김) ---
                // _ScanLineJitter.x : displacement
                // _ScanLineJitter.y : threshold(남아있을 확률)
                float jitterRnd = hash21(float2(uv.y * 200.0, _Time.y));
                if (jitterRnd > _ScanLineJitter.y)
                {
                    float off = (jitterRnd - 0.5) * 2.0 * _ScanLineJitter.x;
                    uv.x += off;
                }

                // --- Horizontal Shake (수평 흔들림, 프레임 고정 노이즈) ---
                float hNoise = (hash21(float2(floor(_Time.y * 60.0), 0.1234)) - 0.5);
                uv.x += hNoise * (_HorizontalShake * 0.02); // 감도 0.02

                // --- Color Drift (RGB 분리) ---
                float drift = _ColorDrift.x;
                float s = sin(_ColorDrift.y);
                float2 shift = float2(drift * s, 0);

                fixed4 col;
                fixed4 c0 = tex2D(_MainTex, uv);
                fixed  r  = tex2D(_MainTex, uv + shift).r;
                fixed  b  = tex2D(_MainTex, uv - shift).b;
                col = fixed4(r, c0.g, b, c0.a);

                // UGUI 클리핑/소프트 마스크
                #ifdef UNITY_UI_CLIP_RECT
                col.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
                #endif

                // 색상 틴트/버텍스컬러
                col *= i.color;

                return col;
            }
            ENDCG
        }
    }

    FallBack "UI/Default"
}
