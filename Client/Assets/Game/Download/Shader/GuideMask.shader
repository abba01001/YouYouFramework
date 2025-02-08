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

		_Origin("Rect",Vector) = (0,0,0,0)
		_TopOri("TopCircle",Vector) = (0,0,0,0)
		_Raid("RectRaid",Range(0,100)) = 0
		_MaskType("Type",Float) = 0		
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

		Cull Off
		Lighting Off
		ZWrite Off
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

			struct appdata_t
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float2 texcoord : TEXCOORD0;
            };

			struct v2f{
				float4 vertex:SV_POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				float4 worldPosition : TEXCOORD1;
            };

			sampler2D _MainTex;
			fixed4 _Color;
			fixed4 _TextureSampleAdd;
			float4 _ClipRect;
			float4 _Origin;
			float4 _TopOri;
			float _Raid;
			float _FadeWidth;
			float _MaskType;
			//0 圆形 1 矩形 2 圆角矩形 

			v2f vert(appdata_t IN){
				v2f OUT;
				OUT.worldPosition = IN.vertex;
				OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color * _Color;
				return OUT;
			}
			//垂直圆角矩形
			fixed checkInCircleRectVectory (float4 worldPosition) {
				float4 rec1Pos=float4(_Origin.x-_Origin.z/2,_Origin.y-_Origin.w/2-_Raid,_Origin.x+_Origin.z/2,_Origin.y+_Origin.w/2+_Raid);
				float4 rec2Pos=float4(_Origin.x-_Origin.z/2+_Raid,_Origin.y-_Origin.w/2-2*_Raid,_Origin.x+_Origin.z/2-_Raid,_Origin.y+_Origin.w/2+2*_Raid);
				fixed2 step1=step(rec1Pos.xy, worldPosition.xy) * step(worldPosition.xy, rec1Pos.zw);
				fixed2 step2=step(rec2Pos.xy, worldPosition.xy) * step(worldPosition.xy, rec2Pos.zw);
				fixed rec1=step1.x*step1.y<1?0:1;
				fixed rec2=step2.x*step2.y<1?0:1;
				fixed dis1=distance(float2(_Origin.x+_Origin.z/2-_Raid,_Origin.y+_Origin.w/2+_Raid),worldPosition.xy)<_Raid?1:0;
				fixed dis2=distance(float2(_Origin.x-_Origin.z/2+_Raid,_Origin.y-_Origin.w/2-_Raid),worldPosition.xy)<_Raid?1:0;
				fixed dis3=distance(float2(_Origin.x+_Origin.z/2-_Raid,_Origin.y-_Origin.w/2-_Raid),worldPosition.xy)<_Raid?1:0;
				fixed dis4=distance(float2(_Origin.x-_Origin.z/2+_Raid,_Origin.y+_Origin.w/2+_Raid),worldPosition.xy)<_Raid?1:0;
				return (dis1+dis2+dis3+dis4+rec1+rec2)>0?0:1;
			}

			//水平圆角矩形
			fixed checkInCircleRectHorizontal (float4 worldPosition) {

				float4 rec1Pos=float4(_Origin.x-_Origin.z/2-_Raid,_Origin.y-_Origin.w/2,_Origin.x+_Origin.z/2+_Raid,_Origin.y+_Origin.w/2);
				float4 rec2Pos=float4(_Origin.x-_Origin.z/2-2*_Raid,_Origin.y-_Origin.w/2+_Raid,_Origin.x+_Origin.z/2+2*_Raid,_Origin.y+_Origin.w/2-_Raid);
				fixed2 step1=step(rec1Pos.xy, worldPosition.xy) * step(worldPosition.xy, rec1Pos.zw);
				fixed2 step2=step(rec2Pos.xy, worldPosition.xy) * step(worldPosition.xy, rec2Pos.zw);
				fixed rec1=step1.x*step1.y<1?0:1;
				fixed rec2=step2.x*step2.y<1?0:1;
				fixed dis1=distance(float2(_Origin.x-_Origin.z/2-_Raid,_Origin.y+_Origin.w/2-_Raid),worldPosition.xy)<_Raid?1:0;
				fixed dis2=distance(float2(_Origin.x-_Origin.z/2-_Raid,_Origin.y-_Origin.w/2+_Raid),worldPosition.xy)<_Raid?1:0;
				fixed dis3=distance(float2(_Origin.x+_Origin.z/2+_Raid,_Origin.y+_Origin.w/2-_Raid),worldPosition.xy)<_Raid?1:0;
				fixed dis4=distance(float2(_Origin.x+_Origin.z/2+_Raid,_Origin.y-_Origin.w/2+_Raid),worldPosition.xy)<_Raid?1:0;
				return (dis1+dis2+dis3+dis4+rec1+rec2)>0?0:1;
			}
			
			// 圆形渐变过渡效果
			fixed checkInCircle(float4 worldPosition) {
			    // 计算当前位置与圆心的距离
			    float dist = distance(worldPosition.xy, _TopOri.xy);
			    // 使用 smoothstep 生成平滑过渡
			    // _TopOri.z 是圆的半径，控制渐变的过渡区间
			    float alpha = smoothstep(_TopOri.z - _FadeWidth, _TopOri.z + _FadeWidth, dist); // 加大渐变区间
			    return alpha;
			}

			
			//矩形
			fixed checkInRect (float4 worldPosition) {
				float4 temp=float4(_Origin.x-_Origin.z/2,_Origin.y-_Origin.w/2,_Origin.x+_Origin.z/2,_Origin.y+_Origin.w/2);
				float2 inside = step(temp.xy, worldPosition.xy) * step(worldPosition.xy, temp.zw);
				return inside.x*inside.y>0?0:1;
				
			}
			fixed4 frag(v2f IN) : SV_Target{
				float dist = 0.0;
				half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
				if(_MaskType==0){
					//color.a=checkInCircle(IN.worldPosition)==0?0:color.a;
					color.a *= checkInCircle(IN.worldPosition);
                }else if(_MaskType==1){
					color.a=checkInRect(IN.worldPosition)==0?0:color.a;
                }else if(_MaskType==3){
					color.a=checkInCircleRectVectory(IN.worldPosition)==0?0:color.a;
                }
				else if(_MaskType==2){
					color.a=checkInCircleRectHorizontal(IN.worldPosition)==0?0:color.a;
                }
				return color;
			}

			ENDCG
        }
    }
}
