Shader "Unlit/PetMask1"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Mask1("Mask1",2D) = "white" {}
        _FlowTex1 ("Flow Texture1", 2D) = "gray" {}
        _ScrollingSpeed("Scrolling speed", Vector) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry"}
        LOD 100
        // Blend SrcAlpha One
        UsePass "Unlit/PetMask2Rim/PET"

        UsePass "Unlit/PetMask2Rim/PETMASK1"
        
    }
}
