Shader "Unlit/Glass"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Noise("Noise",2D)="white"{}
        _WindControl("WindControl(x:XSpeed y:YSpeed z:ZSpeed w:windMagnitude)",vector)=(1,0,1,0.5)
        _WaveControl("WaveControl(x:XSpeed y:YSpeed z:ZSpeed w:worldSize)",vector) = (1,0,1,1)
        // _Speed("Speed",float)=1
        _offsetv("OffsetV",float)=0
        // [Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("SrcBlend",float)=1
        // [Enum(UnityEngine.Rendering.BlendMode)]_DstBlend("DstBlend",float)=6

    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry+200"}
        LOD 100

        Pass
        {
            // Blend [_SrcBlend] [_DstBlend]
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                // fixed3 color:COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _Noise;
            float4 _Noise_ST;
            float4 _WindControl;
            float4 _WaveControl;
            // float _Speed;
            fixed _offsetv;

            v2f vert (appdata v)
            {
                v2f o;
                float4 worldPos = mul(unity_ObjectToWorld,v.vertex);
                float2 samplePos = worldPos.xz/_WaveControl.w;
                samplePos += _Time.x*-_WaveControl.xz;
                fixed waveSample = tex2Dlod(_Noise,float4(samplePos,0,0)).g;
                worldPos.x +=  (sin(waveSample *_WindControl.x)/2  - 0.5)*_WaveControl.x* _WindControl.w*saturate(v.uv.y-_offsetv);
                worldPos.z +=  (sin(waveSample *_WindControl.z)/2 - 0.5)*_WaveControl.z* _WindControl.w*saturate(v.uv.y-_offsetv);
                // worldPos.x += 2*sin(waveSample + _Time.x*_Speed)*saturate(v.uv.y-_offsetv);
                o.vertex = mul(UNITY_MATRIX_VP,worldPos);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                // o.color = fixed3(waveSample,waveSample,waveSample);
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
    }
}
