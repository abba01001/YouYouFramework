Shader "Unlit/Effect/EffectMask"
{
    Properties
    {
        // _MainTex ("Texture", 2D) = "white" {}
        _TintColor("Tint Color",Color)=(0,0,0,0)
        _MaskTexture("Mask Texture",2D) = "white" {}
        _FlowTex ("Flow Texture", 2D) = "gray" {}
        _ScrollingSpeed("Scrolling speed", Vector) = (0,0,0,0)
    }
    SubShader
    {
        // Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        Blend SrcAlpha One
        // Cull On
        Lighting Off ZWrite Off Fog { Color (0,0,0,0) }
        Pass
        {
            
            // Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            // #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 texcoord1:TEXCOORD1;
                float2 texcoord2:TEXCOORD2;
                // UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float2 fresnel:TEXCOORD3;
            };

            sampler2D _MainTex;
            sampler2D _MaskTexture;
            sampler2D _FlowTex;
            float4 _MainTex_ST;
            float4 _ScrollingSpeed;
            float4 _FlowTex_ST;

            fixed4 _TintColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.texcoord2 = TRANSFORM_TEX((v.uv.xy + _Time.x * _ScrollingSpeed.xy), _FlowTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 col = tex2D(_FlowTex, i.texcoord2);
				col.a *= tex2D(_MaskTexture, i.uv).a;
				col = col * _TintColor;
				return col;
            }
            ENDCG
        }
        
    }
}
