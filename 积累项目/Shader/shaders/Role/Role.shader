Shader "Unlit/Role"
{
    Properties
    {
        _Color("Color",Color)=(1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "LightMode"="ForwardBase" "Queue"="Geometry"}
        LOD 100

        UsePass "Unlit/RoleXRay/ROLE"
    }
}
