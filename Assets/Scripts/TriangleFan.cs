using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

namespace Voxell.GPUVectorGraphics
{
  using Font;

  public class TriangleFan : MonoBehaviour
  {
    public FontCurve fontCurve;
    public char character;

    private MeshFilter _meshFilter;
    private Mesh _mesh;

    private void Start()
    {
      this._meshFilter = GetComponent<MeshFilter>();

      this._mesh = new Mesh();
      this._meshFilter.mesh = this._mesh;
    }

    private void Update()
    {
      int glyphIdx = SearchCharGlyphIndex();
      Glyph glyph = fontCurve.Glyphs[glyphIdx];

      int contourCount = glyph.contours.Length;
      if (contourCount == 0) return;

      // get contour points
      NativeList<float3> points = new NativeList<float3>(Allocator.Temp);
      List<CDT.ContourPoint> contours = new List<CDT.ContourPoint>();

      float3 maxRect = new float3(glyph.maxRect, 0.0f);
      float3 minRect = new float3(glyph.minRect, 0.0f);

      int contourStart = 0;
      for (int c=0; c < contourCount; c++)
      {
        QuadraticContour glyphContour = glyph.contours[c];
        int segmentCount = glyphContour.segments.Length;

        for (int s=0; s < segmentCount; s++)
        {
          points.Add(new float3(glyphContour.segments[s].p0, 0.0f));
          contours.Add(new CDT.ContourPoint(contourStart+s, c));
        }

        contours.Add(new CDT.ContourPoint(contourStart, c));
        contourStart += segmentCount;
      }

      // generate triangles
      int pointCount = points.Length;
      int[] indices = new int[pointCount * 3];

      int pivotIdx = 0;

      int tIdx = 0;
      for (int s=1; s < contours.Count-1; s++)
      {
        CDT.ContourPoint c0 = contours[s];
        CDT.ContourPoint c1 = contours[s + 1];

        if (c0.contourIdx != c1.contourIdx)
        {
          pivotIdx = c1.pointIdx;
          continue;
        }

        indices[tIdx] = pivotIdx;
        indices[tIdx + 1] = c0.pointIdx;
        indices[tIdx + 2] = c1.pointIdx;
        tIdx += 3;
      }

      this._mesh.SetVertices<float3>(points);
      this._mesh.SetIndices(indices, MeshTopology.Triangles, 0);

      points.Dispose();
    }

    private int SearchCharGlyphIndex()
    {
      character = (char) (math.min(char.MaxValue, (int) character));
      return fontCurve.SearchGlyhIndex(character);
    }
  }
}