using UnityEngine;

public class Goal : MonoBehaviour {

	[SerializeField] PatherAgent agent;

	void Update () {
		if (Input.GetMouseButtonDown(1)) {
			Vector3 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			position.z = 0;
			transform.position = position;
			agent.Reset();
		}
	}
}
