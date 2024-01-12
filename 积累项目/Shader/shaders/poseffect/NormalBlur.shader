// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "WPYS/PostEffect/NormalBlur"
{
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_BlurSize ("Blur Size", Float) = 1.5
	}
	CGINCLUDE
		#include "UnityCG.cginc"

		sampler2D _MainTex;
		half4 _MainTex_ST;
		uniform half4 _MainTex_TexelSize;

		uniform half _BlurSize;

		static const half curve[7] = { 0.0205, 0.0855, 0.232, 0.324, 0.232, 0.0855, 0.0205 };  // gauss'ish blur weights

		struct v2f_tap {
			float4 pos : SV_POSITION;
			half2 uv20 : TEXCOORD0;
			half2 uv21 : TEXCOORD1;
			half2 uv22 : TEXCOORD2;
			half2 uv23 : TEXCOORD3;
		};			

		v2f_tap vert4Tap ( appdata_img v ) {
			v2f_tap o;

			o.pos = UnityObjectToClipPos (v.vertex);
        	o.uv20 = v.texcoord + _MainTex_TexelSize.xy;				
			o.uv21 = v.texcoord + _MainTex_TexelSize.xy * half2(-0.5h,-0.5h);	
			o.uv22 = v.texcoord + _MainTex_TexelSize.xy * half2(0.5h,-0.5h);		
			o.uv23 = v.texcoord + _MainTex_TexelSize.xy * half2(-0.5h,0.5h);		

			return o; 
		}					
		
		fixed4 fragDownsample ( v2f_tap i ) : SV_Target {				
			fixed4 color = tex2D (_MainTex, i.uv20);
			color += tex2D (_MainTex, i.uv21);
			color += tex2D (_MainTex, i.uv22);
			color += tex2D (_MainTex, i.uv23);
			return color / 4;
		}


		struct v2f_withBlurCoords8 
		{
			float4 pos : SV_POSITION;
			half2 uv : TEXCOORD0;
			half2 offs : TEXCOORD1;
		};

		v2f_withBlurCoords8 vertBlurHorizontal (appdata_img v)
		{
			v2f_withBlurCoords8 o;
			o.pos = UnityObjectToClipPos (v.vertex);
			
			o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
			o.offs = _MainTex_TexelSize.xy * half2(1.0, 0.0) * _BlurSize;

			return o; 
		}
		
		v2f_withBlurCoords8 vertBlurVertical (appdata_img v)
		{
			v2f_withBlurCoords8 o;
			o.pos = UnityObjectToClipPos (v.vertex);
			
			o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
			o.offs = _MainTex_TexelSize.xy * half2(0.0, 1.0) * _BlurSize;
			 
			return o; 
		}	

		fixed4 fragBlur8 ( v2f_withBlurCoords8 i ) : SV_Target
		{
			half2 uv = i.uv; 
			half2 netFilterWidth = i.offs;  
			half2 coords = uv - netFilterWidth * 3.0;  
			
			fixed4 color = 0;
  			for( int l = 0; l < 7; l++ )  
  			{   
				fixed4 tap = tex2D(_MainTex, coords);
				color += tap * curve[l];
				coords += netFilterWidth;
  			}
  			color.a = 1;
			return color;
		}
	ENDCG

	SubShader {
		Tags { "IgnoreProjector"="True"}
		ZTest Off Cull Off ZWrite Off Blend Off Lighting Off Fog { Mode off }

		// 0
		Pass { 
		
			CGPROGRAM
			
			#pragma vertex vert4Tap
			#pragma fragment fragDownsample
			
			ENDCG
			 
			}
		// 1
		Pass {
			CGPROGRAM 
			
			#pragma vertex vertBlurVertical
			#pragma fragment fragBlur8
			
			ENDCG 
			}	
			
		// 2
		Pass {	
			CGPROGRAM
			
			#pragma vertex vertBlurHorizontal
			#pragma fragment fragBlur8
			
			ENDCG
			}	
	}
	Fallback off
}
