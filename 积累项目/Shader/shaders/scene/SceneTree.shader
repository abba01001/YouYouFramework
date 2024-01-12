// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "WPYS/Scene/SceneTree"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
        _TintColor("Tint Color", Color) = (1, 1, 1, 1)
        _Cutoff("Cut Off", Range(0, 1)) = 0.5
        _Bright("Bright", Range(0, 10)) = 1
	}
	SubShader
	{
		Tags { "IgnoreProjector"="True"
                "Queue"="Transparent"
                "RenderType"="Transparent" }
		LOD 100

		Pass
		{
			Tags { "LightMode" = "Vertex" }
			Cull Off
            Lighting Off
            Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
            #pragma fragmentoption ARB_precision_hint_fastest
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			
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
				UNITY_FOG_COORDS(2)
			};

			sampler2D _MainTex;
			fixed4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.color = v.color;
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
            fixed4 _TintColor;
            fixed _Cutoff;
            fixed _Bright;

			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv) * _TintColor * i.color;
				clip(col.a - _Cutoff);
				col.rgb = col.rgb * _Bright;
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
		// Lightmapped, encoded as dLDR 
		Pass
		{
			Tags { "LightMode" = "VertexLM" }
			Cull Off
            Lighting Off
            Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
            #pragma fragmentoption ARB_precision_hint_fastest
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			#pragma multi_compile LIGHTMAP_ON LIGHTMAP_OFF
			
			#include "UnityCG.cginc"

			struct appdata
			{
				half4 vertex : POSITION;
				fixed4 color : COLOR;
				half2 uv : TEXCOORD0;
				half2 uv_l : TEXCOORD1;
			};

			struct v2f
			{
				half4 vertex : SV_POSITION;
				fixed4 color : COLOR;
				half2 uv : TEXCOORD0;
				half2 uv_l : TEXCOORD1;
				UNITY_FOG_COORDS(2)
			};

			sampler2D _MainTex;
			fixed4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv_l.xy = v.uv_l.xy * unity_LightmapST.xy + unity_LightmapST.zw;
				o.color = v.color;
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
            fixed4 _TintColor;
            fixed _Cutoff;
            fixed _Bright;

			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv) * _TintColor * i.color;
				clip(col.a - _Cutoff);
				col.rgb = col.rgb * DecodeLightmap( UNITY_SAMPLE_TEX2D (unity_Lightmap, i.uv_l.xy)) * _Bright;
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
		// Lightmapped, encoded as RGBM 
		Pass
		{
			Tags { "LightMode" = "VertexLMRGBM" }
			Cull Off
            Lighting Off
            Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
            #pragma fragmentoption ARB_precision_hint_fastest
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			#pragma multi_compile LIGHTMAP_ON LIGHTMAP_OFF
			
			#include "UnityCG.cginc"

			struct appdata
			{
				half4 vertex : POSITION;
				fixed4 color : COLOR;
				half2 uv : TEXCOORD0;
				half2 uv_l : TEXCOORD1;
			};

			struct v2f
			{
				half4 vertex : SV_POSITION;
				fixed4 color : COLOR;
				half2 uv : TEXCOORD0;
				half2 uv_l : TEXCOORD1;
				UNITY_FOG_COORDS(2)
			};

			sampler2D _MainTex;
			fixed4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv_l.xy = v.uv_l.xy * unity_LightmapST.xy + unity_LightmapST.zw;
				o.color = v.color;
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
            fixed4 _TintColor;
            fixed _Cutoff;
            fixed _Bright;

			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv) * _TintColor * i.color;
				clip(col.a - _Cutoff);
				col.rgb = col.rgb * DecodeLightmap( UNITY_SAMPLE_TEX2D (unity_Lightmap, i.uv_l.xy)) * _Bright;
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
	SubShader {
		Lighting Off
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
