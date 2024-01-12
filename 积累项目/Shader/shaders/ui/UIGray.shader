Shader "Custom/UI/UIGray"
{
	Properties
	{
		[PerRendererData]_MainTex ("Texture", 2D) = "white" {}
		_Brightness("Bright", Range(0, 2)) = 1
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		_StencilComp ("Stencil Comparison", Float) = 8
		_Stencil ("Stencil ID", Float) = 0
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255
		 _ColorMask ("Color Mask", Float) = 15
	}
	SubShader
	{
		Tags { "Queue"="Transparent"   
        		"IgnoreProjector"="True"   
        		"RenderType"="Transparent"   
        		"PreviewType"="Plane"  
        		"CanUseSpriteAtlas"="True"}
		Stencil
		{
			Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp]
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
		}
		Pass
		{
			ZWrite off
			Lighting Off
			Fog { Mode Off } 
            Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma multi_compile DUMMY PIXELSNAP_ON

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				fixed4 color : COLOR;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.color = v.color;
				#ifdef PIXELSNAP_ON  
	            o.vertex = UnityPixelSnap(o.vertex);  
	            #endif 
				return o;
			}

			fixed _Brightness;
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv) * i.color;
				fixed gray = col.r * 0.299 + col.g * 0.587 + col.b * 0.114;
				gray *= _Brightness;
				fixed4 ret = fixed4(gray, gray, gray, col.a);
				return ret;
			}
			ENDCG
		}
	}
}
