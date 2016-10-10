using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using UniRx;
using MoreLinq;
public class MultiSelectPanel : MonoBehaviour
{
    private List<MultiSelectItem> itemUIs = new List<MultiSelectItem>();
    // Use this for initialization
    public Button ok;
    public ToggleGroup Group;
    Transform contentView;
    GameObject ItemUI;
    void Start()
    {
        ItemUI = Resources.Load("GUI/MultiSelectItem") as GameObject;
        contentView = transform.Find("Scroll View").GetComponent<ScrollRect>().content;
    }

    public IObservable<IEnumerable<MultiSelectItem>> OnSelectedItemsAsObservable()
    {
        return ok.OnClickAsObservable().Select(_ => itemUIs.Where(i => i.IsSelected()));
    }
    MultiSelectItem InitItem()
    {
        var go = GameObject.Instantiate(ItemUI);
        var itemUI = go.GetComponent<MultiSelectItem>();
        itemUI.transform.SetParent(contentView, false);
        itemUIs.Add(itemUI);
        return itemUI;
    }
    public void AddItem(Tuple<string, System.Object, bool> item)
    {
        var itemUI = InitItem();
        itemUI.Init(item.Item1, item.Item2, item.Item3, Group);
    }
    public void AddItem(Tuple<string, System.Object> item)
    {
        var itemUI = InitItem();
        itemUI.Init(item.Item1, item.Item2,group: Group);
    }
    public void AddItems(IEnumerable<Tuple<string, System.Object, bool>> items)
    {
        ClearItemUIs();
        //add new ones
        //the togglegroup has to be created in superclass because Start() cannot be overridden
        items.ForEach(item => AddItem(item));

    }
    public void ClearItemUIs()
    {
        var content = transform.Find("Scroll View").GetComponent<ScrollRect>().content;
        //remove items, and delete them
        content.transform.DetachChildren();
        // itemUIs.ForEach(iUI => Destroy(iUI));
        itemUIs.Clear();
    }
    public void AddItems(IEnumerable<Tuple<string, System.Object>> items)
    {
        ClearItemUIs();
        //add new ones
        //the togglegroup has to be created in superclass because Start() cannot be overridden
        items.ForEach(item => AddItem(item));

    }



}
