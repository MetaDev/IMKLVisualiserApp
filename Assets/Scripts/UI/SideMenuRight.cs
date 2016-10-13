using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System.Linq;
using IMKL_Logic;

public class SideMenuRight : MonoBehaviour
{

    public Toggle openClose;
    public DrawElementPanel ElementPanel;
    public float DrawElementPanelWidth;
    // Use this for initialization
    void Start()
    {
        //skip first firing of the observable
        openClose.OnValueChangedAsObservable().Skip(1).Subscribe(isOn =>
        {
            //move the menu to the right on close
            transform.localPosition += (isOn ? Vector3.left : Vector3.right) *DrawElementPanelWidth;
            ElementPanel.gameObject.SetActive(isOn);
        });

    }



}
