// Slash distance masks and cyan/white glow inspired by Frontier's
// ScreenSplitSlashParticle.shader. See FrontierReference.md and Frontier-LICENSE.txt.
Shader "PSW/VFX/Instinct Energy"
{
    Properties
    {
        [HDR] _CoreColor ("Core", Color) = (2, 2.3, 2.5, 1)
        [HDR] _GlowColor ("Edge", Color) = (0.12, 0.65, 1.4, 1)
        _Style ("0 Slash / 1 Ring / 2 Spark", Float) = 0
        _Width ("Width", Range(0.01, 0.4)) = 0.08
        _Curve ("Blade Curve", Range(-1, 1)) = 0.3
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Tags { "LightMode"="Universal2D" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
                half4 _CoreColor, _GlowColor;
                float _Style, _Width, _Curve;
            CBUFFER_END
            struct Attributes { float4 vertex:POSITION; float2 uv:TEXCOORD0; half4 color:COLOR; };
            struct Varyings { float4 vertex:SV_POSITION; float2 uv:TEXCOORD0; half4 color:COLOR; };
            Varyings Vert(Attributes v)
            {
                Varyings o;
                o.vertex=TransformObjectToHClip(v.vertex.xyz); o.uv=v.uv; o.color=v.color;
                return o;
            }
            half4 Frag(Varyings i):SV_Target
            {
                float2 p=i.uv*2-1;
                float taper=saturate(1-p.x*p.x);
                float distanceToEdge=abs(p.y-_Curve*taper);
                float width=_Width*taper;
                float lengthMask=1-smoothstep(.8,1,abs(p.x));
                if (_Style > .5 && _Style < 1.5)
                {
                    distanceToEdge=abs(length(p)-.72); width=_Width; lengthMask=1;
                }
                if (_Style > 1.5)
                {
                    distanceToEdge=abs(p.y); width=_Width*taper;
                }
                float aa=max(fwidth(distanceToEdge),.003);
                float core=1-smoothstep(width*.25,width*.65+aa,distanceToEdge);
                float edge=1-smoothstep(width,width*2.8+aa,distanceToEdge);
                half3 color=lerp(_GlowColor.rgb,_CoreColor.rgb,core);
                return half4(color*i.color.rgb,edge*lengthMask*i.color.a);
            }
            ENDHLSL
        }
    }
}
