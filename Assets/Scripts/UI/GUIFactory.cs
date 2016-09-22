using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUIFactory: MonoBehaviour {
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	public static MultiSelectPanel CreateMultiSelectPanel(Vector2 screenPos){
		var prefab = Resources.Load("GUI/MultiSelectPanel") as GameObject;
		var go = GameObject.Instantiate(prefab);
		go.GetComponent<RectTransform>().position=screenPos;
		return go.GetComponent<MultiSelectPanel>();
	}

}
