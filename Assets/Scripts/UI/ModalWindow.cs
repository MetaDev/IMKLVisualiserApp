using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
[RequireComponent (typeof (Canvas))]
public class ModalWindow : MonoBehaviour {
	public Button Close;
	public Text Message;
	// Use this for initialization
	void Start () {
		Close.OnClickAsObservable().Subscribe(_=> gameObject.SetActive(false));
	}
	

}
