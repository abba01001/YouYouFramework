Shader "Custom/SimplestInstancedShaderWithDifferentTextures"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing // 启用实例化支持
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0; // 纹理坐标
                UNITY_VERTEX_INPUT_INSTANCE_ID // 实例化 ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0; // 传递纹理坐标
                UNITY_VERTEX_INPUT_INSTANCE_ID // 实例化 ID
            };

            // 定义每个实例的常量缓冲区
            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(texture2D, _MainTex)  // 实例化的纹理属性
                UNITY_DEFINE_INSTANCED_PROP(sampler2D, _MainTex_sampler)  // 实例化的采样器属性
            UNITY_INSTANCING_BUFFER_END(Props)

            // 纹理的平铺和偏移
            float4 _MainTex_ST;

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex); // 纹理坐标变换
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);

                // 使用 tex2D 来访问纹理
                fixed4 texColor = tex2D(UNITY_ACCESS_INSTANCED_PROP(Props, _MainTex), i.uv);
                return texColor;
            }
            ENDCG
        }
    }
}
