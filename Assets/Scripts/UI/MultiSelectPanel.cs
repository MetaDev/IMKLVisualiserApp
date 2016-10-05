using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using UniRx;
public class MultiSelectPanel : MonoBehaviour
{
    private List<MultiSelectItem> items = new List<MultiSelectItem>();
    // Use this for initialization
    public Button ok;
    public ToggleGroup Group;

    public IObservable<IEnumerable<MultiSelectItem>> OnSelectedItemsAsObservable()
    {
        return ok.OnClickAsObservable().Select(_ => items.Where(i => i.IsSelected()));
    }
    public void AddItems(IEnumerable<Tuple<string, System.Object, bool>> texts)
    {
        var prefab = Resources.Load("GUI/MultiSelectItem") as GameObject;

        var content = transform.Find("Scroll View").GetComponent<ScrollRect>().content;
        //remove items, TODO delete them
        content.transform.DetachChildren();
        items.Clear();
        //add new ones
        //the togglegroup has to be created in superclass because Start() cannot be overridden

        items.AddRange(texts.Select((t) =>
        {
            var go = ((GameObject)GameObject.Instantiate(prefab));
            var item = go.GetComponent<MultiSelectItem>();
            item.Init(t.Item1, t.Item2, t.Item3, Group);
            return item;
        }));

        items.ForEach((item) => item.transform.SetParent(content, false));

    }



}
