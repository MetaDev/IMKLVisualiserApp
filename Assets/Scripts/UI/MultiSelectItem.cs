using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
public class MultiSelectItem : MonoBehaviour
{
    string content;
    string label;
	public Tuple<string,string> GetText(){
		return Tuple.Create(label,content);
	}
    public void SetText(string label, string content){
        base.transform.FindChild("Label").GetComponent<Text>().text = label;
        this.label = label;
        this.content=content;
    }

}
