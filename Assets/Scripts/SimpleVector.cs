using UnityEngine;

public class SimpleVector : MonoBehaviour
{
  public MeshFilter meshFilter;

  public Vector3[] vertices = new Vector3[3];
  public Vector3[] uvs = new Vector3[3];
  public Mesh mesh;

  private void Start()
  {
    mesh = new Mesh();
    meshFilter.sharedMesh = mesh;
  }

  private void Update()
  {
    mesh.SetVertices(vertices);
    mesh.SetUVs(0, uvs);
    mesh.SetTriangles(new int[3]{0, 1, 2}, 0);
  }
}
