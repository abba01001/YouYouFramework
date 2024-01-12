// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "WPYS/Scene/SceneDetail" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_DetailColor ("Detail Color", Color) = (1,1,1,1)
		_DetailAmplify ("Detail amplify", float) = 1.0
		_MoveSpeed_X ("Move speed X", float) = 1.0
		_MoveSpeed_Y ("Move speed Y", float) = 1.0
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_DetailMaskTex ("Detail Mask(RGB)", 2D) = "black" {}
		_ReflectTex	("Reflect Tex(RGB)", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 100
        // Non-lightmapped  
        Pass {  
            Tags { "LightMode" = "Vertex" }  
            Lighting Off  
            CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_fog

             #include "UnityCG.cginc"

             struct appdata_lightmap {
                half4 vertex : POSITION;
                half2 uv : TEXCOORD0;
            };

            struct v2f {
                half4 vertex : SV_POSITION;
                half2 uv : TEXCOORD0;
                half2 uv_r : TEXCOORD1;
                UNITY_FOG_COORDS(2)
            };

		    sampler2D _MainTex;
			fixed4 _MainTex_ST;
            fixed4 _Color;
            fixed4 _DetailColor;
            sampler2D _DetailMaskTex;
            sampler2D _ReflectTex;
            fixed4 _ReflectTex_ST;
            fixed _DetailAmplify;
            fixed _MoveSpeed_X;
            fixed _MoveSpeed_Y;

            v2f vert (appdata_lightmap v)
			{
				v2f o;
                // UNITY_INITIALIZE_OUTPUT(v2f, o);
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv_r = TRANSFORM_TEX(v.uv, _ReflectTex);
				half3 view_dir = ObjSpaceViewDir(v.vertex);
				o.uv_r.xy += view_dir.xy * fixed2(_MoveSpeed_X * 0.02, _MoveSpeed_Y * 0.02);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv) * _Color;
				fixed4 detail_c = tex2D(_DetailMaskTex, i.uv);
				fixed4 reflect_c = tex2D(_ReflectTex, i.uv_r);
				col.rgb += detail_c.r * _DetailColor.rgb * reflect_c.rgb * _DetailAmplify;
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
		    ENDCG
        }
        // Lightmapped, encoded as dLDR  
        Pass {  
            Tags { "LightMode" = "VertexLM" }  
  
            Lighting Off  
            CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_fog
            #pragma multi_compile LIGHTMAP_ON LIGHTMAP_OFF

             #include "UnityCG.cginc"

             struct appdata_lightmap {
                half4 vertex : POSITION;
                half2 uv : TEXCOORD0;
                half2 uv_l : TEXCOORD1;
            };

            struct v2f {
                half4 vertex : SV_POSITION;
                half2 uv : TEXCOORD0;
                half2 uv_l : TEXCOORD1;
                half2 uv_r : TEXCOORD2;
                UNITY_FOG_COORDS(3)
            };

		    sampler2D _MainTex;
			fixed4 _MainTex_ST;
            fixed4 _Color;
            fixed4 _DetailColor;
            sampler2D _DetailMaskTex;
            sampler2D _ReflectTex;
            fixed4 _ReflectTex_ST;
            fixed _DetailAmplify;
            fixed _MoveSpeed_X;
            fixed _MoveSpeed_Y;

            v2f vert (appdata_lightmap v)
			{
				v2f o;
                // UNITY_INITIALIZE_OUTPUT(v2f, o);
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv_l.xy = v.uv_l.xy * unity_LightmapST.xy + unity_LightmapST.zw;
                o.uv_r = TRANSFORM_TEX(v.uv, _ReflectTex);
				half3 view_dir = ObjSpaceViewDir(v.vertex);
				o.uv_r.xy += view_dir.xy * fixed2(_MoveSpeed_X * 0.02, _MoveSpeed_Y * 0.02);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                col.rgb = col.rgb * DecodeLightmap( UNITY_SAMPLE_TEX2D (unity_Lightmap, i.uv_l.xy));

                fixed4 detail_c = tex2D(_DetailMaskTex, i.uv);
				fixed4 reflect_c = tex2D(_ReflectTex, i.uv_r);
				col.rgb += detail_c.r * _DetailColor.rgb * reflect_c.rgb * _DetailAmplify;
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
		    ENDCG
        }

        // Lightmapped, encoded as RGBM  
        Pass {  
            Tags { "LightMode" = "VertexLMRGBM" }  
          
            Lighting Off  
            CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_fog
            #pragma multi_compile LIGHTMAP_ON LIGHTMAP_OFF

             #include "UnityCG.cginc"

             struct appdata_lightmap {
                half4 vertex : POSITION;
                half2 uv : TEXCOORD0;
                half2 uv_l : TEXCOORD1;
            };

            struct v2f {
                half4 vertex : SV_POSITION;
                half2 uv : TEXCOORD0;
                half2 uv_l : TEXCOORD1;
                half2 uv_r : TEXCOORD2;
                UNITY_FOG_COORDS(3)
            };

		    sampler2D _MainTex;
			fixed4 _MainTex_ST;
            fixed4 _Color;
            fixed4 _DetailColor;
            sampler2D _DetailMaskTex;
            sampler2D _ReflectTex;
            fixed4 _ReflectTex_ST;
            fixed _DetailAmplify;
            fixed _MoveSpeed_X;
            fixed _MoveSpeed_Y;

            v2f vert (appdata_lightmap v)
			{
				v2f o;
                // UNITY_INITIALIZE_OUTPUT(v2f, o);
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv_l.xy = v.uv_l.xy * unity_LightmapST.xy + unity_LightmapST.zw;
                o.uv_r = TRANSFORM_TEX(v.uv, _ReflectTex);
				half3 view_dir = ObjSpaceViewDir(v.vertex);
				o.uv_r.xy += view_dir.xy * fixed2(_MoveSpeed_X * 0.02, _MoveSpeed_Y * 0.02);
				UNITY_TRANSFER_FOG(o, o.vertex);
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                col.rgb = col.rgb * DecodeLightmap( UNITY_SAMPLE_TEX2D (unity_Lightmap, i.uv_l.xy));

                fixed4 detail_c = tex2D(_DetailMaskTex, i.uv);
				fixed4 reflect_c = tex2D(_ReflectTex, i.uv_r);
				col.rgb += detail_c.r * _DetailColor.rgb * reflect_c.rgb * _DetailAmplify;
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
		    ENDCG
        }
	}
}
/* *
Shader "WPYS/Scene/SceneSolid" {  
Properties {  
    _MainTex ("Albedo (RGB)", 2D) = "white" {}  
    _Color ("Color", Color) = (1,1,1,1)  
}  
  
SubShader {  
    Tags { "RenderType"="Opaque" }  
    LOD 100  
      
    // Non-lightmapped  
    Pass {  
        Tags { "LightMode" = "Vertex" }  
        Lighting Off  
        SetTexture [_MainTex] { combine texture }   
    }  
      
    // Lightmapped, encoded as dLDR  
    Pass {  
        Tags { "LightMode" = "VertexLM" }  
  
        Lighting Off  
        BindChannels {  
            Bind "Vertex", vertex  
            Bind "texcoord1", texcoord0 // lightmap uses 2nd uv  
            Bind "texcoord", texcoord1 // main uses 1st uv  
        }  
          
        SetTexture [unity_Lightmap] {  
            matrix [unity_LightmapMatrix]  
            combine texture  
        }  
        SetTexture [_MainTex] {  
            combine texture * previous DOUBLE, texture * primary  
        }  
    }  
      
    // Lightmapped, encoded as RGBM  
    Pass {  
        Tags { "LightMode" = "VertexLMRGBM" }  
          
        Lighting Off  
        BindChannels {  
            Bind "Vertex", vertex  
            Bind "texcoord1", texcoord0 // lightmap uses 2nd uv  
            Bind "texcoord", texcoord1 // main uses 1st uv  
        }  
          
        SetTexture [unity_Lightmap] {  
            matrix [unity_LightmapMatrix]  
            combine texture * texture alpha DOUBLE  
        }  
        SetTexture [_MainTex] {  
            combine texture * previous QUAD, texture * primary  
        }  
    }      
}  
} 
*/
