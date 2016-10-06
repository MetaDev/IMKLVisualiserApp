using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
[RequireComponent (typeof (Canvas))]
public class ModalWindow : MonoBehaviour {
	public Button OK;
	public Text Message;
	// Use this for initialization
	void Start () {
		OK.OnClickAsObservable().Subscribe(_=> Close());
	}
	public void Show(string message,bool okButton){
		Message.text=message;
		OK.gameObject.SetActive(okButton);
		gameObject.SetActive(true);
	}
	public void Close(){
		gameObject.SetActive(false);
	}
	

}
