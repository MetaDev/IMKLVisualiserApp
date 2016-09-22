using System.Collections;
using System.Collections.Generic;
using IO;
using UnityEngine;
using System.Linq;
using UniRx;
using IMKL_logic;

public class Test : MonoBehaviour {

	// Use this for initialization
	void Start () {

		var panel =GUIFactory.CreateMultiSelectPanel();
		panel.AddItems(IMKLParser.GetAllXMLFiles().Select(f => Tuple.Create(f.Name,f.FullName)));
		panel.SetMethodOnSelectedItems((items)=> {
			IMKL_Geometry.Draw(IMKLParser.Parse(items.Select(i => i.GetText().Item2)));
		});
		// // Debug.Log(test.GetSelected());
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
