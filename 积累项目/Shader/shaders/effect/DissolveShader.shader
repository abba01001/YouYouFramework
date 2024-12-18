﻿Shader "Game/DissolveShader"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_NoiseTex("Noise", 2D) = "white" {}
		_Threshold("Threshold", Range(0.0, 1.0)) = 0.5
		_EdgeLength("Edge Length", Range(0.0, 0.2)) = 0.1
		_RampTex("Ramp", 2D) = "white" {}
		_StartPoint("Start Point", Vector) = (0, 0, 0, 0)
		_MaxDistance("Max Distance", Float) = 0
		_DistanceEffect("Distance Effect", Range(0.0, 1.0)) = 0.5
	}
		SubShader
		{
			Tags { "Queue" = "Geometry" "RenderType" = "Opaque" }

			Pass
			{
				Cull Off

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
					float2 uvMainTex : TEXCOORD0;
					float2 uvNoiseTex : TEXCOORD1;
					float2 uvRampTex : TEXCOORD2;
					float3 objPos : TEXCOORD3;
					float3 objStartPos : TEXCOORD4;
				};

				sampler2D _MainTex;
				float4 _MainTex_ST;
				sampler2D _NoiseTex;
				float4 _NoiseTex_ST;
				float _Threshold;
				float _EdgeLength;
				sampler2D _RampTex;
				float4 _RampTex_ST;
				float _MaxDistance;
				float4 _StartPoint;
				float _DistanceEffect;

				v2f vert(appdata v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);

					o.uvMainTex = TRANSFORM_TEX(v.uv, _MainTex);
					o.uvNoiseTex = TRANSFORM_TEX(v.uv, _NoiseTex);
					o.uvRampTex = TRANSFORM_TEX(v.uv, _RampTex);

					o.objPos = v.vertex;
					o.objStartPos = mul(unity_WorldToObject, _StartPoint);

					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					float dist = length(i.objPos.xyz - i.objStartPos.xyz);
					float normalizedDist = saturate(dist / _MaxDistance);

					fixed cutout = tex2D(_NoiseTex, i.uvNoiseTex).r * (1 - _DistanceEffect) + normalizedDist * _DistanceEffect;
					clip(cutout - _Threshold);

					float degree = saturate((cutout - _Threshold) / _EdgeLength);
					fixed4 edgeColor = tex2D(_RampTex, float2(degree, degree));

					fixed4 col = tex2D(_MainTex, i.uvMainTex);

					fixed4 finalColor = lerp(edgeColor, col, degree);
					return fixed4(finalColor.rgb, 1);
				}
				ENDCG
			}
		}
}
