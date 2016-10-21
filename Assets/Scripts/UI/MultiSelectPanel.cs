using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.UI;
using UniRx;
using MoreLinq;
public class MultiSelectPanel : MonoBehaviour
{
    public List<MultiSelectItem> itemUIs = new List<MultiSelectItem>();
    // Use this for initialization
    public Button[] Buttons;
    public ScrollRect Scroll;

    public RectTransform contentView
    {
        get { return Scroll.content.GetComponent<RectTransform>(); }
    }
    public IEnumerable<object> GetSelectedItemsContent()
    {
        return itemUIs.Where(i => i.IsSelected()).Select(items => items.content);
    }
    public IObservable<IEnumerable<MultiSelectItem>> OnSelectedItemsAsObservable(int buttonIndex)
    {
        if (Buttons[buttonIndex] != null)
        {
            return Buttons[buttonIndex].OnClickAsObservable().Select(_ => itemUIs.Where(i => i.IsSelected()));
        }
        else
        {
            throw new MissingComponentException("Not possible to observe multiselect panel selected items without OK button.");
        }
    }
    IObservable<MultiSelectItem> ObsItemClicked = Observable.Empty<MultiSelectItem>();
    IDisposable obsItemClickedDisp;
    public IObservable<MultiSelectItem> OnItemClicked()
    {
        return ObsItemClicked;
    }
    MultiSelectItem _InitItemUI()
    {
        var go = Instantiate(Resources.Load("GUI/MultiSelectItemPrefab")) as GameObject;
        go.SetActive(true);
        var itemUI = go.GetComponent<MultiSelectItem>();
        itemUI.transform.SetParent(contentView, false);
        return itemUI;
    }
    MultiSelectItem InitItemUI(string label, object content, bool interactable = true,bool toggle=true)
    {
        var itemUI = _InitItemUI();
        itemUI.Init(label, content, interactable,toggle);
        return itemUI;
    }

    public List<MultiSelectItem> AddItems(IEnumerable<Tuple<string, System.Object, bool>> items,bool toggle=true)
    {
        ClearItemUIs();
        //add new ones
        itemUIs = items.Select(item => InitItemUI(item.Item1, item.Item2, item.Item3,toggle)).ToList();
        ObsItemClicked = itemUIs.Select(itemUI => itemUI.OnClickSelf()).Merge();
        return itemUIs;

    }
   
    public void ClearItemUIs()
    {
        //remove items, and delete them
        contentView.DetachChildren();
        itemUIs.ForEach(iUI => iUI.Destroy());
    }



}
