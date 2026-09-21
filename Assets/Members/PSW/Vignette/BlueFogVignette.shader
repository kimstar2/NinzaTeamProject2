Shader "PSW/UI/Blue Fog Vignette"
{
    Properties
    {
        [PerRendererData] _MainTex ("UI Texture", 2D) = "white" {}
        _Color ("Deep Blue", Color) = (0.018, 0.075, 0.16, 1)
        _FogColor ("Fog Blue", Color) = (0.035, 0.16, 0.28, 1)
        _Opacity ("Opacity", Range(0, 1)) = 0.9
        _Width ("Border Width", Range(0.02, 0.6)) = 0.25
        _Softness ("Inner Edge Softness", Range(0.01, 0.4)) = 0.22
        _Irregularity ("Fog Irregularity", Range(0, 0.2)) = 0.09
        _NoiseScale ("Fog Scale", Range(1, 12)) = 4
        _Speed ("Fog Speed", Range(0, 1)) = 0.08
        _LeftBias ("Extra Left Border", Range(0, 0.2)) = 0.06
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="False" }
        Stencil
        {
            Ref [_Stencil]
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
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct Attributes
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct Varyings
            {
                float4 vertex : SV_POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
                float4 localPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            float4 _Color, _FogColor, _ClipRect;
            float _Opacity, _Width, _Softness, _Irregularity;
            float _NoiseScale, _Speed, _LeftBias;

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.localPosition = input.vertex;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.uv = input.uv;
                output.color = input.color;
                return output;
            }

            float Hash(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * 0.1031);
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }

            float Noise(float2 p)
            {
                float2 cell = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(Hash(cell), Hash(cell + float2(1, 0)), f.x),
                    lerp(Hash(cell + float2(0, 1)), Hash(cell + float2(1, 1)), f.x), f.y);
            }

            float FogNoise(float2 p)
            {
                return Noise(p) * 0.57 + Noise(p * 2.03 + 13.7) * 0.29
                    + Noise(p * 4.11 + 31.3) * 0.14;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 uv = input.uv;
                float2 p = abs(uv * 2.0 - 1.0);
                // A rounded rectangle follows the screen edges, including wide screens.
                float2 p4 = p * p * p * p;
                float edgeDistance = 1.0 - pow(p4.x + p4.y, 0.25);
                float aspect = _ScreenParams.x / max(_ScreenParams.y, 1.0);
                float2 noiseUV = uv * float2(aspect, 1.0) * _NoiseScale;
                float time = _Time.y * _Speed;
                float fog = FogNoise(noiseUV + float2(time * 0.37, -time));
                float detail = Noise(noiseUV * 1.73 + float2(-time, time * 0.23) + 7.1);
                float width = _Width + _LeftBias * (1.0 - uv.x) * (1.0 - uv.x);
                float irregularDistance = edgeDistance + (fog - 0.5) * 2.0 * _Irregularity;
                float mask = 1.0 - smoothstep(width - _Softness, width + _Softness, irregularDistance);
                // Keep the outer border dense; let only the inward-facing fog vary.
                float innerEdge = smoothstep(0.0, width + _Softness, edgeDistance);
                float density = lerp(1.0, 0.65 + fog * 0.35, innerEdge);
                float fogTint = innerEdge * (0.25 + 0.55 * detail);
                half4 result = lerp(_Color, _FogColor, fogTint);
                result.rgb *= input.color.rgb;
                result.a *= mask * density * _Opacity * input.color.a;
                #ifdef UNITY_UI_CLIP_RECT
                result.a *= UnityGet2DClipping(input.localPosition.xy, _ClipRect);
                #endif
                #ifdef UNITY_UI_ALPHACLIP
                clip(result.a - 0.001);
                #endif
                return result;
            }
            ENDCG
        }
    }
    FallBack Off
}
