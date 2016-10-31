using UnityEngine;
using System.Collections.Generic;

public class Pather : MonoBehaviour {

	public GameObject mesh;
	// node, vertices
	Dictionary<GameObject, List<GameObject>> nodeList = new Dictionary<GameObject, List<GameObject>>();
	// node, connected nodes, shared vertices
	Dictionary<GameObject, Dictionary<GameObject, List<GameObject>>> adjacencyList = new Dictionary<GameObject, Dictionary<GameObject, List<GameObject>>>();

	private void BuildListFromGO() {
		nodeList = new Dictionary<GameObject, List<GameObject>>();
		adjacencyList = new Dictionary<GameObject, Dictionary<GameObject, List<GameObject>>>();

		// Build list of nodes with vertices
		foreach (Transform trans in mesh.transform) {
			GameObject gobject = trans.gameObject;
			Node n = gobject.GetComponent<Node>();

			if (n != null && n.on) {
				nodeList.Add(gobject, new List<GameObject>(n.vertices));
			}
		}

		// Compare nodes for shared edges and build adjacency list
		foreach (KeyValuePair<GameObject, List<GameObject>> node in nodeList) {
			Dictionary<GameObject, List<GameObject>> connectedNodes = new Dictionary<GameObject, List<GameObject>>();

			foreach (KeyValuePair<GameObject, List<GameObject>> otherNode in nodeList) {
				if (otherNode.Equals(node)) {
					continue;
				}

				List<GameObject> sharedVertices = new List<GameObject>();
				foreach (GameObject vertex in node.Value) {
					if (otherNode.Value.Contains(vertex)) sharedVertices.Add(vertex);
				}

				if (sharedVertices.Count == 2) {
					connectedNodes.Add(otherNode.Key, sharedVertices);
				}
			}

			adjacencyList.Add(node.Key, connectedNodes);
		}
	}

	public Vector3[] getPath(GameObject start, GameObject end) {
		BuildListFromGO();

		bool flag = false;
		GameObject currentNode = getContainingNode(start);
		GameObject endNode = getContainingNode(end);

		if (currentNode == null || endNode == null) {
			return null;
		}

		List<GameObject> closedSet = new List<GameObject>();
		List<GameObject> openSet = new List<GameObject>();
		Dictionary<GameObject, GameObject> parent = new Dictionary<GameObject, GameObject>();
		Dictionary<GameObject, Dictionary<char, float>> scores = new Dictionary<GameObject, Dictionary<char, float>>();

		openSet.Add(currentNode);
		parent.Add(currentNode, null);
		scores.Add(currentNode, new Dictionary<char, float> {
			{ 'g', 0 }, { 'f', hScore(currentNode.transform, end.transform) }
		});

		while (openSet.Count > 0) {
			openSet = sortOpenSet(openSet, scores);
			GameObject current = openSet[0];

			if (current == endNode) {
				flag = true;
				break;
			}

			openSet.Remove(current);
			closedSet.Add(current);

			foreach (GameObject neighbour in adjacencyList[current].Keys) {
				if (closedSet.Contains(neighbour)) {
					continue;
				}

				float gScore = scores[current]['g'] + distanceBetween(current.transform, neighbour.transform);

				if (!openSet.Contains(neighbour) || gScore <= scores[neighbour]['g']) {
					parent.Add(neighbour, current);
					scores.Add(neighbour, new Dictionary<char, float> {
						{ 'g', gScore }, { 'f', gScore + hScore(neighbour.transform, end.transform) }
					});

					if (!openSet.Contains(neighbour)) {
						openSet.Add(neighbour);
					}
				}
			}
		}

		if (!flag) {
			return null;
		} else {
			return buildPath(start, end, endNode, parent);
		}
	}

