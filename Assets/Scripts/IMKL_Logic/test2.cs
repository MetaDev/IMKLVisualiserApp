using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test2 : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Debug.Log(GEO.LBToLL.LambertToLatLong(new Utility.Pos(30421.5675299011,197113.996062124 )));
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
