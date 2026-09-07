Shader "Custom/UI/ScrollingDiagonalStripesMasked"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _StripeColor1 ("Stripe Color 1", Color) = (1,1,1,1)
        _StripeColor2 ("Stripe Color 2", Color) = (0,0,0,0)

        _StripeCount ("Stripe Count", Float) = 10
        _StripeRatio ("Stripe Ratio", Range(0.05, 0.95)) = 0.5
        _ScrollSpeed ("Scroll Speed", Float) = 0.3

        _DirectionX ("Direction X", Float) = 1
        _DirectionY ("Direction Y", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
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

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 uv       : TEXCOORD0;
            };

            sampler2D _MainTex;
            fixed4 _Color;

            fixed4 _StripeColor1;
            fixed4 _StripeColor2;

            float _StripeCount;
            float _StripeRatio;
            float _ScrollSpeed;
            float _DirectionX;
            float _DirectionY;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // 원본 UI Image(스프라이트)의 알파를 읽음
                fixed4 baseTex = tex2D(_MainTex, i.uv);

                // 사선 패턴 계산
                float stripeUV = i.uv.x * _DirectionX + i.uv.y * _DirectionY;
                stripeUV += _Time.y * _ScrollSpeed;

                float stripe = frac(stripeUV * _StripeCount);

                fixed4 stripeColor = (stripe < _StripeRatio) ? _StripeColor1 : _StripeColor2;

                // Image의 원본 알파로 마스킹
                stripeColor.a *= baseTex.a;

                // UI Image의 color tint 반영
                stripeColor *= i.color;

                return stripeColor;
            }
            ENDCG
        }
    }
}