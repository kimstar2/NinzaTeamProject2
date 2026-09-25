Shader "Ninza/Effects/Dice Border Glow"
{
    Properties
    {
        [HDR] _Color("Border Color", Color) = (1, 0.1, 0.05, 1)
        _EdgeWidth("Border Width", Range(0.01, 0.5)) = 0.14
        _EdgeSoftness("Border Softness", Range(0.001, 0.1)) = 0.015
        _Intensity("Border Intensity", Range(0, 10)) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent+20"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "DiceBorderGlow"
            Tags { "LightMode" = "UniversalForwardOnly" }

            Blend SrcAlpha One
            Cull Back
            ZWrite Off
            ZTest LEqual

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionOS : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half _EdgeWidth;
                half _EdgeSoftness;
                half _Intensity;
            CBUFFER_END

            Varyings Vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = positionInputs.positionCS;
                output.positionOS = input.positionOS.xyz;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                // A cube surface always has one object-space coordinate at its
                // maximum extent. The second-largest coordinate approaches the
                // maximum only near an edge, so it provides a stable border mask
                // even when the camera looks straight at a flat face.
                half3 position = abs(input.positionOS);
                half largest = max(position.x, max(position.y, position.z));
                half smallest = min(position.x, min(position.y, position.z));
                half middle = position.x + position.y + position.z - largest - smallest;
                half edgeCoordinate = middle / max(largest, 0.0001h);

                half edgeStart = 1.0h - _EdgeWidth;
                half edge = smoothstep(
                    edgeStart - _EdgeSoftness,
                    edgeStart + _EdgeSoftness,
                    edgeCoordinate);
                half3 borderColor = _Color.rgb * _Intensity;

                return half4(borderColor, edge * _Color.a);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
