Shader "OverlappingReveal"
{
  Properties
  {
    [HDR] _Color("Color", Color) = (1, 1, 1, 1)
  }

  SubShader
  {
    Tags
    {
      "RenderType" = "Opaque"
      "Queue" = "Geometry"
    }

    LOD 100
    // Cull Off
    // ZWrite Off

    Pass
    {
      Name "UnlitPass"
      Tags
      {
        "RenderPipeline" = "UniversalPipeline"
        "LightMode" = "UniversalForward"
      }

      Stencil
      {
        Ref 0
        Comp NotEqual
        Pass Keep
      }

      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag

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
      };

      v2f vert (appdata v)
      {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.uv = v.uv;
        return o;
      }

      uniform half4 _Color;

      half4 frag (v2f i) : SV_Target
      {
        return _Color;
      }
      ENDCG
    }
  }
}