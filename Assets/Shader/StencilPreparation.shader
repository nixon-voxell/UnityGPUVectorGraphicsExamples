Shader "StencilPreparation"
{
  Properties
  {
    // [HDR] _Color("Color", Color) = (1, 1, 1, 1)
  }

  SubShader
  {
    Tags
    {
      "RenderType" = "Opaque"
      "Queue" = "Geometry-1"
    }
    LOD 100
    Cull Off
    ZWrite Off

    Pass
    {
      Name "StencilPreparation"
      Tags
      {
        "RenderPipeline" = "UniversalPipeline"
        "LightMode" = "UniversalForward"
      }

      Stencil
      {
        Comp Always
        Pass Invert
      }

      ColorMask 0
    }
  }
}