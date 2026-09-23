Shader "KJY/Selection Chain"
{
    Properties
    {
        _FlowSpeed ("Flow Speed", Range(0, 2)) = 0.45
        _FlowStrength ("Flow Strength", Range(0, 1)) = 0.3
        _Softness ("Edge Softness", Range(0.01, 0.9)) = 0.45
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        Pass
        {
            Tags { "LightMode"="Universal2D" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
                float _FlowSpeed;
                float _FlowStrength;
                float _Softness;
            CBUFFER_END
            struct Attributes
            {
                float3 positionOS : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
            };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS);
                output.color = input.color;
                output.uv = input.uv;
                return output;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                // 선의 그라데이션 그대로 받고 빛만 플레이어 -> 적 방향으로 흘림
                float edge = 1.0 - smoothstep(1.0 - _Softness, 1.0, abs(input.uv.y * 2.0 - 1.0));
                float flow = pow(saturate(0.5 + 0.5 * cos((input.uv.x - _Time.y * _FlowSpeed) * 6.283185)), 12.0);
                return half4(input.color.rgb + flow * _FlowStrength, input.color.a * edge);
            }
            ENDHLSL
        }
    }
}
