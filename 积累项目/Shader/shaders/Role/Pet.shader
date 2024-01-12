Shader "Unlit/Pet"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry"}
        LOD 100
        // Blend SrcAlpha One
        UsePass "Unlit/PetMask2Rim/PET"
    }
}
