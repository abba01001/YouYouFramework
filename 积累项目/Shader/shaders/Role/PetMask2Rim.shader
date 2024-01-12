Shader "Unlit/PetMask2Rim"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Mask1("Mask1",2D) = "white" {}
        _FlowTex1 ("Flow Texture1", 2D) = "gray" {}
        _Mask2("Mask1",2D) = "white" {}
        _FlowTex2 ("Flow Texture2", 2D) = "gray" {}
        _Color ("Rim Color", Color) = (0.5,0.5,0.5,0.5)
	    _FPOW("FPOW Fresnel", Float) = 5.0
        _R0("R0 Fresnel", Float) = 0.05
        _ScrollingSpeed("Scrolling speed", Vector) = (0,0,0,0)
        _ScrollingSpeed2("Scrolling speed2", Vector) = (-2,2,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry"}
        LOD 100
        // Blend SrcAlpha One
        Pass
        {
            Name "Pet"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            // #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 texcoord1:TEXCOORD1;
                float2 texcoord2:TEXCOORD2;
                // UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float2 fresnel:TEXCOORD3;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }

        Pass
        {
            Name "PetMask1"
            Blend SrcAlpha One
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            // #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 texcoord1:TEXCOORD1;
                float2 texcoord2:TEXCOORD2;
                // UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float2 fresnel:TEXCOORD3;
            };

            sampler2D _MainTex;
            sampler2D _Mask1;
            sampler2D _FlowTex1;
            float4 _MainTex_ST;
            float4  _ScrollingSpeed;
            float4 _FlowTex1_ST;

            fixed4 _Color;
			float _FPOW;
			float _R0;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.texcoord1 = TRANSFORM_TEX((v.uv.xy + _Time.x * _ScrollingSpeed.xy), _FlowTex1);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col_mask1 = tex2D(_Mask1, i.uv);
                fixed4 col_flow1 = tex2D(_FlowTex1, i.texcoord1);
                col_mask1 *= col_flow1;
                return col_mask1;
            }
            ENDCG
        }

        Pass
        {
            Name "PetMask2"
            Blend SrcAlpha One
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            // #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 texcoord1:TEXCOORD1;
                float2 texcoord2:TEXCOORD2;
                // UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float2 fresnel:TEXCOORD3;
            };

            sampler2D _MainTex;
            sampler2D _Mask2;
            sampler2D _FlowTex2;
            float4 _MainTex_ST;
            float4 _ScrollingSpeed2;
            float4 _FlowTex2_ST;

            fixed4 _Color;
			float _FPOW;
			float _R0;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.texcoord2 = TRANSFORM_TEX((v.uv.xy + _Time.x * _ScrollingSpeed2.xy), _FlowTex2);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col_mask2 = tex2D(_Mask2, i.uv);
                fixed4 col_flow2 = tex2D(_FlowTex2, i.texcoord2);

                col_mask2 *= col_flow2;
                return col_mask2;
            }
            ENDCG
        }

        Pass
        {
            Name "PetRim"
            Blend SrcAlpha One
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            // #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                fixed4 color : COLOR;
                
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 texcoord1:TEXCOORD1;
                float2 texcoord2:TEXCOORD2;
                // UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float2 fresnel:TEXCOORD3;
                fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            fixed4 _Color;
			float _FPOW;
			float _R0;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                float3 viewDir = normalize(ObjSpaceViewDir(v.vertex));
                half fresnel = saturate(1.0 - dot(v.normal, viewDir));
				fresnel = pow(fresnel, _FPOW);
				fresnel = _R0 + (1.0 - _R0) * fresnel;
                o.fresnel = float2(fresnel,1);
                o.color = v.color;
                o.color *= fresnel;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return 2.0f * i.color * _Color * tex2D(_MainTex, i.uv);
            }
            ENDCG
        }
    }
}
