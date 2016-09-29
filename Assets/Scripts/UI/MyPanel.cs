using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class MyPanel : MonoBehaviour {

	public Button close;
    void Start()
    {
        close.OnClickAsObservable().Subscribe(_=>gameObject.SetActive(false));
    }
	public void Show()
    {
        gameObject.SetActive(true);
    }
}
