Shader "Custom/ArrowPath"
{
    Properties
    {
        // 箭头贴图
        _MainTex ("Arrow Texture", 2D) = "white" {}
        
        // 🔥 核心可调参数
        _TilingX ("Arrow Density (箭头密度)", Range(0.1, 2)) = 0.5
        _FlowSpeed ("Flow Speed (流动速度)", Range(-20, 20)) = 1
        _Color ("Tint Color (颜色)", Color) = (1,1,1,1)
        _FadeOut ("End Fade (末端透明度)", Range(0,1)) = 0.3
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        LOD 100
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
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float distance : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _TilingX;
            float _FlowSpeed;
            float4 _Color;
            float _FadeOut;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                // 🔥 箭头流动：UV滚动动画
                float scroll = _Time.x * _FlowSpeed;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex) * float2(_TilingX, 1) + float2(scroll, 0);
                // 轨迹渐变
                o.distance = v.uv.x;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                col *= _Color;
                // 末端渐隐
                col.a *= lerp(1, _FadeOut, i.distance);
                return col;
            }
            ENDCG
        }
    }
}