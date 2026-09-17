Shader "Custom/PixelWavyAura"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [HDR] _GlowTop ("Glow Top", Color) = (0.05, 0.35, 1.5, 1)
        [HDR] _GlowBottom ("Glow Bottom", Color) = (1.0, 0.05, 1.0, 1)

        _GlowSize ("Glow Size (Pixel)", Range(1, 20)) = 5
        _GlowIntensity ("Glow Intensity", Range(0, 3)) = 1.2

        _WaveStrength ("Wave Strength", Range(0, 1)) = 0.35
        _WaveSpeed ("Wave Speed", Range(0, 10)) = 2
        _WaveFrequency ("Wave Frequency", Range(1, 40)) = 14

        _PulseStrength ("Pulse Strength", Range(0, 1)) = 0.15
        _PulseSpeed ("Pulse Speed", Range(0, 10)) = 2
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;

            float4 _Color;

            float4 _GlowTop;
            float4 _GlowBottom;

            float _GlowSize;
            float _GlowIntensity;

            float _WaveStrength;
            float _WaveSpeed;
            float _WaveFrequency;

            float _PulseStrength;
            float _PulseSpeed;


            v2f vert(appdata v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color * _Color;

                return o;
            }


            float SampleAlpha(float2 uv)
            {
                return tex2D(_MainTex, uv).a;
            }


            // 8방향으로 주변 알파값 검사
            float SampleRing(float2 uv, float radius)
            {
                float2 px = _MainTex_TexelSize.xy * radius;

                float a = 0;

                a = max(a, SampleAlpha(uv + float2( px.x, 0)));
                a = max(a, SampleAlpha(uv + float2(-px.x, 0)));

                a = max(a, SampleAlpha(uv + float2(0,  px.y)));
                a = max(a, SampleAlpha(uv + float2(0, -px.y)));

                float2 diagonal = px * 0.7071;

                a = max(a, SampleAlpha(uv + float2( diagonal.x,  diagonal.y)));
                a = max(a, SampleAlpha(uv + float2(-diagonal.x,  diagonal.y)));
                a = max(a, SampleAlpha(uv + float2( diagonal.x, -diagonal.y)));
                a = max(a, SampleAlpha(uv + float2(-diagonal.x, -diagonal.y)));

                return a;
            }


            fixed4 frag(v2f i) : SV_Target
            {
                float4 sprite = tex2D(_MainTex, i.uv) * i.color;

                float originalAlpha = sprite.a;

                float time = _Time.y * _WaveSpeed;

                // 서로 다른 방향의 파동을 섞음
                float wave1 =
                    sin(i.uv.y * _WaveFrequency + time);

                float wave2 =
                    sin(i.uv.x * (_WaveFrequency * 0.73) - time * 1.31);

                float wave3 =
                    sin(
                        (i.uv.x + i.uv.y) *
                        (_WaveFrequency * 0.48)
                        + time * 0.77
                    );

                float wave =
                    (wave1 + wave2 * 0.5 + wave3 * 0.35)
                    / 1.85;

                // 오라 크기 자체가 일렁거림
                float animatedRadius =
                    _GlowSize *
                    (1.0 + wave * _WaveStrength);


                // 여러 겹으로 알파 확장
                float inner =
                    SampleRing(i.uv, animatedRadius * 0.6);

                float middle =
                    SampleRing(i.uv, animatedRadius * 1.2);

                float outer =
                    SampleRing(i.uv, animatedRadius * 2.0);


                // 가까울수록 강하고 멀수록 약하게
                float glow =
                    max(
                        inner,
                        max(
                            middle * 0.75,
                            outer * 0.4
                        )
                    );

                // 캐릭터 내부에는 오라를 그리지 않음
                glow *= (1.0 - originalAlpha);


                // 전체 밝기가 아주 살짝 숨쉬듯 움직임
                float pulse =
                    1.0 +
                    sin(_Time.y * _PulseSpeed) *
                    _PulseStrength;

                glow *= _GlowIntensity * pulse;

                glow = saturate(glow);


                // 위쪽 파랑 → 아래쪽 보라
                float gradient = saturate(1.0 - i.uv.y);

                // 파동에 따라 그라데이션도 살짝 흔들림
                gradient += wave * 0.08;

                gradient = saturate(gradient);

                float3 glowColor =
                    lerp(
                        _GlowTop.rgb,
                        _GlowBottom.rgb,
                        gradient
                    );


                // 캐릭터 부분은 원본 유지
                float3 finalColor =
                    lerp(
                        glowColor,
                        sprite.rgb,
                        originalAlpha
                    );

                float finalAlpha =
                    saturate(
                        originalAlpha +
                        glow * (1.0 - originalAlpha)
                    );


                return float4(finalColor, finalAlpha);
            }

            ENDCG
        }
    }
}