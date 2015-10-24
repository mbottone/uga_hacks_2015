using UnityEngine;
using System.Collections;

public class SpinningTire : MonoBehaviour {

	public float speed = 5f;
	
	// Update is called once per frame
	void Update () {
		transform.Rotate (speed, 0, 0, Space.Self);
	}
}
