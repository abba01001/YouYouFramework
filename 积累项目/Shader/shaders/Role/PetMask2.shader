Shader "Unlit/PetMask2"
{
    Properties
    {
         _MainTex ("Texture", 2D) = "white" {}
        _Mask1("Mask1",2D) = "white" {}
        _FlowTex1 ("Flow Texture1", 2D) = "gray" {}
        _Mask2("Mask1",2D) = "white" {}
        _FlowTex2 ("Flow Texture2", 2D) = "gray" {}
        _ScrollingSpeed("Scrolling speed", Vector) = (0,0,0,0)
        _ScrollingSpeed2("Scrolling speed2", Vector) = (-2,2,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry"}
        LOD 100
        // Blend SrcAlpha One
        UsePass "Unlit/PetMask2Rim/PET"

        UsePass "Unlit/PetMask2Rim/PETMASK1"

        UsePass "Unlit/PetMask2Rim/PETMASK2"
        
    }
}
