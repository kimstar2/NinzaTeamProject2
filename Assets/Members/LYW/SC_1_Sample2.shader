Shader "Custom/PixelFlameAura"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [HDR] _CoreColor ("Core Color", Color) = (3.0, 2.2, 0.8, 1)
        [HDR] _MidColor  ("Mid Color",  Color) = (2.4, 0.8, 0.15, 1)
        [HDR] _EdgeColor ("Edge Color", Color) = (1.2, 0.1, 0.0, 1)

        _AuraSize ("Aura Size (Pixel)", Range(1, 24)) = 6
        _AuraIntensity ("Aura Intensity", Range(0, 4)) = 1.4
        _EdgeSoftness ("Edge Softness", Range(0.01, 1)) = 0.25

        _NoiseScale ("Noise Scale", Range(1, 40)) = 10
        _FlowSpeed ("Flow Speed", Range(0, 10)) = 2.2
        _Distortion ("Distortion", Range(0, 1)) = 0.25

        _FlickerSpeed ("Flicker Speed", Range(0, 20)) = 8
        _FlickerStrength ("Flicker Strength", Range(0, 1)) = 0.15

        _EmberDensity ("Ember Density", Range(0, 1)) = 0.22
        _EmberSpeed ("Ember Speed", Range(0, 10)) = 2.0
        _EmberSize ("Ember Size", Range(0.01, 0.3)) = 0.08
        [HDR] _EmberColor ("Ember Color", Color) = (3.5, 1.2, 0.2, 1)
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
            #pragma target 3.0
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                float4 color  : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv     : TEXCOORD0;
                float4 color  : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;

            float4 _Color;

            float4 _CoreColor;
            float4 _MidColor;
            float4 _EdgeColor;

            float _AuraSize;
            float _AuraIntensity;
            float _EdgeSoftness;

            float _NoiseScale;
            float _FlowSpeed;
            float _Distortion;

            float _FlickerSpeed;
            float _FlickerStrength;

            float _EmberDensity;
            float _EmberSpeed;
            float _EmberSize;
            float4 _EmberColor;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color * _Color;
                return o;
            }

            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 345.45));
                p += dot(p, p + 34.345);
                return frac(p.x * p.y);
            }

            float noise21(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);

                float a = hash21(i);
                float b = hash21(i + float2(1, 0));
                float c = hash21(i + float2(0, 1));
                float d = hash21(i + float2(1, 1));

                float2 u = f * f * (3.0 - 2.0 * f);

                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            float fbm(float2 p)
            {
                float v = 0.0;
                float a = 0.5;

                v += noise21(p) * a; p *= 2.02; a *= 0.5;
                v += noise21(p) * a; p *= 2.03; a *= 0.5;
                v += noise21(p) * a; p *= 2.01; a *= 0.5;
                v += noise21(p) * a;

                return v;
            }

            float SampleAlpha(float2 uv)
            {
                return tex2D(_MainTex, uv).a;
            }

            float SampleRing(float2 uv, float radius)
            {
                float2 px = _MainTex_TexelSize.xy * radius;
                float a = 0.0;

                a = max(a, SampleAlpha(uv + float2( px.x, 0)));
                a = max(a, SampleAlpha(uv + float2(-px.x, 0)));
                a = max(a, SampleAlpha(uv + float2(0,  px.y)));
                a = max(a, SampleAlpha(uv + float2(0, -px.y)));

                float2 d = px * 0.7071;
                a = max(a, SampleAlpha(uv + float2( d.x,  d.y)));
                a = max(a, SampleAlpha(uv + float2(-d.x,  d.y)));
                a = max(a, SampleAlpha(uv + float2( d.x, -d.y)));
                a = max(a, SampleAlpha(uv + float2(-d.x, -d.y)));

                a = max(a, SampleAlpha(uv + float2(px.x * 0.5, px.y)));
                a = max(a, SampleAlpha(uv + float2(-px.x * 0.5, px.y)));
                a = max(a, SampleAlpha(uv + float2(px.x * 0.5, -px.y)));
                a = max(a, SampleAlpha(uv + float2(-px.x * 0.5, -px.y)));

                return a;
            }

            float emberMask(float2 uv, float auraMask, float originalAlpha, float time)
            {
                float2 p = uv;

                float2 gridUV = float2(p.x * 18.0, p.y * 26.0 - time * _EmberSpeed * 2.5);
                float2 cell = floor(gridUV);
                float2 local = frac(gridUV) - 0.5;

                float rnd = hash21(cell);
                float appear = step(1.0 - _EmberDensity, rnd);

                float2 offset = float2(
                    hash21(cell + 1.73),
                    hash21(cell + 8.11)
                ) - 0.5;

                float dist = length(local - offset * 0.6);
                float ember = smoothstep(_EmberSize, 0.0, dist);

                float life = frac(rnd + time * 0.65 + hash21(cell + 3.17));
                float fade = smoothstep(0.0, 0.15, life) * (1.0 - smoothstep(0.55, 1.0, life));

                ember *= appear;
                ember *= fade;
                ember *= auraMask;
                ember *= (1.0 - originalAlpha);

                return ember;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float4 sprite = tex2D(_MainTex, i.uv) * i.color;
                float originalAlpha = sprite.a;

                float time = _Time.y;
                float t = time * _FlowSpeed;

                // 기본 외곽 오라 범위
                float inner = SampleRing(i.uv, _AuraSize * 0.55);
                float mid   = SampleRing(i.uv, _AuraSize * 1.15);
                float outer = SampleRing(i.uv, _AuraSize * 1.85);

                float baseAura = max(inner, max(mid * 0.75, outer * 0.45));
                baseAura *= (1.0 - originalAlpha);

                // 물결 대신 랜덤 노이즈 + 위로 흐르는 불 형태
                float2 flameUV = i.uv;

                // 좌우로 살짝 흔들리는 불기둥 느낌
                float sideDrift =
                    (fbm(float2(flameUV.y * 6.0, -t * 0.7)) - 0.5) * _Distortion;

                flameUV.x += sideDrift;

                float2 n1UV = float2(flameUV.x * _NoiseScale, flameUV.y * (_NoiseScale * 1.35) - t * 2.3);
                float2 n2UV = float2(flameUV.x * (_NoiseScale * 1.9) + 7.2, flameUV.y * (_NoiseScale * 1.8) - t * 3.0);

                float n1 = fbm(n1UV);
                float n2 = fbm(n2UV);
                float flameNoise = n1 * 0.65 + n2 * 0.35;

                // 위로 갈수록 길쭉하게 타오르는 느낌
                float upwardBias = pow(saturate(i.uv.y), 1.4);
                float erosion = flameNoise * lerp(0.55, 0.95, upwardBias);

                // baseAura를 노이즈로 깎아내서 불꽃처럼 테두리 형성
                float flame = smoothstep(0.18, 0.18 + _EdgeSoftness, baseAura - erosion * 0.55);

                // 미세 깜빡임
                float flicker =
                    1.0 +
                    (noise21(float2(time * _FlickerSpeed, i.uv.y * 13.0)) - 0.5) * 2.0 * _FlickerStrength +
                    sin(time * (_FlickerSpeed * 0.7) + i.uv.x * 21.0) * (_FlickerStrength * 0.35);

                flame *= flicker;
                flame = saturate(flame) * _AuraIntensity;

                // 코어/중간/가장자리 색 혼합
                float coreMask = saturate(inner * 1.25 - flameNoise * 0.18) * (1.0 - originalAlpha);
                float edgeMask = saturate(1.0 - baseAura);

                float3 auraColor = lerp(_EdgeColor.rgb, _MidColor.rgb, saturate(baseAura * 1.1));
                auraColor = lerp(auraColor, _CoreColor.rgb, saturate(coreMask));

                // 불똥
                float embers = emberMask(i.uv, saturate(outer * 1.2), originalAlpha, time);
                float3 emberColor = _EmberColor.rgb * embers * 1.5;

                // 최종 합성
                float auraAlpha = saturate(flame * 0.9 + embers * 0.8);

                float3 finalColor = sprite.rgb;
                finalColor = lerp(finalColor, auraColor, auraAlpha * (1.0 - originalAlpha));
                finalColor += emberColor;

                float finalAlpha = saturate(originalAlpha + auraAlpha * (1.0 - originalAlpha));

                return float4(finalColor, finalAlpha);
            }
            ENDCG
        }
    }
}