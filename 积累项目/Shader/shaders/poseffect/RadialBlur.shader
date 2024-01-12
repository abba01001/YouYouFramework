// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "WPYS/PostEffect/RadialBlur"
{
	Properties {
                 _MainTex ("Base (RGB)", 2D) = "white" {}
                 _fSampleDist("SampleDist", Float) = 1 //采样距离
                 _fSampleStrength("SampleStrength", Float) = 2.2 //采样力度
         }
         SubShader {
                 Pass {
                 		 Tags { "IgnoreProjector"="True"}
                         ZTest Off
                         Cull Off
                         ZWrite Off
                         Lighting Off
                         Fog { Mode off }  
                         CGPROGRAM
                         #pragma vertex vert
                         #pragma fragment frag
                         #pragma fragmentoption ARB_precision_hint_fastest
         
                         #include "UnityCG.cginc"
         
                         struct appdata_t {
                                 half4 vertex : POSITION;
                                 half2 texcoord : TEXCOORD;
                         };
         
                         struct v2f {
                                 half4 vertex : POSITION;
                                 half2 texcoord : TEXCOORD;
                         };
                         
                         half4 _MainTex_ST;
                         
                         v2f vert (appdata_t v)
                         {
                                 v2f o;
                                 o.vertex = UnityObjectToClipPos(v.vertex);
                                 o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                                 return o;
                         }
         
                         uniform sampler2D _MainTex;
                         fixed _fSampleDist;
                         fixed _fSampleStrength;

                         // some sample positions  
                         static const fixed samples[10] =   
                         {   
                            -0.08,
   							-0.05,
   							-0.03,
   							-0.02,
   							-0.01,
   							0.01,
   							0.02,
   							0.03,
   							0.05,
   							0.08  
                         }; 
                         
                         fixed4 frag (v2f i) : SV_Target
                         {
                                
                            //0.5,0.5屏幕中心
                            half2 dir = half2(0.5, 0.5) - i.texcoord;//从采样中心到uv的方向向量
                            half2 texcoord = i.texcoord;
                            half dist = length(dir);
                            fixed4 color = tex2D(_MainTex, texcoord);  

                            fixed4 sum = color;
                            dir = normalize(dir) * _fSampleDist; 
                               //    6次采样
                            sum += tex2D(_MainTex, texcoord + dir * samples[0]);
                            sum += tex2D(_MainTex, texcoord + dir * samples[1]);
                            sum += tex2D(_MainTex, texcoord + dir * samples[2]);
                            sum += tex2D(_MainTex, texcoord + dir * samples[3]);
                            sum += tex2D(_MainTex, texcoord + dir * samples[4]);
                            sum += tex2D(_MainTex, texcoord + dir * samples[5]);
                            sum += tex2D(_MainTex, texcoord + dir * samples[6]);
                            sum += tex2D(_MainTex, texcoord + dir * samples[7]);
                            sum += tex2D(_MainTex, texcoord + dir * samples[8]);
                            sum += tex2D(_MainTex, texcoord + dir * samples[9]);

                            //求均值
                            sum /= 11.0f;  

                           
                            //越离采样中心近的地方，越不模糊
                            half t = saturate(dist * _fSampleStrength);  

                            //插值
                            return lerp(color, sum, t);
                            
                         }
                         ENDCG 
                 }
         } 
         Fallback off
}
