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

    // Use this for initialization
    void Start()
    {
        openClose.OnValueChangedAsObservable().Subscribe(isOn => ElementPanel.gameObject.SetActive(isOn));

    }



}
