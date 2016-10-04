using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System;
using UniRx;
public class MultiSelectPanel : MyPanel
{
    private List<MultiSelectItem> items = new List<MultiSelectItem>();
    // Use this for initialization
    public Button ok;


    public IObservable<IEnumerable<MultiSelectItem>> OnSelectedItemsAsObservable()
    {
        return ok.OnClickAsObservable().Select(_ => items.Where(i => i.IsSelected()));
    }
    public void AddItems(IEnumerable<Tuple<string, System.Object, bool>> texts)
    {
        var prefab = Resources.Load("GUI/MultiSelectItem") as GameObject;

        var content = transform.Find("Scroll View").GetComponent<ScrollRect>().content;
        //remove items
        content.transform.DetachChildren();
        items.Clear();
        //add new ones
        items.AddRange(texts.Select((t) =>
        {
            var go = ((GameObject)GameObject.Instantiate(prefab));
            var item = go.GetComponent<MultiSelectItem>();
            item.SetLabelAndContent(t.Item1, t.Item2);
            item.Interactable=(t.Item3);
            return item;
        }));

        items.ForEach((item) => item.transform.SetParent(content, false));

    }



}
