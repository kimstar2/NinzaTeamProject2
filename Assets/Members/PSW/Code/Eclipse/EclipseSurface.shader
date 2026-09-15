Shader "PSW/Eclipse/Surface"
{
    Properties
    {
        _Tint ("Tint", Color) = (1,1,1,1)
        _Kind ("0 Sun / 1 Moon / 2 Eclipse / 3 Corona / 4 Ring / 5 Atmosphere / 6 Shard / 7 Crack / 8 Particle", Float) = 0
        _Opacity ("Opacity", Range(0,1)) = 1
        _Phase ("Timeline", Float) = 0
        _Reveal ("Crack reveal", Range(0,1)) = 1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; float4 color : COLOR; };
            struct v2f { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; float4 color : COLOR; };
            float4 _Tint;
            float _Kind, _Opacity, _Phase, _Reveal;
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }
            float hash(float2 p) { return frac(sin(dot(p, float2(127.1,311.7))) * 43758.5453); }
            float noise(float2 p)
            {
                float2 i = floor(p), f = frac(p);
                f = f*f*(3-2*f);
                return lerp(lerp(hash(i),hash(i+float2(1,0)),f.x),
                    lerp(hash(i+float2(0,1)),hash(i+1),f.x),f.y);
            }
            float fbm(float2 p) { return noise(p)*0.55 + noise(p*2.07)*0.28 + noise(p*4.13)*0.17; }
            float4 frag(v2f i) : SV_Target
            {
                float2 p = (i.uv - 0.5)*2;
                float r = length(p), a = atan2(p.y,p.x);
                float alpha = 0;
                float3 color = _Tint.rgb;
                if (_Kind < 0.5)
                {
                    float body = 1-smoothstep(0.674,0.686,r);
                    float grain = fbm(p*9 + float2(_Phase*0.16,0));
                    color = lerp(float3(0.9,0.23,0.065),float3(1,0.91,0.57),saturate(grain*1.2 + 0.3*(1-r)));
                    float edge = exp(-abs(r-0.682)*125);
                    float rays = pow(saturate(sin(a*29 + fbm(p*4)*7 + _Phase)*0.5+0.5),4);
                    float halo = exp(-max(0,r-0.69)*18)*(0.28 + rays*0.5);
                    color = lerp(color,float3(1,0.79,0.39),saturate(edge + (1-body)));
                    alpha = max(body,halo) * (1-smoothstep(0.9,1,r));
                }
                else if (_Kind < 1.5)
                {
                    float body = 1-smoothstep(0.66,0.672,r);
                    float terrain = fbm(p*12);
                    float light = saturate(0.48-p.x*0.68+p.y*0.32);
                    color = lerp(float3(0.08,0.085,0.16),float3(0.73,0.80,0.94),terrain*light);
                    float rim = exp(-abs(r-0.665)*135);
                    color += rim*float3(0.68,0.73,0.91);
                    alpha = max(body,exp(-abs(r-0.67)*29)*0.43);
                }
                else if (_Kind < 2.5)
                {
                    color = float3(0.012,0.009,0.026) + fbm(p*7)*0.015;
                    alpha = 1-smoothstep(0.672,0.679,r);
                }
                else if (_Kind < 3.5)
                {
                    float ring = exp(-abs(r-0.685)*160);
                    float tendril = pow(saturate(sin(a*19 + fbm(p*6)*8-_Phase*1.5)*0.5+0.5),3);
                    float wisps = exp(-abs(r-0.7)*19)*(0.22+tendril*0.45);
                    color = lerp(float3(0.38,0.23,0.67),float3(1,0.92,0.8),saturate(ring + tendril*0.3));
                    alpha = (ring+wisps)*(1-smoothstep(0.92,1,r))*smoothstep(0.66,0.68,r);
                }
                else if (_Kind < 4.5)
                {
                    alpha = exp(-abs(r-0.7)*100);
                }
                else if (_Kind < 5.5)
                {
                    float cloud = fbm(p*4 + float2(_Phase*0.05,0));
                    color = lerp(float3(0.012,0.01,0.034),float3(0.07,0.04,0.13),cloud);
                    alpha = (1-smoothstep(0.45,1.2,r))*0.92;
                }
                else if (_Kind < 6.5)
                {
                    color *= i.color.rgb;
                    alpha = i.color.a;
                }
                else if (_Kind < 7.5)
                {
                    alpha = (1-smoothstep(_Reveal-0.07,_Reveal,i.uv.x)) * (0.5+0.5*sin(i.uv.y*3.14159));
                }
                else
                {
                    alpha = pow(saturate(1-r),2)*i.color.a;
                    color *= i.color.rgb;
                }
                return float4(color, saturate(alpha)*_Opacity*_Tint.a);
            }
            ENDCG
        }
    }
}
