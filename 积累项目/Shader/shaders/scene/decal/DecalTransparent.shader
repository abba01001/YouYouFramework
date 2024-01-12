// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Projector' with 'unity_Projector'

Shader "WPYS/Decal/DecalTransparent" {
  Properties {
     _ShadowTex ("Cookie", 2D) = "" { }
  }
  Subshader {
    Tags {"IgnoreProjector"="True"
                "Queue"="Transparent"
                "RenderType"="Transparent"}
     pass {
        ZWrite off
        // ColorMask RGB
        // Blend Off
        // Blend DstColor Zero
        Blend SrcAlpha OneMinusSrcAlpha
        Offset -1, -1
       CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #include "UnityCG.cginc"

        sampler2D _ShadowTex;
        float4x4 unity_Projector;

        struct v2f {
            half4 pos:SV_POSITION;
            half4 texc:TEXCOORD0;
        };
        v2f vert(appdata_base v)
        {
            v2f o;
            o.pos=UnityObjectToClipPos(v.vertex);
            o.texc=mul(unity_Projector,v.vertex);
            return o;
        }
        fixed4 frag(v2f i):COLOR
        {
            fixed4 c=tex2Dproj(_ShadowTex,i.texc);
            return c;
        }
        ENDCG
    }//endpass
  }
}
