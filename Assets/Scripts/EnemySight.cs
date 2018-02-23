using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class EnemySight : MonoBehaviour {
	[Range(0, 360)]
	public float viewAngle = 180;

	public float viewRadius = 5;
	bool playerInSight = false;
	Vector2 lastSighting = new Vector2();
	Vector2 prevSighting = new Vector2();
	Vector2 globalLastSighting = new Vector2();
	private Player player;
	Enemy controller;

	public float meshResolution = 1;
	public int edgeResolveIterations;
	public float edgeDstThreshold;
	private MeshFilter viewMeshFilter;
	private Mesh viewMesh;

	public delegate void Callback();
	public event Callback OnPlayerSeen;

	[HideInInspector]
	public List<SuspiciousObject> objectsInView = new List<SuspiciousObject>();
	public LayerMask layerMask;
	public LayerMask obstacleMask;
	void Start() {
		player = GameObject.Find("Player").GetComponent<Player>();
		controller = this.transform.parent.GetComponent<Enemy>();
		viewMeshFilter = GetComponent<MeshFilter>();
		viewMesh = new Mesh();
		viewMesh.name = "View Mesh";
		viewMeshFilter.mesh = viewMesh;
		StartCoroutine("FindTargetsWithDelay", .2f);
	}
	IEnumerator FindTargetsWithDelay(float delay) {
		while (true) {
			yield return new WaitForSeconds(delay);
			FindVisibleTargets();
		}
	}
	void FindVisibleTargets() {
		objectsInView.Clear();
		Collider2D[] targetsInViewRadius = Physics2D.OverlapCircleAll(transform.position, viewRadius, layerMask);

		for (int i = 0; i < targetsInViewRadius.Length; i++) {
			Transform target = targetsInViewRadius[i].transform;
			Vector3 dirToTarget = (target.position - transform.position).normalized;
			var angle = Vector2.Angle(Vector2.right * controller.facingDirection, dirToTarget);
			if (angle < viewAngle/2) {
				float dstToTarget = Vector2.Distance(transform.position, target.position);
				if (!Physics2D.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask)) {
					if (target.GetComponent<Player>() != null) {
						OnPlayerSeen();
					}
				}
			}
		}
	}

	// Update is called once per frame
	void Update() {
		if (globalLastSighting != prevSighting) {
			lastSighting = globalLastSighting;
		}
		DrawFieldOfView();
	}


	public void DrawFieldOfView() {
		int stepCount = Mathf.RoundToInt(viewAngle * meshResolution);
		float stepAngle = viewAngle / stepCount;
		List<Vector3> viewPoints = new List<Vector3>();
		ViewCastInfo oldViewCast = new ViewCastInfo();

		for (var i = 0; i < stepCount; i++) {
			float angle = stepAngle * i - viewAngle / 2 + 90 * controller.facingDirection;
			ViewCastInfo newViewCast = CastView(angle);

			if (i > 0) {
				bool edgeDstThresholdExceeded = Mathf.Abs(oldViewCast.dst - newViewCast.dst) > edgeDstThreshold;
				if (oldViewCast.hit != newViewCast.hit || (oldViewCast.hit && newViewCast.hit && edgeDstThresholdExceeded)) {
					EdgeInfo edge = FindEdge(oldViewCast, newViewCast);
					if (edge.pointA != Vector2.zero) {
						viewPoints.Add(edge.pointA);
					}
					if (edge.pointB != Vector2.zero) {
						viewPoints.Add(edge.pointB);
					}
				}
			}


			viewPoints.Add(newViewCast.point);
			oldViewCast = newViewCast;
		}

		int vertexCount = viewPoints.Count + 1;
		Vector3[] vertices = new Vector3[vertexCount];
		int[] triangles = new int[(vertexCount - 2) * 3];

		vertices[0] = Vector3.zero;
		for (int i = 0; i < vertexCount - 1; i++) {
			vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i]);
			if (i < vertexCount - 2) {
				triangles[i * 3] = 0;
				triangles[i * 3 + 1] = i + 1;
				triangles[i * 3 + 2] = i + 2;
			}
		}

		viewMesh.Clear();
		viewMesh.vertices = vertices;
		viewMesh.triangles = triangles;
		viewMesh.RecalculateNormals();
	}

	public ViewCastInfo CastView(float angle) {
		Vector2 dir = DirFromAngle(angle);
		RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, viewRadius, obstacleMask);

		if (hit) {
			return new ViewCastInfo(true, hit.point, hit.distance, angle);
		}
		else {
			return new ViewCastInfo(false, (Vector2) transform.position + dir * viewRadius, viewRadius, angle);
		}
	}
	EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast) {
		float minAngle = minViewCast.angle;
		float maxAngle = maxViewCast.angle;
		Vector2 minPoint = Vector2.zero;
		Vector2 maxPoint = Vector2.zero;

		for (int i = 0; i < edgeResolveIterations; i++) {
			float angle = (minAngle + maxAngle) / 2;
			ViewCastInfo newViewCast = CastView(angle);

			bool edgeDstThresholdExceeded = Mathf.Abs(minViewCast.dst - newViewCast.dst) > edgeDstThreshold;
			if (newViewCast.hit == minViewCast.hit && !edgeDstThresholdExceeded) {
				minAngle = angle;
				minPoint = newViewCast.point;
			}
			else {
				maxAngle = angle;
				maxPoint = newViewCast.point;
			}
		}

		return new EdgeInfo(minPoint, maxPoint);
	}
	public Vector2 DirFromAngle(float angle) {
		return new Vector2(Mathf.Sin(angle * Mathf.Deg2Rad) , -Mathf.Cos(angle * Mathf.Deg2Rad));
	}

	public struct ViewCastInfo {
		public bool hit;
		public Vector2 point;
		public float dst;
		public float angle;

		public ViewCastInfo(bool _hit, Vector2 _point, float _dst, float _angle) {
			hit = _hit;
			point = _point;
			dst = _dst;
			angle = _angle;
		}
	}
	public struct EdgeInfo {
		public Vector2 pointA;
		public Vector2 pointB;

		public EdgeInfo(Vector2 _pointA, Vector2 _pointB) {
			pointA = _pointA;
			pointB = _pointB;
		}
	}

	private void OnTriggerEnter2D(Collider2D collision) {
	}
}