	private GameObject getContainingNode(GameObject point) {
		foreach (KeyValuePair<GameObject, List<GameObject>> node in nodeList) {
			List<Vector3> vertices = new List<Vector3>();

			foreach (GameObject vertex in node.Value) {
				vertices.Add(new Vector3(vertex.transform.position.x, vertex.transform.position.y, vertex.transform.position.z));
			}

			// The below code is from http://www.blackpawn.com/texts/pointinpoly/default.html
			// It uses barycentric coordinates to determine if a point is inside a triangle.
			Vector2[] vectors = new Vector2[] {
				vertices[2] - vertices[0],
				vertices[1] - vertices[0],
				point.transform.position - vertices[0]
			};

			float[] scalarProducts = new float[] {
				Vector2.Dot(vectors[0], vectors[0]),
				Vector2.Dot(vectors[0], vectors[1]),
				Vector2.Dot(vectors[0], vectors[2]),
				Vector2.Dot(vectors[1], vectors[1]),
				Vector2.Dot(vectors[1], vectors[2])
			};

			float invDemon = 1 / (scalarProducts[0] * scalarProducts[3] - scalarProducts[1] * scalarProducts[1]);
			float u = (scalarProducts[3] * scalarProducts[2] - scalarProducts[1] * scalarProducts[4]) * invDemon;
			float v = (scalarProducts[0] * scalarProducts[4] - scalarProducts[1] * scalarProducts[2]) * invDemon;

			if ((u >= 0) && (v >= 0) && (u + v < 1)) {
				return node.Key;
			}
		}

		return null;
	}

	private float hScore(Transform start, Transform end) {
		float dx = Mathf.Abs((start.position.x - start.GetComponent<Renderer>().bounds.size.x / 2) - (end.position.x - end.GetComponent<Renderer>().bounds.size.x / 2));
		float dy = Mathf.Abs((start.position.y - start.GetComponent<Renderer>().bounds.size.y / 2) - (end.position.y - end.GetComponent<Renderer>().bounds.size.y / 2));

		if (dx > dy) {
			return 1.4f * dy + (dx - dy);
		} else {
			return 1.4f * dx + (dy - dx);
		}
	}

	private List<GameObject> sortOpenSet(List<GameObject> list, Dictionary<GameObject, Dictionary<char, float>> scores) {
		if (list.Count == 1) {
			return list;
		}

		bool swap = false;
		do {
			swap = false;
			for (int i = 0; i < list.Count - 1; i++) {
				if (scores[list[i]]['f'] > scores[list[i + 1]]['f']) {
					GameObject tmp = list[i];
					list[i] = list[i + 1];
					list[i + 1] = tmp;
					swap = true;
				}
			}
		} while (swap);

		return list;
	}

	private float distanceBetween(Transform start, Transform end) {
		float dx = Mathf.Abs((start.position.x - start.GetComponent<Renderer>().bounds.size.x / 2) - (end.position.x - end.GetComponent<Renderer>().bounds.size.x / 2));
		float dy = Mathf.Abs((start.position.y - start.GetComponent<Renderer>().bounds.size.y / 2) - (end.position.y - end.GetComponent<Renderer>().bounds.size.y / 2));

		return Mathf.Sqrt((dx * dx) + (dy * dy));
	}

	private Vector3[] buildPath(GameObject start, GameObject end, GameObject endNode, Dictionary<GameObject, GameObject> parent) {
		List<Vector3> path = new List<Vector3>();
		path.Add(start.transform.position);

		// Make new list and add nodes (the midpoint of shared edge)
		List<Vector3> tmpList = new List<Vector3>();
		GameObject node = endNode;
		while (parent[node] != null) {
			tmpList.Add(getWaypoint(node, parent[node]));
			node = parent[node];
		}

		tmpList.Reverse();
		path.AddRange(tmpList);
		path.Add(end.transform.position);

		return path.ToArray();
	}

	private Vector3 getWaypoint(GameObject currentNode, GameObject nextNode) {
		Vector3 waypoint = new Vector3(0, 0, 0);
		int i = 0;

		foreach (GameObject vertex in adjacencyList[currentNode][nextNode]) {
			waypoint += vertex.transform.position;
			i++;
		}

		waypoint /= i;
		return waypoint;
	}
}
