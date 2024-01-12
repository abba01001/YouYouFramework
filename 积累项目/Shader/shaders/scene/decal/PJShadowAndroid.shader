// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Projector' with 'unity_Projector'

Shader "WPYS/Decal/PJShadowAndroid" {
	Properties {
		_ShadowTex ("Shadow", 2D) = "gray" {}
		_ClibTex("ClipTex", 2D) = "white" {}
	}
	SubShader {
		Tags { "Queue"="Transparent"
				"IgnoreProjector"="True"}
		Pass {
			ZWrite Off
			Blend DstColor Zero
			Offset -1, -1

			CGPROGRAM
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct v2f {
				half4 pos:POSITION;
				half4 sproj:TEXCOORD0;
			};

			float4x4 unity_Projector;

			sampler2D _ShadowTex;
			sampler2D _ClibTex;

			v2f vert(half4 vertex:POSITION){
				v2f o;
				o.pos = UnityObjectToClipPos(vertex);
				o.sproj = mul(unity_Projector, vertex);
				return o;
			}

			fixed4 frag(v2f i):COLOR{
				half4 coord = UNITY_PROJ_COORD(i.sproj);
				fixed4 c = tex2Dproj(_ShadowTex, coord);
				fixed4 clip_c = tex2Dproj(_ClibTex, coord);
				fixed4 result;
				// fixed ab = step(3, c.r + c.g + c.b);
				// result.rgb = ab * 1.3;
				result.rgb = c.rgb;
				result.rgb += clip_c.rgb + 0.4;
				result.a = 1;
				return result;
			}

			ENDCG
		}
	} 
	FallBack "Diffuse"
}
