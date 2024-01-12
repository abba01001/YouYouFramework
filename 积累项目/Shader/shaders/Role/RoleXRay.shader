// Upgrade NOTE: replaced tex2D unity_LightmapInd with UNITY_SAMPLE_TEX2D_SAMPLER

// Upgrade NOTE: replaced tex2D unity_LightmapInd with UNITY_SAMPLE_TEX2D_SAMPLER

Shader "Unlit/RoleXRay"
{
    Properties
    {
        _Color("Color",Color)=(1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}
        _OColor("OColor",Color)=(0,0.529,0.6627,0)
        _OcclusionPow("OcclusionPow",Range(0,10))=0.2
        _OccusionStrength("OccusionStrength",Range(1,4))=1
        [Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("SrcBlend",float)=1
        [Enum(UnityEngine.Rendering.BlendMode)]_DstBlend("DstBlend",float)=6
    }
    
    CGINCLUDE        
            #pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float3 normal:NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                fixed3 SHLighting:COLOR;
                float3 normal:NORMAL;
                float3 viewDir:NORMAL1;
                #ifdef LIGHTMAP_ON
                    float2 LMuv : TEXCOORD1;
                #endif
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            fixed4 _OColor;
            float _OcclusionPow;
            float _OccusionStrength;

            v2f vert2 (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.viewDir = normalize(WorldSpaceViewDir(o.vertex));
                return o;
            }

            fixed4 frag2 (v2f i) : SV_Target
            {
                i.normal = normalize(i.normal);
                float dotVal = pow((1 - saturate(dot(i.viewDir,i.normal))),_OcclusionPow)*_OccusionStrength;
                fixed3 col = _OColor.rgb*dotVal;
                return fixed4(col,_OColor.a);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                #ifdef LIGHTMAP_ON
                    o.LMuv = v.uv1.xy*unity_LightmapST.xy + unity_LightmapST.zw;
                #endif
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                i.normal = normalize(i.normal);
                fixed4 col = tex2D(_MainTex, i.uv);
                #ifdef LIGHTMAP_ON
                    half3 bakedColor = DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.LMuv));
                    #ifdef UNITY_COLORSPACE_GAMMA
                        // col.rgb = LinearToGammaSpace(col.rgb);
                        // col.rgb = fixed3(GammaToLinearSpaceExact(col.x), GammaToLinearSpaceExact(col.y), GammaToLinearSpaceExact(col.z) ); //GammaToLinearSpace(baseColor);
                        col.rgb = GammaToLinearSpace (col.rgb);
                        // lm = GammaToLinearSpace (lm);
                    #endif
                    fixed4 lightmapDir = UNITY_SAMPLE_TEX2D_SAMPLER(unity_LightmapInd,unity_Lightmap,i.LMuv);
                    fixed3 lm = DecodeDirectionalLightmap(col.rgb*bakedColor,lightmapDir,i.normal);
                    col.rgb += lm;
                    #ifdef UNITY_COLORSPACE_GAMMA
                        col.rgb = LinearToGammaSpace(col.rgb);
                    #endif    
                #endif
                return fixed4(col.rgb,1);
            }
    ENDCG
    SubShader
    {
        Tags { "RenderType"="Opaque" "LightMode"="ForwardBase" "Queue"="Geometry+100"}
        LOD 100

        Pass
        {
            Name "XRay"
            Blend [_SrcBlend] [_DstBlend]
            ZTest Greater
            ZWrite Off
            CGPROGRAM
            #pragma vertex vert2
            #pragma fragment frag2
            ENDCG
        }

        Pass
        {
            Name "Role"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }
    }
}
