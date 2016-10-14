using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent (typeof (Camera))]
public class FitToMapAndScreen : MonoBehaviour {

	// Use this for initialization
	void Start () {
		float mapWidth = OnlineMaps.instance.width;
		float mapHeight = OnlineMaps.instance.height;
		Camera.main.orthographicSize =  mapHeight/ 2.0f;
		transform.position=new Vector3(mapWidth/2,-mapHeight/2,-100);

	}
	
	
}
