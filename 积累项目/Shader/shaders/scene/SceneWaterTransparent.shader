// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "WPYS/Scene/SceneWaterTransparent" {
Properties {
	_horizonColor ("Horizon color", COLOR)  = ( .172 , .463 , .435 , 0)
	_WaveScale ("Wave scale", Range (0.02,0.15)) = .07
	[NoScaleOffset] _ColorControl ("Reflective color (RGB) fresnel (A) ", 2D) = "" { }
	[NoScaleOffset] _ColorControl2 ("Waves ", 2D) = "" { }
	WaveSpeed ("Wave speed (map1 x,y; map2 x,y)", Vector) = (19,9,-16,-7)
	_brightness ("Brightness", Range (0,10)) = 1
	}

CGINCLUDE

#include "UnityCG.cginc"

uniform fixed4 _horizonColor;

uniform fixed4 WaveSpeed;
uniform fixed _WaveScale;
uniform fixed4 _WaveOffset;

struct appdata {
	float4 vertex : POSITION;
	float3 normal : NORMAL;
};

struct v2f {
	float4 pos : SV_POSITION;
	half2 bumpuv[2] : TEXCOORD0;
	// half3 viewDir : TEXCOORD2;
	UNITY_FOG_COORDS(3)
};

v2f vert(appdata v)
{
	v2f o;

	o.pos = UnityObjectToClipPos (v.vertex);

	// scroll bump waves
	half4 temp;
	half4 wpos = mul (unity_ObjectToWorld, v.vertex);
	temp.xyzw = wpos.xzxz * _WaveScale + _WaveOffset;
	// o.bumpuv[0] = temp.xy * half2(.4, .45) + WaveSpeed.xy * _Time.xx;
	o.bumpuv[0] = temp.xy + frac(WaveSpeed.xy * _Time.xx);
	o.bumpuv[1] = temp.wz + frac(WaveSpeed.wz * _Time.xx);

	// object space view direction
	//o.viewDir.xzy = normalize( WorldSpaceViewDir(v.vertex) );

	UNITY_TRANSFER_FOG(o,o.pos);
	return o;
}

ENDCG


Subshader {
	Tags { "IgnoreProjector"="True"
                   "RenderType"="Transparent"
                   "Queue"="Transparent" }
    LOD 100
    Lighting Off
    ZWrite Off
	Pass {
	Blend SrcAlpha OneMinusSrcAlpha
	// Blend SrcAlpha One
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma multi_compile_fog
#pragma fragmentoption ARB_precision_hint_fastest

sampler2D _ColorControl2;
sampler2D _ColorControl;

fixed _brightness;

half4 frag( v2f i ) : COLOR
{
	//half3 bump1 = UnpackNormal(tex2D( _ColorControl2, i.bumpuv[0] )).rgb;
	//half3 bump2 = UnpackNormal(tex2D( _ColorControl2, i.bumpuv[1] )).rgb;
	// half3 bump = (bump1 + bump2) * 0.5;
	
	// half fresnel = dot( i.viewDir, bump );
	// half4 water = tex2D( _ColorControl, float2(fresnel,fresnel) );
	half4 water2 = tex2D(_ColorControl2, i.bumpuv[1].xy);
	half4 water1 = tex2D(_ColorControl, i.bumpuv[0].xy);
	half4 water = water1 * 0.8 + water2 * 0.6;
	// half4 water = tex2D( _ColorControl, i.bumpuv[0] );
	half4 col;
	// col.rgb = lerp( water.rgb, _horizonColor.rgb, water.a);
	col.rgb = water.rgb * _horizonColor.rgb * _brightness;
	// col.rgb = water.rgb * _brightness;
	col.a = water.a * _horizonColor.a;
	// col.a = water.a * _horizonColor.a;

	UNITY_APPLY_FOG(i.fogCoord, col);
	return col;
}
ENDCG
	}
}

}
