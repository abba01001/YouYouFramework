Shader "Unlit/ParticleAdditive"
{
    Properties
    {
        _TintColor("Tint Color",Color)=(0.5,0.5,0.5,0.5)
        _MainTex ("Texture", 2D) = "white" {}
        [HDR]_Color("Color",Color)=(0,0,0,0)
        _InvFade("Soft Particles Factor",Range(0.01,3.0))=1.0
        [Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("SrcBlend",float)=3
        [Enum(UnityEngine.Rendering.BlendMode)]_DstBlend("DstBlend",float)=1
    }
    Category
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType" = "Plane"}
    
    SubShader
    {
        // Tags { "RenderType"="Opaque" "Queue"= "Geometry" "PreviewType" = "Plane"}
        
        LOD 100

        ColorMask RGB
        Cull Off
        ZWrite Off
        Lighting Off
        // ColorMask RGB
        Blend [_SrcBlend] [_DstBlend]
        Pass
        {
            // 
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            // make fog work
            #pragma multi_compile_fog

            #pragma multi_compile_particles

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                fixed4 color:COLOR;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                // UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                fixed4 color:COLOR;
                #ifdef SOFTPARTICLES_ON
                    float4 projPos:TEXCOORD1;
                #endif
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _InvFade;
            fixed4 _TintColor;
            float4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                #ifdef SOFTPARTICLES_ON
                    o.projPos = ComputeScreenPos(o.vertex);
                    COMPUTE_EYEDEPTH(o.projPos.z);
                #endif
                // UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
            fixed4 frag (v2f i) : SV_Target
            {
                #ifdef SOFTPARTICLES_ON
                    float sceneZ = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(tex2Dproj(_CameraDepthTexture,UNITY_PROJ_COORD(i.projPos))));
                    float partZ = i.projPos.partZ;
                    float fade = saturate(_InvFade*(sceneZ - partZ));
                    i.color.a *= fade;
                #endif
                // sample the texture
                fixed4 col = 2.0*i.color*_TintColor*tex2D(_MainTex, i.uv);
                col.a = saturate(col.a);
                // apply fog
                // UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
    }
}
