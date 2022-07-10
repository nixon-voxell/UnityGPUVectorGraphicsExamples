using UnityEngine;
using Unity.Mathematics;
using Voxell.GPUVectorGraphics.Font;
using Voxell.GPUVectorGraphics;

public class FontTest : MonoBehaviour
{
  public FontCurve fontCurve;
  public float debugSize = 0.05f;
  public int glyphIdx;

  private void OnDrawGizmos()
  {
    float3 shift = float3.zero;
    Gizmos.color = new Color(0.6f, 0.8f, 1.0f, 0.7f);
    Glyph glyph = fontCurve.Glyphs[glyphIdx];
    int contourCount = glyph.contours.Length;

    float2 maxRect = float2.zero;
    for (int c=0; c < contourCount; c++)
    {
      QuadraticContour glyphContour = glyph.contours[c];
      maxRect = math.max(glyph.maxRect, maxRect);
      int segmentCount = glyphContour.segments.Length;

      QuadraticPathSegment currSegment;
      QuadraticPathSegment nextSegment;
      for (int s=0; s < segmentCount; s++)
      {
        int nextIdx = (s + 1) % segmentCount;
        currSegment = glyphContour.segments[s];
        nextSegment = glyphContour.segments[nextIdx];
        Gizmos.DrawCube(new float3(glyphContour.segments[s].p0, 0.0f) + shift, new float3(debugSize));
        Gizmos.DrawSphere(new float3(glyphContour.segments[s].p1, 0.0f) + shift, debugSize*0.5f);

        Gizmos.DrawLine(
          new float3(currSegment.p0, 0.0f) + shift,
          new float3(nextSegment.p0, 0.0f) + shift
        );
      }
    }
  }
}
