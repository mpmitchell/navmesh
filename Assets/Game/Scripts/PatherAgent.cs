using UnityEngine;

public class PatherAgent : MonoBehaviour {

	[SerializeField] GameObject mesh;
	[SerializeField] GameObject end;
	[SerializeField] float speed;

	Vector3[] path;
	int currentWaypoint;
	float distance;
	float time;

	void Start() {
		Reset();
	}

	public void Reset() {
		Pather pather = mesh.GetComponent<Pather>();

		if (pather != null) {
			path = pather.getPath(gameObject, end);
			currentWaypoint = 0;
			distance = 0;
			time = 0;

			if (path != null && path.Length > 1) {
				distance = Vector3.Distance(path[currentWaypoint], path[currentWaypoint + 1]);
				transform.LookAt(path[currentWaypoint + 1]);
			}
		}
	}

	void Update() {
		if (path != null && currentWaypoint + 1 < path.Length) {
			time += (speed * Time.deltaTime) / distance;
			transform.position = Vector3.Lerp(path[currentWaypoint], path[currentWaypoint + 1], time);

			if (transform.position == path[currentWaypoint + 1]) {
				currentWaypoint++;
				time = 0;

				if (currentWaypoint + 1 != path.Length) {
					distance = Vector3.Distance(path[currentWaypoint], path[currentWaypoint + 1]);
					transform.LookAt(path[currentWaypoint + 1]);
				}
			}
		}
	}
}
