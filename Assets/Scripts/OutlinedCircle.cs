using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class OutlinedCircle : MonoBehaviour {

	private LineRenderer lineRenderer;
	public float radius = 5.0f;
	private List<Vector3> points = new List<Vector3>();
	private List<Vector3> activePoints = new List<Vector3>();

	private MeshFilter meshFilter;
	private Mesh mesh;
	private new MeshRenderer renderer;
	void Awake () {
		meshFilter = GetComponent<MeshFilter>();
		renderer = GetComponent<MeshRenderer>();
		mesh = new Mesh();
		mesh.name = "View Mesh";
		//meshFilter.mesh = mesh;
		mesh.MarkDynamic();
	}
	public void Render(float alpha=1.0f) {
		if (radius <= 2) return;
		this.lineRenderer = GetComponent<LineRenderer>();	
		int iters = (int) radius;
		lineRenderer.positionCount = iters;
		var col = lineRenderer.endColor;
		lineRenderer.material.color = new Color(col.r, col.g, col.b, alpha);
		Vector3[] points =  new Vector3[iters];
		for (int i = 0; i < iters; i++) {
			float angle = (float) (i * 360 / iters) * Mathf.Deg2Rad;
			var p = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)).normalized * radius;
			this.lineRenderer.SetPosition(i, p);
			points[i] = p;
		}
		int vertexCount = points.Length + 1;
		Vector3[] vertices = new Vector3[vertexCount];
		int[] triangles = new int[(vertexCount - 2) * 3];

		vertices[0] = Vector3.zero;
		for (int i = 0; i < vertexCount - 1; i++) {
			vertices[i + 1] = points[i];
			if (i < vertexCount - 2) {
				triangles[i * 3] = 0;
				triangles[i * 3 + 1] = i + 1;
				triangles[i * 3 + 2] = i + 2;
			}
		}

		mesh.Clear();
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.RecalculateNormals();
		renderer.material.color = new Color(renderer.material.color.r, renderer.material.color.g, renderer.material.color.b, alpha);

		
	}
}
