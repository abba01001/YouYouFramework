// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/myAdditive" {
    Properties{
        _MainTex("MainTex (RGB)", 2D) = "white" {}
        _TintColor("TintColor (RGB)", Color) = (0.5,0.5,0.5,0.5)
        _InvFade("Transparency Factor", Range(0.0,1.0)) = 1.0
        _BodyType("BodyType", Range(0.0,1.0)) = 1.0

    }
        SubShader{
               Tags { "QUEUE" = "Transparent" "IGNOREPROJECTOR" = "true" "RenderType" = "Transparent" }

            Pass {
               ZWrite Off
                Blend SrcAlpha One//use (SrcAlpha,One), not (One,One)
                CGPROGRAM

                #pragma vertex vert
                #pragma fragment frag
                #pragma target 3.0

                #include "UnityCG.cginc"

                sampler2D _MainTex;
                fixed4 _TintColor;
                float _InvFade;
                float _BodyType;

                struct myV2F {
                    float4 pos:SV_POSITION;//http://wiki.unity3d.com/index.php?title=Shader_Code
                    float2 uv    : TEXCOORD0;
                };

                myV2F vert(appdata_base v) {
                    myV2F v2f;
                    v2f.pos = UnityObjectToClipPos(v.vertex);
                    v2f.uv = v.texcoord;
                    return v2f;
                }


                fixed4 frag(myV2F v2f) : COLOR {
                    fixed4 main_color = tex2D(_MainTex, v2f.uv);
                    fixed4 tranpare_color = fixed4(1, 1, 1, _InvFade);
                    main_color = _TintColor * main_color * tranpare_color;
                    return main_color;
                }

                ENDCG
            }
    }
}