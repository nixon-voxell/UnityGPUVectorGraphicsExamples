using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Voxell.GPUVectorGraphics;

public class CubicBezierTest : MonoBehaviour
{
  public Transform[] positions = new Transform[4];
  public Mesh mesh;
  public MeshFilter meshFilter;

  private void Start()
  {
    mesh = new Mesh();
    meshFilter.sharedMesh = mesh;
  }

  private void Update()
  {
    float2 p0 = new float2(positions[0].position.x, positions[0].position.y);
    float2 p1 = new float2(positions[1].position.x, positions[1].position.y);
    float2 p2 = new float2(positions[2].position.x, positions[2].position.y);
    float2 p3 = new float2(positions[3].position.x, positions[3].position.y);

    int triangleCount = 9;
    int vertexCount = triangleCount * 3;

    NativeArray<float2> vertexArray = new NativeArray<float2>(vertexCount, Allocator.Temp);
    NativeArray<float3> vertex3Array = new NativeArray<float3>(vertexCount, Allocator.Temp);
    NativeArray<float3> coordsArray = new NativeArray<float3>(vertexCount, Allocator.Temp);

    NativeSlice<float2> vertexSlice = vertexArray.Slice(0);
    NativeSlice<float3> coordsSlice = coordsArray.Slice(0);

    int vertexStart = 0;
    int coordsStart = 0;
    CubicBezier.ComputeCubic(p0, p1, p2, p3, ref vertexStart, ref vertexSlice, ref coordsStart, ref coordsSlice);

    int[] triangles = new int[vertexCount];
    for (int v=0; v < vertexCount; v++)
    {
      triangles[v] = v;
      vertex3Array[v] = new float3(vertexArray[v], 0.0f);
    }

    mesh.SetVertices(vertex3Array);
    mesh.SetTriangles(triangles, 0);
    mesh.SetUVs(0, coordsArray);

    vertex3Array.Dispose();
    coordsArray.Dispose();
  }
}
