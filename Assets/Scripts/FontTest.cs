using UnityEngine;
using Unity.Mathematics;
using Voxell.GPUVectorGraphics.Font;
using Voxell.GPUVectorGraphics;

public class FontTest : MonoBehaviour
{
  public FontCurve fontCurve;
  public string text;
  public float debugSize = 0.05f;

  private int[] _glyphIndices;
  private int _charCount;

  private void Start()
  {
    _charCount = text.Length;
    _glyphIndices = new int[_charCount];
    for (int c=0; c < _charCount; c++)
    {
      char character = text[c];
      int glyphIdx = fontCurve.TryGetGlyhIndex(character);
      _glyphIndices[c] = glyphIdx;
    }
  }

  private void OnDrawGizmos()
  {
    Start();
    float3 shift = float3.zero;
    Gizmos.color = new Color(0.6f, 0.8f, 1.0f, 0.7f);
    for (int g=0; g < _charCount; g++)
    {
      int glyphIdx = _glyphIndices[g];
      if (glyphIdx != -1)
      {
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

            Gizmos.DrawLine(new float3(currSegment.p0, 0.0f) + shift, new float3(nextSegment.p0, 0.0f) + shift);
          }
        }
        shift.x += maxRect.x;
      }
    }
  }
}
