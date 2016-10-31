using UnityEngine;

public class Node : MonoBehaviour {

	public bool on = true;
	public GameObject[] vertices;

	[SerializeField] Material enabled;
	[SerializeField] Material disabled;

	Renderer renderer;

	void Start() {
		renderer = GetComponent<Renderer>();
	}

	void OnMouseDown() {
		on = !on;

		if (on) {
			renderer.material = enabled;
		} else {
			renderer.material = disabled;
		}
	}
}
