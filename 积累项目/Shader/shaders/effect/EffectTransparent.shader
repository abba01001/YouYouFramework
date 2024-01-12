// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "WPYS/Effect/EffectTransparent" 
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
        _TintColor("Tint Color", Color) = (1, 1, 1, 1)
        _Amply("Bright Amply", float) = 1
	}
	SubShader
	{
		Tags { "IgnoreProjector"="True"
                "Queue"="Transparent"
                "RenderType"="Transparent" }
		LOD 100

		Pass
		{
            ZWrite off
            Cull off
            Lighting Off
            Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
            #pragma fragmentoption ARB_precision_hint_fastest
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_particles
			
			#include "UnityCG.cginc"

			struct appdata
			{
				half4 vertex : POSITION;
				fixed4 color : COLOR;
				half2 uv : TEXCOORD0;
			};

			struct v2f
			{
				half4 vertex : SV_POSITION;
				fixed4 color : COLOR;
				half2 uv : TEXCOORD0;
			};

			sampler2D _MainTex;
			fixed4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.color = v.color;
				return o;
			}
			
            fixed4 _TintColor;
            fixed _Amply;

			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv) * _TintColor * i.color;
				col.rgb = col.rgb * _Amply;
				return col;
			}
			ENDCG
		}
	}
	SubShader {
		Lighting Off
		ZWrite off
		Blend SrcAlpha OneMinusSrcAlpha
		Pass {
			Name "BASE"
			SetTexture [_MainTex] {
				constantColor [_TintColor]
				combine texture * constant
				}
			SetTexture [_MainTex] {
				combine previous * Primary
			}
		}
	}
}
