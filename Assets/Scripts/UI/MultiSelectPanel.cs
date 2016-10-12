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

    public RectTransform contentView
    {
        get { return transform.Find("Scroll View").GetComponent<ScrollRect>().content; }
    }


    public IObservable<IEnumerable<MultiSelectItem>> OnSelectedItemsAsObservable()
    {
        return ok.OnClickAsObservable().Select(_ => itemUIs.Where(i => i.IsSelected()));
    }
    MultiSelectItem _InitItemUI()
    {
        var go = Instantiate(Resources.Load("GUI/MultiSelectItemPrefab")) as GameObject;
        go.SetActive(true);
        var itemUI = go.GetComponent<MultiSelectItem>();
        itemUI.transform.SetParent(contentView, false);
        return itemUI;
    }
    MultiSelectItem InitItemUI(string label, object content, bool interactable)
    {
        var itemUI = _InitItemUI();
        itemUI.Init(label,content,interactable, Group);
        return itemUI;
    }
 
    public void AddItems(IEnumerable<Tuple<string, System.Object, bool>> items)
    {
        ClearItemUIs();
        //add new ones
        //the togglegroup has to be created in superclass because Start() cannot be overridden
        itemUIs = items.Select(item => InitItemUI(item.Item1,item.Item2,item.Item3)).ToList();

    }
    public void ClearItemUIs()
    {
        var content = transform.Find("Scroll View").GetComponent<ScrollRect>().content;
        //remove items, and delete them
        content.transform.DetachChildren();
        itemUIs.ForEach(iUI => iUI.Destroy());
    }
    public void AddItems(IEnumerable<Tuple<string, System.Object>> items)
    {
        //add new ones
        //the togglegroup has to be created in superclass because Start() cannot be overridden
        itemUIs = items.Select(item => InitItemUI(item.Item1,item.Item2,true)).ToList();

    }



}
