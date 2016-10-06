using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
public class MultiSelectItem : MonoBehaviour
{
    public object content
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
    bool Interactable;
    ToggleGroup Group;
    public void Init(string label, object content,bool interactable=true,ToggleGroup group=null)
    {
        base.transform.FindChild("Label").GetComponent<Text>().text = label;
        this.label = label;
        this.content = content;
        this.Interactable=interactable;
        this.Group=group;
    }
    public bool IsSelected()
    {
        return toggle.isOn;
    }
    void Start()
    {
        toggle = GetComponent<Toggle>();
        
        toggle.interactable = Interactable;
        if (Group!=null){
            toggle.group=Group;
        }

    }
}
