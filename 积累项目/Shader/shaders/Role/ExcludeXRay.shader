Shader "Unlit/ExcludeXRay"
{
    Properties
    {
        _Color("Color",Color)=(1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "LightMode"="ForwardBase" "Queue"="Geometry+200"}
        LOD 100

        UsePass "Unlit/RoleXRay/ROLE"
    }
}
