Shader "Ninza/Effects/Dice Border Glow"
{
    Properties
    {
        [HDR] _Color("Border Color", Color) = (1, 0.1, 0.05, 1)
        [Toggle] _UseFaceColors("Use Dice Grade Colors", Float) = 0
        [HDR] _FrontColor("Front Color", Color) = (1,1,1,1)
        [HDR] _BackColor("Back Color", Color) = (1,1,1,1)
        [HDR] _LeftColor("Left Color", Color) = (1,1,1,1)
        [HDR] _RightColor("Right Color", Color) = (1,1,1,1)
        [HDR] _TopColor("Top Color", Color) = (1,1,1,1)
        [HDR] _BottomColor("Bottom Color", Color) = (1,1,1,1)
        _Expansion("Border Expansion", Range(0, 0.1)) = 0.04
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
            // SRPDefaultUnlit is drawn by both the URP 2D and Forward renderers.
            Tags { "LightMode" = "SRPDefaultUnlit" }

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
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionOS : TEXCOORD0;
                half3 normalOS : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half4 _FrontColor, _BackColor, _LeftColor, _RightColor, _TopColor, _BottomColor;
                half _UseFaceColors;
                half _Expansion;
                half _EdgeWidth;
                half _EdgeSoftness;
                half _Intensity;
            CBUFFER_END

            Varyings Vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz * (1.0 + _Expansion));
                output.positionCS = positionInputs.positionCS;
                output.positionOS = input.positionOS.xyz;
                output.normalOS = input.normalOS;
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
                half4 color = _Color;
                if (_UseFaceColors > 0.5h)
                {
                    // Cube normals keep each grade color attached to its face while rolling.
                    if (abs(input.normalOS.x) > 0.5h)
                        color = input.normalOS.x > 0 ? _RightColor : _LeftColor;
                    else if (abs(input.normalOS.y) > 0.5h)
                        color = input.normalOS.y > 0 ? _TopColor : _BottomColor;
                    else
                        color = input.normalOS.z > 0 ? _BackColor : _FrontColor;
                }
                half3 borderColor = color.rgb * _Intensity;

                return half4(borderColor, edge * color.a);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
