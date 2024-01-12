Shader "OverDraw/OverDraw"
{
    Properties
    {
        _Color("Color",Color)=(0.858,0.160,0.753,0.5)
        [Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("SrcBlend",float)=1
        [Enum(UnityEngine.Rendering.BlendMode)]_DstBlend("DstBlend",float)=0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        LOD 100

        Pass
        {
            Fog {Mode Off}
            ZWrite Off
            ZTest Always
            Blend One One
            // Blend [_SrcBlend] [_DstBlend]
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
                float4 vertex : SV_POSITION;
            };
            fixed4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return fixed4(0.1,0.04,0.02,0);
            }
            ENDCG
        }
    }
}
