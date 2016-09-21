using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System;
using UniRx;
public class MultiSelectPanel : MonoBehaviour
{
    private List<MultiSelectItem> items = new List<MultiSelectItem>();
    // Use this for initialization
    public Button ok;
    void Start()
    {
        this.transform.SetParent(GameObject.Find("Canvas").transform, false);
		Debug.Log("brak");
    }
    public void Show(Vector2 screenPos)
    {
        this.GetComponent<RectTransform>().position = screenPos;
        gameObject.SetActive(true);
    }
    public void Close()
    {
        gameObject.SetActive(false);
    }
	public delegate void MethodOnSelectedItems(IEnumerable<MultiSelectItem> message);
    public void SetMethodOnSelectedItems(MethodOnSelectedItems methodOnSelectedItems)
    {
        ok.OnClickAsObservable().Subscribe(_=> methodOnSelectedItems(items));
    }
    public void AddItems(IEnumerable<Tuple<string,string>> texts)
    {
        var prefab = Resources.Load("GUI/MultiSelectItem") as GameObject;

        var content = transform.Find("Scroll View").GetComponent<ScrollRect>().content;
        Debug.Log(content);
        //remove items
        content.transform.DetachChildren();
        items.Clear();
        //add new ones
        items.AddRange(texts.Select((t) =>
        {
            var go = ((GameObject)GameObject.Instantiate(prefab));
            var item = go.GetComponent<MultiSelectItem>();
            item.SetText(t.Item1,t.Item2);
            return item;
        }));

        items.ForEach((item) => item.transform.SetParent(content, false));

    }



}
