Shader "UI/GuideMask"
{
    Properties{
        [PerRendererData] _MainTex("Sprite Texture", 2D)="white"{}
        _Color("Tint",Color)=(1,1,1,1)

        _StencilComp("Stencil Comparison", Float) = 8
        _Stencil("Stencil ID", Float) = 0
        _StencilOp("Stencil Operation", Float) = 0
        _StencilWriteMask("Stencil Write Mask", Float) = 255
        _StencilReadMask("Stencil Read Mask", Float) = 255    
        _ColorMask("Color Mask", Float) = 15

        _Origin("Rect",Vector) = (0,0,0,0) // xy: 中心, zw: 宽高
        _TopOri("TopCircle",Vector) = (0,0,0,0) // xy: 圆心, z: 半径
        _Raid("RectRaid",Range(0,100)) = 0
        _MaskType("Type",Int) = 0       
        _FadeWidth("FadeWidth",Range(0,100)) = 60
    }

    SubShader{
        Tags{
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Stencil{
            Ref[_Stencil]
            Comp[_StencilComp]
            Pass[_StencilOp]
            ReadMask[_StencilReadMask]
            WriteMask[_StencilWriteMask]
        }

        Cull Off Lighting Off ZWrite Off
        ZTest[unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask[_ColorMask]

        Pass{
            Name "Default"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _Origin;
            float4 _TopOri;
            float _Raid;
            float _FadeWidth;
            int _MaskType;

            v2f vert(appdata_t IN) {
                v2f OUT;
                OUT.worldPosition = IN.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            // 内部工具：计算通用的圆角矩形距离
            float getRectSDF(float2 p, float2 center, float2 size, float radius) {
                float2 d = abs(p - center) - (size * 0.5 - radius);
                return length(max(d, 0.0)) + min(max(d.x, d.y), 0.0) - radius;
            }

            // 圆形带羽化
            fixed checkInCircle(float4 worldPosition) {
                float dist = distance(worldPosition.xy, _TopOri.xy);
                // 内部为0，外部为1
                return smoothstep(_TopOri.z - _FadeWidth, _TopOri.z + _FadeWidth, dist);
            }

            // 矩形（SDF简化版）
            fixed checkInRect (float4 worldPosition) {
                float2 d = abs(worldPosition.xy - _Origin.xy) - _Origin.zw * 0.5;
                return step(0, max(d.x, d.y));
            }

            fixed4 frag(v2f IN) : SV_Target {
                half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
                
                fixed mask = 1.0;
                
                // 使用 if 处理 MaskType，在现代 GPU 上少量子路径切换开销很低
                if (_MaskType == 0) { //圆形
                    mask = checkInCircle(IN.worldPosition);
                } 
                else if (_MaskType != 0){ //圆角矩形
                    mask = getRectSDF(IN.worldPosition.xy, _Origin.xy, _Origin.zw, _Raid) > 0 ? 1 : 0;
                }

                color.a *= mask;
                return color;
            }
            ENDCG
        }
    }
}