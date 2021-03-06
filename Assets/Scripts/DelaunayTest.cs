using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using Voxell.GPUVectorGraphics;
using Voxell.GPUVectorGraphics.Font;
using Voxell.Inspector;

public class DelaunayTest : MonoBehaviour
{
  public FontCurve fontCurve;
  public Transform[] transforms;
  public char character;
  public FontTest fontTest;
  public int glyphIdx;
  public float2 maxRect;
  public float2 minRect;

  [InspectOnly] public Mesh mesh;
  public MeshFilter meshFilter;
  public int highlightTriangle;
  public int highlightVertex;
  public float debugSize = 0.02f;

  [Button]
  private void TransformTest()
  {
    if (transforms == null || transforms.Length == 0) return;
    float2[] points = new float2[transforms.Length];
    mesh.Clear();

    int pointCount = points.Length;
    for (int p=0; p < pointCount; p++)
    {
      float3 pos = transforms[p].localPosition;
      points[p] = new float2(pos.x, pos.y);
    }

    NativeArray<float2> na_points;
    NativeList<int> na_triangles;
    JobHandle jobHandle = CDT.Triangulate(
      minRect, maxRect, in points, out na_points, out na_triangles
    );
    jobHandle.Complete();

    int vertexCount = na_points.Length;
    NativeArray<float3> na_vertices = new NativeArray<float3>(vertexCount, Allocator.Temp);
    for (int v=0; v < vertexCount; v++) na_vertices[v] = new float3(na_points[v], 0.0f);
    mesh.SetVertices<float3>(na_vertices);
    mesh.SetIndices<int>(na_triangles, MeshTopology.Triangles, 0);

    meshFilter.sharedMesh = mesh;

    na_points.Dispose();
    na_triangles.Dispose();
    na_vertices.Dispose();
  }

  [Button]
  private void DTTest()
  {
    if (fontCurve == null) return;
    // glyphIdx = fontCurve.SearchGlyhIndex(character);
    Glyph glyph = fontCurve.Glyphs[glyphIdx];
    fontTest.glyphIdx = glyphIdx;

    int contourCount = glyph.contours.Length;
    List<float2> points = new List<float2>();

    maxRect = glyph.maxRect;
    minRect = glyph.minRect;
    for (int c=0; c < contourCount; c++)
    {
      QuadraticContour glyphContour = glyph.contours[c];
      int segmentCount = glyphContour.segments.Length;

      for (int s=0; s < segmentCount; s++)
        points.Add(glyphContour.segments[s].p0);
    }

    if (mesh == null) mesh = new Mesh();
    mesh.Clear();

    NativeArray<float2> na_points;
    NativeList<int> na_triangles;
    JobHandle jobHandle = CDT.Triangulate(
      minRect, maxRect, points.ToArray(), out na_points, out na_triangles
    );
    jobHandle.Complete();

    int vertexCount = na_points.Length;
    NativeArray<float3> na_vertices = new NativeArray<float3>(vertexCount, Allocator.Temp);
    for (int v=0; v < vertexCount; v++) na_vertices[v] = new float3(na_points[v], 0.0f);
    mesh.SetVertices<float3>(na_vertices);
    mesh.SetIndices<int>(na_triangles, MeshTopology.Triangles, 0);
    mesh.RecalculateNormals();

    meshFilter.sharedMesh = mesh;

    na_points.Dispose();
    na_triangles.Dispose();
    na_vertices.Dispose();
  }

  [Button]
  private void CDTTest()
  {
    if (fontCurve == null) return;
    // glyphIdx = fontCurve.SearchGlyhIndex(character);
    Glyph glyph = fontCurve.Glyphs[glyphIdx];
    fontTest.glyphIdx = glyphIdx;

    int contourCount = glyph.contours.Length;
    if (contourCount == 0) return;

    List<float2> points = new List<float2>();
    List<CDT.ContourPoint> contours = new List<CDT.ContourPoint>();

    maxRect = glyph.maxRect;
    minRect = glyph.minRect;
    int contourStart = 0;
    for (int c=0; c < contourCount; c++)
    {
      QuadraticContour glyphContour = glyph.contours[c];
      int segmentCount = glyphContour.segments.Length;

      for (int s=0; s < segmentCount; s++)
      {
        points.Add(glyphContour.segments[s].p0);
        contours.Add(new CDT.ContourPoint(contourStart+s, c));
      }
      contours.Add(new CDT.ContourPoint(contourStart, c));
      contourStart += segmentCount;
    }

    if (mesh == null) mesh = new Mesh();
    mesh.Clear();

    NativeArray<float2> na_points;
    NativeList<int> na_triangles;
    NativeArray<CDT.ContourPoint> na_contours;
    JobHandle jobHandle = CDT.ConstraintTriangulate(
      minRect, maxRect, points.ToArray(), contours.ToArray(),
      out na_points, out na_triangles, out na_contours
    );
    jobHandle.Complete();

    int vertexCount = na_points.Length;
    NativeArray<float3> na_vertices = new NativeArray<float3>(vertexCount, Allocator.Temp);
    for (int v=0; v < vertexCount; v++) na_vertices[v] = new float3(na_points[v], 0.0f);
    mesh.SetVertices<float3>(na_vertices);
    mesh.SetIndices<int>(na_triangles, MeshTopology.Triangles, 0);
    mesh.RecalculateNormals();

    meshFilter.sharedMesh = mesh;

    na_points.Dispose();
    na_triangles.Dispose();
    na_vertices.Dispose();
    na_contours.Dispose();
  }

  [Button]
  private void NextDT()
  {
    SearchCharGlyphIndex(1);
    DTTest();
  }

  [Button]
  private void PrevDT()
  {
    SearchCharGlyphIndex(-1);
    DTTest();
  }

  [Button]
  private void NextCDT()
  {
    SearchCharGlyphIndex(1);
    CDTTest();
  }

  [Button]
  private void PrevCDT()
  {
    SearchCharGlyphIndex(-1);
    CDTTest();
  }

  private void SearchCharGlyphIndex(int valueChange)
  {
    character = (char)(math.min(char.MaxValue, character + valueChange));
    glyphIdx = fontCurve.SearchGlyhIndex(character);
  }

  private void OnDrawGizmos()
  {
    if (mesh == null) return;
    Gizmos.color = Color.red;
    int[] triangles = mesh.triangles;
    Vector3[] vertices = mesh.vertices;
    int tIdx = highlightTriangle*3;

    if (tIdx < triangles.Length && tIdx > 0)
    {
      Gizmos.DrawLine(transform.position + vertices[triangles[tIdx]], transform.position + vertices[triangles[tIdx + 1]]);
      Gizmos.DrawLine(transform.position + vertices[triangles[tIdx + 1]], transform.position + vertices[triangles[tIdx + 2]]);
      Gizmos.DrawLine(transform.position + vertices[triangles[tIdx + 2]], transform.position + vertices[triangles[tIdx]]);
    }

    if (highlightVertex < vertices.Length && highlightVertex >= 0)
      Gizmos.DrawSphere(transform.position + vertices[highlightVertex], debugSize);
  }
}
