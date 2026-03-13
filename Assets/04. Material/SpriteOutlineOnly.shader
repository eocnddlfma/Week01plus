Shader "Custom/SpriteOutlineOnly"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _OutlineColor    ("Outline Color",          Color)             = (1, 1, 1, 1)
        _OutlineUVSize   ("Outline UV Size",        Range(0.001, 0.2)) = 0.01
        _AlphaThreshold  ("Alpha Threshold",        Range(0.01, 1))    = 0.1
    }

    SubShader
    {
        Tags
        {
            "Queue"             = "Transparent"
            "IgnoreProjector"   = "True"
            "RenderType"        = "Transparent"
            "PreviewType"       = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _OutlineColor;
                float  _OutlineUVSize;
                float  _AlphaThreshold;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color      : COLOR;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float4 color       : COLOR;
                float2 uv          : TEXCOORD0;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv          = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.color       = IN.color;
                return OUT;
            }

            // UV가 [0,1] 밖이면 알파 0 반환 → Clamp 텍스처의 경계 오판 방지
            float SampleAlphaClamped(float2 uv)
            {
                if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0)
                    return 0.0;
                return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).a;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;
                float  center = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).a;

                // 스프라이트 바깥 → 버림
                if (center < _AlphaThreshold)
                    discard;

                float d = _OutlineUVSize;

                float n0 = SampleAlphaClamped(uv + float2( 0,  d));
                float n1 = SampleAlphaClamped(uv + float2( 0, -d));
                float n2 = SampleAlphaClamped(uv + float2(-d,  0));
                float n3 = SampleAlphaClamped(uv + float2( d,  0));
                float n4 = SampleAlphaClamped(uv + float2(-d,  d));
                float n5 = SampleAlphaClamped(uv + float2( d,  d));
                float n6 = SampleAlphaClamped(uv + float2(-d, -d));
                float n7 = SampleAlphaClamped(uv + float2( d, -d));

                float minNeighbor = min(min(min(n0, n1), min(n2, n3)),
                                        min(min(n4, n5), min(n6, n7)));

                // 8방향 이웃이 모두 불투명 → 완전 내부 픽셀 → 버림(속을 비움)
                if (minNeighbor >= _AlphaThreshold)
                    discard;

                // 이웃 중 하나라도 투명 → 경계 픽셀 → 외곽선 출력
                return _OutlineColor * IN.color;
            }
            ENDHLSL
        }
    }

    FallBack "Sprites/Default"
}
