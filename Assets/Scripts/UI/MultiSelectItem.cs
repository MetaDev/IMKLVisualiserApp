﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
public class MultiSelectItem : MonoBehaviour
{
    public System.Object content
    {
        get;
        private set;
    }
    public string label
    {
        get;
        private set;
    }
    Toggle toggle;

    public void SetLabelAndContent(string label, System.Object content)
    {
        base.transform.FindChild("Label").GetComponent<Text>().text = label;
        this.label = label;
        this.content = content;
    }
    public bool IsSelected()
    {
        return toggle.isOn;
    }
    void Start()
    {
        toggle = GetComponent<Toggle>();
    }
}
