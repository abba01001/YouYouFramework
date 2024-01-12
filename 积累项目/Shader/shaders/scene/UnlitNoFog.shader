// Unlit shader. Simplest possible textured shader.
// - no lighting
// - no lightmap support
// - no per-material color

Shader "WPYS/Scene/UnlitOffFog" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
}

SubShader {
	Tags { "RenderType"="Opaque"
			"IgnoreProjector"="True"}
	LOD 100
	Fog {Mode Off}
	Pass {
		Lighting Off
		SetTexture [_MainTex] { combine texture } 
	}
}
}
